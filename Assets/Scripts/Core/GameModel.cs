using System;

namespace ZeroToMillionaire
{
    [Serializable]
    public sealed class QuestRecord
    {
        public string id;
        public QuestStatus status;
        public QuestRecord Clone(){return new QuestRecord{id=id,status=status};}
    }
    [Serializable]
    public sealed class PlayerState
    {
        public int version = Economy.SaveVersion;
        public int cash;
        public int freeSpins = Economy.StarterFreeSpins;
        public int spins;
        public int recoveries;
        public float heroineU = 1.85f, heroineV = 1.45f;
        public int[] fragments = new int[Economy.SymbolCount];
        public bool[] purchased = new bool[Economy.SymbolCount];
        public int[] lastGrid = new int[Economy.Columns * Economy.Rows];
        public QuestRecord[] quests;
        public PlayerState(){quests=Economy.QuestRecords(this);}
        public PlayerState Clone()
        {
            var copy=new PlayerState { version = version, cash = cash, freeSpins = freeSpins, spins = spins,
                recoveries = recoveries, heroineU = heroineU, heroineV = heroineV, fragments = (int[])fragments.Clone(),
                purchased = (bool[])purchased.Clone(), lastGrid = (int[])lastGrid.Clone() };
            if(quests==null)copy.quests=null;
            else{copy.quests=new QuestRecord[quests.Length];for(int i=0;i<quests.Length;i++)copy.quests[i]=quests[i]==null?null:quests[i].Clone();}
            return copy;
        }
    }

    public sealed class SpinResult
    {
        public readonly int[] Grid, Fragments, Pool;
        public readonly int Cost, Payout, Triples;
        public int NetCash { get { return Payout - Cost; } }
        public SpinResult(int[] grid, int[] fragments, int[] pool, int cost, int payout, int triples)
        { Grid = grid; Fragments = fragments; Pool=pool; Cost = cost; Payout = payout; Triples = triples; }
    }

    public sealed class GameModel
    {
        private readonly object gate = new object();
        private PlayerState state;
        public GameModel() { state = new PlayerState(); }
        public GameModel(PlayerState loaded)
        {
            loaded = Migrate(loaded);
            string reason;
            if (!IsValid(loaded, out reason)) throw new ArgumentException(reason);
            state = loaded.Clone();
        }
        public PlayerState Snapshot { get { lock (gate) return state.Clone(); } }
        public int PurchasedCount
        {
            get { lock (gate) { int n = 0; foreach (bool p in state.purchased) if (p) n++; return n; } }
        }
        public bool Complete { get { return PurchasedCount == Economy.SymbolCount; } }
        public int ActiveQuest { get { lock(gate)return Economy.ActiveQuest(state); } }
        public int NextQuest { get { lock(gate)return Economy.NextQuest(state); } }
        public QuestStatus QuestState(int index){lock(gate)return state.quests[index].status;}
        public bool CanSpin { get { lock (gate) {int current=Economy.ActiveQuest(state);return current>=0&&!CanBuyUnsafe(current)&&(state.freeSpins > 0 || state.cash >= Economy.SpinCost);} } }
        public bool CanRecover { get { lock (gate) return Economy.ActiveQuest(state)>=0&&state.freeSpins == 0 && state.cash < Economy.SpinCost; } }
        public int[] ActivePool { get { lock (gate) return Economy.ActivePool(state); } }
        public void SetHeroinePosition(FloorPoint feet)
        { lock (gate) { if (RoomGeometry.IsFree(feet)) { state.heroineU = feet.U; state.heroineV = feet.V; } } }

        public bool GrantRecovery()
        {
            lock (gate)
            {
                if (Economy.ActiveQuest(state)<0||state.freeSpins != 0 || state.cash >= Economy.SpinCost || state.recoveries == int.MaxValue) return false;
                state.freeSpins = 1; state.recoveries++; return true;
            }
        }

        public bool TrySpin(IRandomSource random, out SpinResult result)
        {
            if (random == null) throw new ArgumentNullException("random");
            lock (gate)
            {
                result = null;
                int current=Economy.ActiveQuest(state);
                if (current<0||CanBuyUnsafe(current)||(state.freeSpins == 0 && state.cash < Economy.SpinCost)) return false;
                int[] grid = new int[Economy.Columns * Economy.Rows];
                int[] gains = new int[Economy.SymbolCount];
                int[] pool = Economy.ActivePool(state);
                for (int i = 0; i < grid.Length; i++)
                {
                    int choice = random.Next(pool.Length);
                    if (choice < 0 || choice >= pool.Length) throw new ArgumentOutOfRangeException("random");
                    int symbol = pool[choice];
                    grid[i] = symbol;
                    if (symbol != (int)ReelSymbol.Cash) {
                        int index=symbol-1,remaining=Math.Max(0,Economy.Upgrades[index].FragmentGoal-state.fragments[index]);
                        // Keep one visible icon == one part. Extra rolls become cash,
                        // so a finite task never creates useless surplus.
                        if(gains[index]>=remaining){symbol=(int)ReelSymbol.Cash;grid[i]=symbol;}
                        else gains[index]++;
                    }
                }
                int triples = 0;
                for (int x = 0; x < Economy.Columns; x++)
                    if (grid[x] == grid[x + Economy.Columns] && grid[x] == grid[x + 2 * Economy.Columns]) triples++;
                int cost = state.freeSpins > 0 ? 0 : Economy.SpinCost;
                int payout = Economy.BasePayout + triples * Economy.TripleBonus;
                foreach (int symbol in grid)
                {
                    if (symbol == (int)ReelSymbol.Cash) payout += 2;
                }
                // Validate the complete transaction before changing any balance.
                long nextCash=(long)state.cash-cost+payout;
                if(nextCash<0||nextCash>int.MaxValue||state.spins==int.MaxValue)return false;
                for (int i = 0; i < gains.Length; i++) if (state.fragments[i] > int.MaxValue - gains[i]) return false;
                if (state.freeSpins > 0) state.freeSpins--;
                state.cash = (int)nextCash;
                state.spins++;
                for (int i = 0; i < gains.Length; i++) state.fragments[i] += gains[i];
                state.lastGrid = grid;
                state.quests=Economy.QuestRecords(state);
                result = new SpinResult((int[])grid.Clone(), gains, (int[])pool.Clone(), cost, payout, triples);
                return true;
            }
        }

