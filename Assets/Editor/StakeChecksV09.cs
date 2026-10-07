using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ZeroToMillionaire
{
    public static class StakeChecksV09
    {
        sealed class Pattern : IRandomSource { int n; public int Next(int maximum) => n++ % maximum; }
        sealed class CashOnly : IRandomSource { public int Next(int maximum) => 0; }
        sealed class Store : IStateStore
        {
            public PlayerState State = new PlayerState(); public bool Fail; public int Writes;
            public bool Save(PlayerState candidate, out string error) { Writes++; error = Fail ? "Simulated save failure" : ""; if (Fail) return false; State = candidate.Clone(); return true; }
            public PlayerState Load(out string notice) { notice = ""; return State.Clone(); }
        }
        static int assertions;
        static void Check(bool ok, string message) { assertions++; if (!ok) throw new Exception("STAKE V09: " + message); }
        static PlayerState Collecting(int multiplier, int cash = 1000, int free = 0) { var s = new PlayerState { stakeMultiplier = multiplier, cash = cash, freeSpins = free }; s.tutorial.completed = true; return s; }
        public static string Run(IStateCodec codec, string directory)
        {
            assertions = 0;
            SpinResult baseline;
            var baseModel = new GameModel(Collecting(1));
            Check(baseModel.TrySpin(new Pattern(), out baseline), "Baseline spin");
            for (int stake = 1; stake <= 3; stake++)
            {
                var model = new GameModel(Collecting(stake)); SpinResult result;
                Check(model.NextSpinCost == 5 * stake && model.NextRewardMultiplier == stake, "Paid preview");
                Check(model.TrySpin(new Pattern(), out result), "Paid accepted");
                Check(result.SelectedStake == stake && result.RewardMultiplier == stake && !result.Free, "Captured immutable stake");
                Check(result.Grid.SequenceEqual(baseline.Grid) && result.Fragments.SequenceEqual(baseline.Fragments), "Stake does not multiply or hide parts");
                Check(result.Payout == baseline.Payout * stake && result.Cost == 5 * stake, "All cash rewards and cost scale exactly");
                Check(model.Snapshot.cash == 1000 - result.Cost + result.Payout && model.Snapshot.spins == 1, "One debit and reward");
                Check(model.Snapshot.fragments[0] == result.Grid.Count(x => x == (int)ReelSymbol.FloorPatch), "One visible icon equals one part");

                model = new GameModel(Collecting(stake, 0, 3));
                Check(model.NextSpinCost == 0 && model.NextRewardMultiplier == 1, "Free preview always1x");
                Check(model.TrySpin(new Pattern(), out result) && result.Free && result.Cost == 0 && result.RewardMultiplier == 1 && result.SelectedStake == stake, "Free captures choice but rewards1x");
                Check(result.Payout == baseline.Payout && model.Snapshot.freeSpins == 2 && model.SelectedStake == stake, "Free allowance consumed once and choice retained");

                model = new GameModel(Collecting(stake, 5 * stake - 1)); var before = codec.Encode(model.Snapshot);
                Check(!model.CanSpin && !model.TrySpin(new CashOnly(), out result), "Insufficient selected-stake funds");
                Check(codec.Encode(model.Snapshot) == before, "Insufficient funds leaves state unchanged");
                if (stake > 1) { Check(!model.CanRecover, "No free recovery while base stake affordable"); Check(model.TrySelectStake(1) && model.CanSpin, "Lower stake remains reachable"); }

                model = new GameModel(Collecting(stake, int.MaxValue - (80 * stake - 5 * stake)));
                Check(model.TrySpin(new CashOnly(), out result) && model.Snapshot.cash == int.MaxValue, "Exact net boundary accepted");
                model = new GameModel(Collecting(stake, int.MaxValue - (80 * stake - 5 * stake) + 1)); before = codec.Encode(model.Snapshot);
                Check(!model.TrySpin(new CashOnly(), out result) && codec.Encode(model.Snapshot) == before, "Overflow rolls back every field");

                var store = new Store { State = Collecting(stake) };
                var session = new GameSession(store, new Pattern());
                int accepted = 0; SpinResult captured = null;
                Parallel.For(0, 100, _ => { if (session.TryBeginSpin(out var pending)) { System.Threading.Interlocked.Increment(ref accepted); captured = pending; } });
                Check(accepted == 1 && store.State.spins == 1 && store.Writes == 1, "100 concurrent begin calls commit once");
                before = codec.Encode(session.Model.Snapshot);
                Parallel.For(0, 60, n => { CheckLocked(session, n % 3 + 1); });
                Check(codec.Encode(session.Model.Snapshot) == before && captured.SelectedStake == stake && captured.RewardMultiplier == stake, "Captured transaction unaffected by locked stake changes");
                Check(new GameSession(store, new Pattern()).Model.Snapshot.cash == store.State.cash, "Interrupted presentation relaunch keeps awarded cash");
                session.FinishSpinPresentation(); Check(session.TrySelectStake(stake == 3 ? 1 : 3), "Selection unlocked after presentation");
                Check(captured.SelectedStake == stake, "Result immutable after later selection");
            }
            var failed = new Store { State = Collecting(1), Fail = true }; var failedSession = new GameSession(failed, new Pattern());
            Check(!failedSession.TrySelectStake(3) && failedSession.Model.SelectedStake == 1, "Failed preference commit preserves selection");
            Check(!failedSession.TrySelectStake(0) && !failedSession.TrySelectStake(4), "Undefined selection rejected");
            // A selected stake is a no-op, never evidence that an earlier failed write succeeded.
            foreach (bool purchase in new[] { false, true }) {
                var statusStore = new Store { State = Collecting(1), Fail = true };
                if(purchase){statusStore.State.fragments[0]=20;statusStore.State.quests=Economy.QuestRecords(statusStore.State);}
                var statusSession=new GameSession(statusStore,new Pattern());
                Check(purchase ? !statusSession.TryPurchase(0) : !statusSession.TryBeginSpin(out _),"Initial purchase/spin commit fails");
                string failure=statusSession.Error, failedNotice=statusSession.Notice;int writes=statusStore.Writes;
                Check(failure.Length>0 && statusSession.TrySelectStake(1),"Current stake no-op remains accepted");
                Check(statusStore.Writes==writes && statusSession.Error==failure && statusSession.Notice==failedNotice,"No-op retains persistent failure and performs no write");
                Check(!statusSession.TrySelectStake(3) && statusSession.Model.SelectedStake==1 && statusSession.Error.Length>0,"Different stake failed write retains prior committed selection");
                statusStore.Fail=false;
                Check(statusSession.TrySelectStake(3) && statusSession.Model.SelectedStake==3 && statusSession.Error=="" && statusSession.Notice.StartsWith("Saved locally"),"Successful different stake clears error after actual commit");
            }
            var low = new GameModel(Collecting(3, 0));
            Check(low.CanRecover && low.GrantRecovery() && !low.GrantRecovery(), "High selected stake retains bounded free recovery");
            Check(low.TrySpin(new Pattern(), out var recovered) && recovered.RewardMultiplier == 1 && recovered.Cost == 0, "Recovery uses fair1x reward");

            Directory.CreateDirectory(directory); var file = Path.Combine(directory, "stakes-v09-save.json");
            foreach (string suffix in new[] { "", ".bak", ".tmp" }) if (File.Exists(file + suffix)) File.Delete(file + suffix);
            var disk = new SaveStore(file, codec);
            var old = Collecting(1, 123, 2); old.version = 5; old.stakeMultiplier = 0; old.spins = 19; old.recoveries = 4;
            old.purchased[0] = old.purchased[6] = true; old.fragments[1] = 37; old.fragments[7] = 92;
            old.tutorial.step = 3; old.tutorial.skipped = true; old.tutorial.storySeenMask = 5; old.quests = Economy.QuestRecords(old);
            string oldBytes = codec.Encode(old); File.WriteAllText(file, oldBytes);
            var migrated = disk.Load(out var notice);
            Check(migrated.version == 6 && migrated.stakeMultiplier == 1 && notice.Contains("upgraded"), "Schema5 defaults to1x");
            Check(migrated.cash == 123 && migrated.freeSpins == 2 && migrated.spins == 19 && migrated.recoveries == 4 && migrated.fragments.SequenceEqual(old.fragments) && migrated.purchased.SequenceEqual(old.purchased), "Schema5 every resource/surplus/out-of-order purchase retained");
            Check(migrated.tutorial.step == 3 && migrated.tutorial.skipped && migrated.tutorial.storySeenMask == 5 && migrated.tutorial.completed, "Schema5 tutorial/history preserved exactly");
            Check(File.ReadAllText(file) == oldBytes, "Load alone does not rewrite old save");
            var saved = new GameSession(disk, new Pattern());
            Check(saved.TrySelectStake(3) && new GameSession(disk, new Pattern()).Model.SelectedStake == 3, "Selection commits and survives disk reload");
            Check(disk.Save(saved.Model.Snapshot, out var error), "Establish current-schema backup " + error);
            var broken = saved.Model.Snapshot; broken.stakeMultiplier = 4;
            Check(!GameModel.IsPersistedValid(broken, out var reason), "Invalid schema6 selection rejected before normalization");
            File.WriteAllText(file, codec.Encode(broken));
            Check(disk.Load(out notice).stakeMultiplier == 3 && notice.Contains("Recovered"), "Invalid selection restores valid backup");
            var noStake = Regex.Replace(codec.Encode(saved.Model.Snapshot), @"\s*""stakeMultiplier""\s*:\s*\d+\s*,", "");
            Check(noStake != codec.Encode(saved.Model.Snapshot), "Missing-stake test actually removes property");
            File.WriteAllText(file, noStake);
            Check(disk.Load(out notice).stakeMultiplier == 3 && notice.Contains("Recovered"), "Missing schema6 selection is not constructor-filled");

            var balance = new List<string> { "stake,seeds,min_spins,median_spins,p95_spins,max_spins,mean_spins,mean_final_cash,min_paid_stake_reductions" };
            int[] baselineTotals = null;
            for (int stake = 1; stake <= 3; stake++)
            {
                int[] totals = new int[1000]; long finalCash = 0; int reductions = 0;
                for (int seed = 0; seed < 1000; seed++)
                {
                    var game = new GameModel(); game.TrySelectStake(stake); var rng = new SeededRandom(seed); int guard = 0;
                    while (!game.Complete && guard++ < 1000)
                    {
                        int active = game.ActiveQuest;
                        if (game.CanBuy(active)) { Check(game.TryBuy(active), "Sim manual buy"); Check(!game.TryBuy(active), "Sim repeat buy"); }
                        else if (game.CanRecover) Check(game.GrantRecovery(), "Sim recovery");
                        else
                        {
                            int affordable = Math.Min(stake, game.Snapshot.cash / Economy.SpinCost);
                            if (game.Snapshot.freeSpins == 0 && affordable < game.SelectedStake) { Check(game.TrySelectStake(Math.Max(1, affordable)), "Sim lower affordable stake"); reductions++; }
                            else if (game.SelectedStake != stake && game.Snapshot.cash >= 5 * stake) game.TrySelectStake(stake);
                            Check(game.TrySpin(rng, out var result), "Sim no selected-stake deadlock");
                            Check(game.Snapshot.cash >= 0 && result.Fragments.Sum() == result.Grid.Count(x => x != 0), "Sim no overdraw/hidden parts");
                        }
                    }
                    Check(game.Complete, "Sim full level complete"); totals[seed] = game.Snapshot.spins; finalCash += game.Snapshot.cash;
                }
                if (stake == 1) baselineTotals = (int[])totals.Clone();
                else Check(totals.Average() >= baselineTotals.Average() * .9, "Higher cash stake retains at least90percent baseline spin pacing");
                Array.Sort(totals);
                balance.Add(string.Join(",", stake, 1000, totals[0], totals[499], totals[949], totals[999], totals.Average().ToString("F3", System.Globalization.CultureInfo.InvariantCulture), (finalCash / 1000.0).ToString("F3", System.Globalization.CultureInfo.InvariantCulture), reductions));
            }
            File.WriteAllLines(Path.Combine(directory, "stakes-v09-pacing.csv"), balance);
            foreach (string suffix in new[] { "", ".bak", ".tmp" }) if (File.Exists(file + suffix)) File.Delete(file + suffix);
            return "PASS " + assertions + " stake/schema6 assertions; 3000 full levels,1x/2x/3x cost+cash scaling,one-icon-one-part,free1x,insufficient funds,overflow,concurrency,rollback,migration/reload; pacing=" + Path.Combine(directory, "stakes-v09-pacing.csv");
        }
        static void CheckLocked(GameSession session, int selection) { if (session.TrySelectStake(selection)) throw new Exception("Stake changed during presentation"); }
    }
}
