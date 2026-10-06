using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ZeroToMillionaire
{
    public static class ModelChecks
    {
        sealed class ConstantRandom : IRandomSource
        { readonly int value; public ConstantRandom(int v) { value = v; } public int Next(int max) { return value; } }
        sealed class SequenceRandom : IRandomSource
        { int index; public int Next(int max) { return index++ % max; } }
        static int assertions;
        static void Check(bool condition, string message)
        { assertions++; if (!condition) throw new Exception("FAILED: " + message); }

        public static string Run(IStateCodec codec, string directory)
        {
            assertions = 0;
            var model = new GameModel();
            Check(model.Snapshot.cash == 0 && model.Snapshot.freeSpins == 3, "new game starter allowance");
            Check(!model.GrantRecovery(), "cannot farm free recovery with allowance");
            Check(!model.TryBuy(0), "cannot buy without both resources");
            SpinResult spin;
            Check(model.TrySpin(new ConstantRandom(1), out spin), "free starter spin works at zero cash");
            Check(spin.Grid.Length == 15 && spin.Cost == 0 && spin.Triples == 0 && spin.Payout == 26, "5x3 finite grid: excess item rolls turn into cash");
            Check(model.Snapshot.fragments[0] == 8 && model.Snapshot.fragments[1] == 0 && model.Snapshot.fragments[3] == 0, "floor icon feeds exactly one matching counter up to task goal");
            Check(model.QuestState(0)==QuestStatus.ReadyToBuy&&!model.CanSpin,"full resources lead to explicit installation, no idle farming");
            var before = model.Snapshot;
            Check(model.TryBuy(0), "explicit BUY applies upgrade");
            var after = model.Snapshot;
            Check(after.cash == before.cash - 18 && after.fragments[0] == 0 && after.fragments[1] == 0 && after.purchased[0], "only target counter/cash consumed");
            for (int i = 0; i < 30; i++) Check(!model.TryBuy(0), "repeated BUY cannot double purchase");
            Check(model.Snapshot.cash == after.cash, "repeated taps never overdraw");

            var enoughCash = new PlayerState { cash = 1000 };
            Check(!new GameModel(enoughCash).TryBuy(0), "cash alone insufficient");
            var enoughFragments = new PlayerState(); enoughFragments.fragments[0] = 100;
            Check(!new GameModel(enoughFragments).TryBuy(0), "fragments alone insufficient");
            var cashNeeded=new PlayerState{cash=5,freeSpins=0};cashNeeded.fragments[0]=100;
            var earnCash=new GameModel(cashNeeded);
            Check(earnCash.QuestState(0)==QuestStatus.Collecting&&earnCash.ActivePool.Length==1&&earnCash.ActivePool[0]==0,"parts full with cash missing remains collecting cash, never generic completion");
            Check(earnCash.TrySpin(new ConstantRandom(0),out spin)&&earnCash.Snapshot.fragments[0]==100&&spin.Fragments[0]==0,"legacy extra parts preserved while cash-only reels add no useless stock");
            Check(earnCash.CanBuy(0)&&earnCash.QuestState(0)==QuestStatus.ReadyToBuy&&!earnCash.CanSpin,"cash completion becomes ready to buy, then pauses collection");
            var rich = new PlayerState { cash = 1000 };
            for (int i = 0; i < 8; i++) rich.fragments[i] = 100;
            var race = new GameModel(rich);
            Check(!race.TryBuy(1), "prerequisite enforced even with resources");
            int successes = 0;
            Parallel.For(0, 100, _ => { if (race.TryBuy(0)) System.Threading.Interlocked.Increment(ref successes); });
            Check(successes == 1 && race.Snapshot.cash == 982, "concurrent repeated purchase commits once");
            Check(!race.TryBuy(-1) && !race.TryBuy(8), "out of range purchase safely rejected");

            var zero = new PlayerState { cash = 0, freeSpins = 0 };
            var recovery = new GameModel(zero);
            Check(!recovery.TrySpin(new SequenceRandom(), out spin), "insufficient paid spin rejected");
            Check(recovery.GrantRecovery() && !recovery.GrantRecovery(), "transparent single recovery allowance");
            Check(recovery.TrySpin(new SequenceRandom(), out spin) && spin.Cost == 0 && recovery.Snapshot.cash >= 12, "recovery resolves zero cash");
            var paid = new GameModel(new PlayerState { cash = 5, freeSpins = 0 });
            Check(paid.TrySpin(new SequenceRandom(), out spin) && spin.Cost == 5 && paid.Snapshot.cash == spin.NetCash + 5, "paid spin charges exactly once");
            int total = 0;
            foreach (int v in paid.Snapshot.fragments) total += v;
            Check(total == 7, "alternating 15-cell grid gives exactly seven matching parts");

            Check(new GameModel().ActivePool.Length==2&&new GameModel().ActivePool[1]==(int)ReelSymbol.FloorPatch,"only current floor task and cash appear initially");
            var transitionState=rich.Clone();transitionState.fragments[2]=0;
            var transition = new GameModel(transitionState);
            Check(transition.TryBuy(0) && transition.TryBuy(1), "repair transition purchased explicitly");
            Check(Array.IndexOf(transition.ActivePool, (int)ReelSymbol.WallRepair) < 0 &&
                  Array.IndexOf(transition.ActivePool, (int)ReelSymbol.Wallpaper) >= 0 &&
                  Array.IndexOf(transition.ActivePool, (int)ReelSymbol.FloorPatch) < 0, "wallpaper replaces repaired wall icon; floor retired");
            foreach (int icon in transition.ActivePool)
            {
                int choice = Array.IndexOf(transition.ActivePool, icon);
                var exact = new GameModel(transition.Snapshot); SpinResult itemSpin;
                Check(exact.TrySpin(new ConstantRandom(choice), out itemSpin), "active icon selectable");
                for (int i = 0; i < 8; i++) Check(itemSpin.Fragments[i] == (icon == i + 1 ? Economy.Upgrades[i].FragmentGoal : 0), "one-to-one finite reward for item " + icon);
                for(int i=0;i<8;i++){int matching=0;foreach(int cell in itemSpin.Grid)if(cell==i+1)matching++;Check(matching==itemSpin.Fragments[i],"every visible matching icon awards exactly one part");}
            }
            var legacy = model.Snapshot; legacy.version = 1; legacy.lastGrid[0] = 3; legacy.lastGrid[1] = 1;
            legacy.fragments[4] = 57;
            var migrated = new GameModel(legacy).Snapshot;
            Check(migrated.version == 3 && migrated.cash == legacy.cash && migrated.fragments[4] == 57 &&
                  migrated.fragments[0] == legacy.fragments[0] && migrated.purchased[0] &&
                  migrated.freeSpins == legacy.freeSpins && migrated.lastGrid[0] == (int)ReelSymbol.Mattress,
                  "v1 migration preserves all balances, progress and purchases without minting fragments");

            var isolation = model.Snapshot; isolation.cash = 999; isolation.fragments[1] = 999;
            Check(model.Snapshot.cash != 999 && model.Snapshot.fragments[1] != 999, "snapshots cannot mutate live state");
            string reason;
            var invalid = new PlayerState { cash = -1 };
            Check(!GameModel.IsValid(invalid, out reason), "negative balances rejected");
            var outOfOrder=new PlayerState{version=2,cash=42,freeSpins=0,spins=17};outOfOrder.purchased[2]=outOfOrder.purchased[6]=true;
            outOfOrder.fragments[0]=91;outOfOrder.fragments[4]=57;
            var unordered=new GameModel(outOfOrder);var kept=unordered.Snapshot;
            Check(unordered.ActiveQuest==0&&unordered.NextQuest==1&&unordered.PurchasedCount==2,"legacy out-of-order ownership remains complete; first unfinished activates");
            Check(kept.cash==42&&kept.spins==17&&kept.fragments[0]==91&&kept.fragments[4]==57&&kept.purchased[2]&&kept.purchased[6],"v2 migration preserves all surplus and out-of-order purchases");
            var again=GameModel.Migrate(kept);
            Check(codec.Encode(again)==codec.Encode(kept),"schema3 migration is idempotent");
            Check(kept.quests[2].id=="cheap-wallpaper"&&kept.quests[2].status==QuestStatus.Completed&&kept.quests[1].status==QuestStatus.Locked,"stable quest IDs and locked/completed states persist");
            Check(unordered.TryBuy(0)&&unordered.ActiveQuest==1&&unordered.TryBuy(1)==false,"manual buying only activates next unfinished task");
            var stale=kept.Clone();stale.quests[0].status=QuestStatus.ReadyToBuy;stale.cash=0;
            Check(new GameModel(stale).QuestState(0)==QuestStatus.Collecting&&!new GameModel(stale).CanBuy(0),"stored readiness is derived again from both live balances");
            invalid = new PlayerState(); invalid.quests[1].id=invalid.quests[0].id;
            Check(!GameModel.IsValid(invalid,out reason),"duplicate stable quest IDs rejected");
            invalid = new PlayerState(); invalid.lastGrid[0] = Economy.ReelSymbolCount;
            Check(!GameModel.IsValid(invalid, out reason), "unknown reel icon rejected");

            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "checks-save.json");
            // This directory is the explicit disposable test sandbox, never a player's save.
            foreach (string suffix in new[] { "", ".bak", ".tmp" }) if (File.Exists(path + suffix)) File.Delete(path + suffix);
            var store = new SaveStore(path, codec); string error, notice;
            Check(store.Save(model.Snapshot, out error), "first disk save: " + error);
            var loaded = new GameModel(store.Load(out notice));
            Check(loaded.Snapshot.cash == model.Snapshot.cash && loaded.Snapshot.purchased[0] && loaded.Snapshot.fragments[1] == 0, "new model reload preserves exact purchase/resources");
            File.WriteAllText(path, codec.Encode(legacy));
            Check(store.Load(out notice).version == 3 && store.Load(out notice).fragments[4] == 57 && notice.Length > 0, "legacy disk save migrates on load");
            Check(store.Save(kept,out error)&&new GameModel(store.Load(out notice)).ActiveQuest==0,"out-of-order migrated quest state survives disk reload");
            var completedState=rich.Clone();for(int i=0;i<8;i++)completedState.purchased[i]=true;
            var completed=new GameModel(completedState);
            Check(completed.Complete&&completed.ActiveQuest==-1&&completed.NextQuest==-1&&!completed.CanSpin&&!completed.GrantRecovery(),"final 8/8 has no new quest or endless collection");
            Check(!completed.TrySpin(new ConstantRandom(0),out spin)&&!completed.TryBuy(7),"completed room rejects spins and repeated final purchase");
            Check(store.Save(completed.Snapshot,out error)&&new GameModel(store.Load(out notice)).Complete,"completed stable quests and owned room survive reload");
            Check(store.Save(model.Snapshot, out error), "restore tested primary before backup check");
            Check(store.Save(race.Snapshot, out error), "atomic second save: " + error);
            File.WriteAllText(path, "{broken save");
            Check(store.Load(out notice).cash == model.Snapshot.cash && notice.Length > 0, "corrupt primary recovers known valid backup");
            Check(store.Save(new PlayerState(), out error) && store.Save(new PlayerState(), out error), "confirmed reset writes primary and backup");
            Check(store.Load(out notice).freeSpins == 3 && store.Load(out notice).cash == 0, "reset survives relaunch");
            File.WriteAllText(path, "bad"); File.WriteAllText(path + ".bak", "bad");
            Check(store.Load(out notice).cash == 0 && notice.Length > 0, "both corrupt saves handled without exception");
            foreach (string suffix in new[] { "", ".bak", ".tmp" }) if (File.Exists(path + suffix)) File.Delete(path + suffix);

            var runs = new List<int>();
            var watch = Stopwatch.StartNew();
            for (int seed = 0; seed < 1000; seed++)
            {
                var game = new GameModel(); var rng = new SeededRandom(seed);
                int spins = 0;
                while (!game.Complete && spins < 300)
                {
                    if (!game.CanSpin) Check(game.GrantRecovery(), "simulation recovery path");
                    Check(game.TrySpin(rng, out spin), "simulation can progress"); spins++;
                    for (int j = 0; j < 8; j++) game.TryBuy(j);
                    Check(game.Snapshot.cash >= 0, "simulation nonnegative balances");
                }
                Check(game.Complete, "seed " + seed + " can complete full level"); runs.Add(spins);
            }
            watch.Stop(); runs.Sort();
            string walking = NavigationChecks.Run();
            string sessions=SessionChecks.Run(codec,directory);
            return "PASS " + assertions + " economy assertions; 1000 complete levels; spins min=" + runs[0] +
                " median=" + runs[500] + " p95=" + runs[950] + " max=" + runs[999] +
                "; simulation_ms=" + watch.ElapsedMilliseconds + "\n" + walking+"\n"+sessions;
        }
    }
}