        private bool CanBuyUnsafe(int index)
        {
            if (index < 0 || index >= Economy.Upgrades.Length || state.purchased[index]) return false;
            if(index!=Economy.ActiveQuest(state))return false;
            UpgradeDefinition d = Economy.Upgrades[index];
            return state.cash >= d.CashPrice && state.fragments[index] >= d.FragmentGoal;
        }
        public bool CanBuy(int index) { lock (gate) return CanBuyUnsafe(index); }
        public bool TryBuy(int index)
        {
            lock (gate)
            {
                if (!CanBuyUnsafe(index)) return false;
                UpgradeDefinition d = Economy.Upgrades[index];
                state.cash -= d.CashPrice; state.fragments[index] -= d.FragmentGoal;
                state.purchased[index] = true;
                state.quests=Economy.QuestRecords(state);
                return true;
            }
        }

        public static bool IsValid(PlayerState s,out string reason)
        {
            if(!IsPersistedValid(s,out reason))return false;
            if(s.version!=Economy.SaveVersion){reason="Save requires migration";return false;}
            return true;
        }

        // Check raw persisted shape BEFORE deriving current quest statuses.
        public static bool IsPersistedValid(PlayerState s,out string reason)
        {
            reason="Invalid or unsupported save";
            if(s==null||s.version<1||s.version>Economy.SaveVersion||s.cash<0||
                s.freeSpins<0||s.freeSpins>Economy.StarterFreeSpins||s.spins<0||s.recoveries<0||
                s.fragments==null||s.fragments.Length!=Economy.SymbolCount||
                s.purchased==null||s.purchased.Length!=Economy.SymbolCount||
                s.lastGrid==null||s.lastGrid.Length!=Economy.Columns*Economy.Rows)return false;
            if(s.version>1&&!RoomGeometry.IsFreeForSave(new FloorPoint(s.heroineU,s.heroineV),s.version))return false;
            foreach(int part in s.fragments)if(part<0)return false;
            int iconCount=s.version==1?5:Economy.ReelSymbolCount;
            foreach(int icon in s.lastGrid)if(icon<0||icon>=iconCount)return false;
            if(s.version>=3){
                if(s.quests==null||s.quests.Length!=Economy.SymbolCount){reason="Missing quest records";return false;}
                var ids=new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
                foreach(var quest in s.quests){
                    if(quest==null||string.IsNullOrEmpty(quest.id)||!ids.Add(quest.id)||
                        !Enum.IsDefined(typeof(QuestStatus),quest.status)||
                        Array.Find(Economy.Upgrades,d=>d.Id==quest.id)==null){
                        reason="Invalid or duplicate quest ID";return false;
                    }
                }
            }
            reason="";return true;
        }

        public static PlayerState Migrate(PlayerState source)
        {
            string reason;if(!IsPersistedValid(source,out reason))return source;
            // v1 counters already belonged to individual upgrades. Preserve every
            // unspent fragment, purchase, cash balance and starter/recovery allowance.
            if (source.fragments == null || source.fragments.Length!=Economy.SymbolCount||source.purchased == null||source.purchased.Length!=Economy.SymbolCount || source.lastGrid == null) return source;
            PlayerState migrated = source.Clone();
            if(source.version==1){
                foreach (int icon in source.lastGrid) if (icon < 0 || icon >= 5) return source;
                for (int i = 0; i < migrated.lastGrid.Length; i++)
                    migrated.lastGrid[i] = source.lastGrid[i] == 1 ? (int)ReelSymbol.WallRepair :
                        source.lastGrid[i] == 3 ? (int)ReelSymbol.Mattress : (int)ReelSymbol.Cash;
                migrated.heroineU = 1.85f; migrated.heroineV = 1.45f;
            }
            if(source.version>=3&&source.quests!=null){
                foreach(var quest in source.quests)if(quest!=null&&quest.status==QuestStatus.Completed){
                    int index=Array.FindIndex(Economy.Upgrades,d=>d.Id==quest.id);if(index>=0)migrated.purchased[index]=true;
                }
            }
            if(source.version>1&&source.version<4){var feet=RoomGeometry.ProjectLegacyFeet(new FloorPoint(migrated.heroineU,migrated.heroineV));migrated.heroineU=feet.U;migrated.heroineV=feet.V;}
            migrated.version = Economy.SaveVersion;
            migrated.quests=Economy.QuestRecords(migrated);
            return migrated;
        }
    }
}
