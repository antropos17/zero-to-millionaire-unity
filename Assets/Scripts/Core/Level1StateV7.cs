using System;

namespace ZeroToMillionaire
{
    public sealed class DecorDefinition
    {
        public readonly string Id,Title; public readonly int CashPrice,FragmentGoal,ReelSymbol;
        public DecorDefinition(string id,string title,int cash,int parts,int symbol){Id=id;Title=title;CashPrice=cash;FragmentGoal=parts;ReelSymbol=symbol;}
    }
    public static class DecorEconomyV7
    {
        public const int SaveVersion=7,Count=4,FirstSymbol=9,ReelCount=13;
        public static readonly DecorDefinition[] Items={
            new DecorDefinition("plant","Windowsill plant",90,18,9),
            new DecorDefinition("crate","Small storage crate",120,22,10),
            new DecorDefinition("curtains","Simple curtains",150,26,11),
            new DecorDefinition("shelf","Small wall shelf",180,30,12)
        };
    }
    // Deliberately separate from the verified schema6 runtime DTO until UI/art integration.
    [Serializable] public sealed class Level1StateV7
    {
        public int version=7,cash,stakeMultiplier=1,freeSpins=3,spins,recoveries;
        public float heroineU=1.85f,heroineV=1.45f;
        public int[] fragments=new int[8],lastGrid=new int[15],decorFragments=new int[4];
        public bool[] purchased=new bool[8],decorPurchased=new bool[4];
        public QuestRecord[] quests;
        public TutorialState tutorial=new TutorialState();
        public bool decorStarted;
        public Level1StateV7(){quests=new PlayerState().quests;}
        public Level1StateV7 Clone()
        {
            var s=(Level1StateV7)MemberwiseClone();s.fragments=fragments==null?null:(int[])fragments.Clone();s.lastGrid=lastGrid==null?null:(int[])lastGrid.Clone();s.decorFragments=decorFragments==null?null:(int[])decorFragments.Clone();
            s.purchased=purchased==null?null:(bool[])purchased.Clone();s.decorPurchased=decorPurchased==null?null:(bool[])decorPurchased.Clone();s.tutorial=tutorial?.Clone();
            if(quests!=null){s.quests=new QuestRecord[quests.Length];for(int i=0;i<quests.Length;i++)s.quests[i]=quests[i]?.Clone();}return s;
        }
        public PlayerState RenovationView()
        {
            var s=new PlayerState{version=6,cash=cash,stakeMultiplier=stakeMultiplier,freeSpins=freeSpins,spins=spins,recoveries=recoveries,heroineU=heroineU,heroineV=heroineV,fragments=fragments==null?null:(int[])fragments.Clone(),purchased=purchased==null?null:(bool[])purchased.Clone(),lastGrid=lastGrid==null?null:(int[])lastGrid.Clone(),tutorial=tutorial?.Clone(),quests=null};
            if(s.lastGrid!=null)for(int i=0;i<s.lastGrid.Length;i++)if(s.lastGrid[i]>=9)s.lastGrid[i]=0;
            if(quests!=null){s.quests=new QuestRecord[quests.Length];for(int i=0;i<quests.Length;i++)s.quests[i]=quests[i]?.Clone();}return s;
        }
        internal void ApplyRenovation(PlayerState s)
        {
            cash=s.cash;stakeMultiplier=s.stakeMultiplier;freeSpins=s.freeSpins;spins=s.spins;recoveries=s.recoveries;heroineU=s.heroineU;heroineV=s.heroineV;
            fragments=(int[])s.fragments.Clone();purchased=(bool[])s.purchased.Clone();lastGrid=(int[])s.lastGrid.Clone();tutorial=s.tutorial.Clone();quests=new QuestRecord[8];for(int i=0;i<8;i++)quests[i]=s.quests[i].Clone();
        }
        public static Level1StateV7 MigrateLegacy(PlayerState source)
        {
            string error;if(!GameModel.IsPersistedValid(source,out error))throw new ArgumentException(error);
            if(source.version==6){var expected=Economy.QuestRecords(source);foreach(var q in source.quests){int i=Array.FindIndex(expected,x=>x.id==q.id);if(i<0||expected[i].status!=q.status)throw new ArgumentException("Inconsistent schema6 quest status");}}
            var migrated=GameModel.Migrate(source);var s=new Level1StateV7();s.ApplyRenovation(migrated);
            if(!Valid(s,out error))throw new ArgumentException(error);return s;
        }
        public static bool Valid(Level1StateV7 s,out string error)
        {
            error="Invalid schema7 state";
            if(s==null||s.version!=7||s.decorFragments==null||s.decorFragments.Length!=4||s.decorPurchased==null||s.decorPurchased.Length!=4||s.lastGrid==null||s.lastGrid.Length!=15)return false;
            string baseError;if(!GameModel.IsValid(s.RenovationView(),out baseError)){error=baseError;return false;}
            foreach(int icon in s.lastGrid)if(icon<0||icon>=13||(!s.decorStarted&&icon>=9))return false;
            bool gap=false;int owned=0;
            for(int i=0;i<4;i++)
            {
                if(s.decorFragments[i]<0||s.decorFragments[i]>DecorEconomyV7.Items[i].FragmentGoal)return false;
                if(s.decorPurchased[i]){if(gap||s.decorFragments[i]!=0)return false;owned++;}else gap=true;
                if(!s.decorStarted&&(s.decorPurchased[i]||s.decorFragments[i]!=0))return false;
                if(i>owned&&s.decorFragments[i]!=0)return false;
            }
            if(s.decorStarted&&Array.Exists(s.purchased,b=>!b))return false;
            // Newly written schema7 must have canonical, consistent base quest statuses.
            var expected=Economy.QuestRecords(s.RenovationView());
            foreach(var q in s.quests){int i=Array.FindIndex(expected,x=>x.id==q.id);if(i<0||expected[i].status!=q.status)return false;}
            error="";return true;
        }
    }
    public sealed class Level1ModelV7
    {
        readonly object gate=new object();Level1StateV7 state;
        public Level1ModelV7(Level1StateV7 s){string error;if(!Level1StateV7.Valid(s,out error))throw new ArgumentException(error);state=s.Clone();}
        public Level1StateV7 Snapshot{get{lock(gate)return state.Clone();}}
        public int PurchasedCount{get{lock(gate)return Array.FindAll(state.purchased,b=>b).Length;}}
        public bool Complete=>PurchasedCount==8;
        public int DecorPurchasedCount{get{lock(gate)return Array.FindAll(state.decorPurchased,b=>b).Length;}}
        public bool DecorComplete{get{lock(gate)return state.decorStarted&&Array.TrueForAll(state.decorPurchased,b=>b);}}
        int DecorQuestUnsafe()=>state.decorStarted?Array.FindIndex(state.decorPurchased,b=>!b):-1;
        public int ActiveDecorQuest{get{lock(gate)return DecorQuestUnsafe();}}
        bool CanBuyDecorUnsafe(int index){if(index<0||index>=4||index!=DecorQuestUnsafe()||state.decorPurchased[index])return false;var d=DecorEconomyV7.Items[index];return state.cash>=d.CashPrice&&state.decorFragments[index]>=d.FragmentGoal;}
        public bool CanBuyDecor(int index){lock(gate)return CanBuyDecorUnsafe(index);}
        public bool CanSpin{get{lock(gate){if(!state.decorStarted)return new GameModel(state.RenovationView()).CanSpin;int q=DecorQuestUnsafe();return q>=0&&!CanBuyDecorUnsafe(q)&&(state.freeSpins>0||state.cash>=5*state.stakeMultiplier);}}}
        public bool CanRecover{get{lock(gate)return state.decorStarted?DecorQuestUnsafe()>=0&&state.freeSpins==0&&state.cash<5:new GameModel(state.RenovationView()).CanRecover;}}
        public bool TryStartDecorating(){lock(gate){if(state.decorStarted||!Complete)return false;state.decorStarted=true;return true;}}
        public bool TrySelectStake(int value){lock(gate){if(!Economy.ValidStake(value))return false;state.stakeMultiplier=value;return true;}}
        public bool TryBasePurchase(int index){lock(gate){if(state.decorStarted)return false;var b=new GameModel(state.RenovationView());if(!b.TryBuy(index))return false;state.ApplyRenovation(b.Snapshot);return true;}}
        public bool TryDecorPurchase(int index)
        {lock(gate){if(!CanBuyDecorUnsafe(index))return false;var d=DecorEconomyV7.Items[index];state.cash-=d.CashPrice;state.decorFragments[index]-=d.FragmentGoal;state.decorPurchased[index]=true;return true;}}
        public bool TryRecover(){lock(gate){if(!CanRecover||state.recoveries==int.MaxValue)return false;state.freeSpins=1;state.recoveries++;return true;}}
        public bool TrySpin(IRandomSource random,out SpinResult result)
        {
            if(random==null)throw new ArgumentNullException(nameof(random));lock(gate)
            {
                result=null;if(!state.decorStarted){var b=new GameModel(state.RenovationView());if(!b.TrySpin(random,out result))return false;state.ApplyRenovation(b.Snapshot);return true;}
                if(!CanSpin||state.spins==int.MaxValue)return false;int q=DecorQuestUnsafe();var d=DecorEconomyV7.Items[q];int remaining=d.FragmentGoal-state.decorFragments[q];
                int[] pool=remaining>0?new[]{0,d.ReelSymbol}:new[]{0};int[] grid=new int[15],gains=new int[12];int parts=0;
                for(int i=0;i<15;i++){int roll=random.Next(pool.Length);if(roll<0||roll>=pool.Length)throw new ArgumentOutOfRangeException(nameof(random));int icon=pool[roll];if(icon!=0){if(parts>=remaining)icon=0;else parts++;}grid[i]=icon;}
                if(remaining>0&&parts==0){grid[14]=d.ReelSymbol;parts=1;}
                int triples=0,cashIcons=0;for(int x=0;x<5;x++)if(grid[x]==grid[x+5]&&grid[x]==grid[x+10])triples++;foreach(int icon in grid)if(icon==0)cashIcons++;
                int selected=state.stakeMultiplier,multiplier=state.freeSpins>0?1:selected,cost=state.freeSpins>0?0:5*selected,payout=(12+2*cashIcons+10*triples)*multiplier;
                long cash=(long)state.cash-cost+payout;if(cash<0||cash>int.MaxValue)return false;
                state.cash=(int)cash;state.spins++;if(state.freeSpins>0)state.freeSpins--;state.decorFragments[q]+=parts;state.lastGrid=grid;gains[8+q]=parts;
                result=new SpinResult((int[])grid.Clone(),gains,pool,cost,payout,triples,selected,multiplier);return true;
            }
        }
    }
}
