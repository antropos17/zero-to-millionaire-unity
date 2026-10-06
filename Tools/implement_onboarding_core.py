from pathlib import Path
r=Path(r"X:\Projects\ZERO-TO-MILLIONAIRE")
def edit(rel,old,new):
 p=r/rel;s=p.read_text(encoding='utf-8');assert old in s,rel+' missing replacement';p.write_text(s.replace(old,new),encoding='utf-8')
edit('Assets/Scripts/Core/Economy.cs','public const int SaveVersion = 4;','public const int SaveVersion = 5;')
edit('Assets/Scripts/Core/Economy.cs','public static readonly UpgradeDefinition[] Upgrades = {','public static readonly UpgradeDefinition[] LegacyUpgrades = {')
edit('Assets/Scripts/Core/Economy.cs','        public static int ActiveQuest(PlayerState s)', '''        public static readonly UpgradeDefinition[] Upgrades = SlowCurve();
        static UpgradeDefinition[] SlowCurve(){
            int[] parts={20,30,38,44,50,58,62,70},cash={60,115,155,195,235,300,315,355};
            var values=new UpgradeDefinition[8];for(int i=0;i<8;i++){var d=LegacyUpgrades[i];values[i]=new UpgradeDefinition(d.Symbol,d.Id,d.ActionTitle,d.Title,cash[i],parts[i],d.Description);}return values;
        }
        public static UpgradeDefinition For(PlayerState s,int index){
            var d=Upgrades[index];if(s.quests!=null)foreach(var q in s.quests)if(q!=null&&q.id==d.Id&&q.cashPrice>0&&q.fragmentGoal>0)
                return q.cashPrice==LegacyUpgrades[index].CashPrice&&q.fragmentGoal==LegacyUpgrades[index].FragmentGoal?LegacyUpgrades[index]:d;
            return s.version<5?LegacyUpgrades[index]:d;
        }
        public static bool ValidRequirements(int index,int cash,int parts){return (cash==Upgrades[index].CashPrice&&parts==Upgrades[index].FragmentGoal)||(cash==LegacyUpgrades[index].CashPrice&&parts==LegacyUpgrades[index].FragmentGoal);}
        public static int ActiveQuest(PlayerState s)''')
edit('Assets/Scripts/Core/Economy.cs','for(int i=0;i<SymbolCount;i++)records[i]=new QuestRecord{ id=Upgrades[i].Id,','for(int i=0;i<SymbolCount;i++){var rule=For(s,i);records[i]=new QuestRecord{ id=Upgrades[i].Id,cashPrice=rule.CashPrice,fragmentGoal=rule.FragmentGoal,')
edit('Assets/Scripts/Core/Economy.cs','s.fragments[i]>=Upgrades[i].FragmentGoal&&s.cash>=Upgrades[i].CashPrice?QuestStatus.ReadyToBuy:QuestStatus.Collecting};','s.fragments[i]>=rule.FragmentGoal&&s.cash>=rule.CashPrice?QuestStatus.ReadyToBuy:QuestStatus.Collecting};}')
edit('Assets/Scripts/Core/Economy.cs','s.fragments[current]<Upgrades[current].FragmentGoal','s.fragments[current]<For(s,current).FragmentGoal')
edit('Assets/Scripts/Core/GameModel.cs','        public QuestStatus status;','        public QuestStatus status;\n        public int cashPrice,fragmentGoal;')
edit('Assets/Scripts/Core/GameModel.cs','new QuestRecord{id=id,status=status}','new QuestRecord{id=id,status=status,cashPrice=cashPrice,fragmentGoal=fragmentGoal}')
edit('Assets/Scripts/Core/GameModel.cs','        public QuestRecord[] quests;','        public QuestRecord[] quests;\n        public TutorialState tutorial=new TutorialState();')
edit('Assets/Scripts/Core/GameModel.cs','purchased = (bool[])purchased.Clone(), lastGrid = (int[])lastGrid.Clone()','purchased = (bool[])purchased.Clone(), lastGrid = (int[])lastGrid.Clone(),tutorial=tutorial==null?null:tutorial.Clone()')
edit('Assets/Scripts/Core/GameModel.cs','        public bool CanSpin {','        public UpgradeDefinition Definition(int index){lock(gate)return Economy.For(state,index);}\n        public void SetTutorial(TutorialState value){if(!TutorialPolicy.Valid(value))throw new ArgumentException("Invalid tutorial state");lock(gate)state.tutorial=value.Clone();}\n        public bool CanSpin {')
edit('Assets/Scripts/Core/GameModel.cs','Economy.Upgrades[index].FragmentGoal-state.fragments[index]','Economy.For(state,index).FragmentGoal-state.fragments[index]')
edit('Assets/Scripts/Core/GameModel.cs','                int triples = 0;','''                // Every collecting spin displays at least one matching icon; no hidden reward.
                if(pool.Length>1&&gains[current]==0){grid[grid.Length-1]=(int)Economy.Upgrades[current].FragmentSymbol;gains[current]=1;}
                int triples = 0;''')
edit('Assets/Scripts/Core/GameModel.cs','UpgradeDefinition d = Economy.Upgrades[index];','UpgradeDefinition d = Economy.For(state,index);')
edit('Assets/Scripts/Core/GameModel.cs','            reason="";return true;\n        }\n\n        public static PlayerState Migrate','''            if(s.version>=5){
                if(!TutorialPolicy.Valid(s.tutorial)){reason="Invalid tutorial state";return false;}
                foreach(var q in s.quests){int i=Array.FindIndex(Economy.Upgrades,d=>d.Id==q.id);if(!Economy.ValidRequirements(i,q.cashPrice,q.fragmentGoal)){reason="Invalid stored task requirements";return false;}}
            }
            reason="";return true;
        }

        public static PlayerState Migrate''')
edit('Assets/Scripts/Core/GameModel.cs','            migrated.version = Economy.SaveVersion;','''            if(source.version<5){
                int active=Economy.ActiveQuest(migrated);var records=new QuestRecord[8];
                for(int i=0;i<8;i++){var d=i==active?Economy.LegacyUpgrades[i]:Economy.Upgrades[i];records[i]=new QuestRecord{id=d.Id,cashPrice=d.CashPrice,fragmentGoal=d.FragmentGoal};}
                migrated.quests=records;
                bool experienced=source.spins>0||Array.Exists(migrated.purchased,p=>p);
                migrated.tutorial=new TutorialState{completed=experienced,storySeenMask=TutorialPolicy.MetMilestones(migrated)};
            }
            migrated.version = Economy.SaveVersion;''')
edit('Assets/Scripts/Core/GameSession.cs','        public bool Save(){','''        public bool TrySetTutorial(TutorialState tutorial){lock(gate){var candidate=new GameModel(model.Snapshot);candidate.SetTutorial(tutorial);return Publish(candidate);}}
        public bool Save(){''')
edit('Assets/Scripts/Runtime/GameView.cs','public QuestRecord[] quests;}','public QuestRecord[] quests;public TutorialState tutorial;}')
edit('Assets/Scripts/Runtime/GameView.cs','state.lastGrid=raw.lastGrid;state.quests=raw.quests;','state.lastGrid=raw.lastGrid;state.quests=raw.quests;state.tutorial=raw.tutorial;')
edit('Assets/Scripts/Runtime/GameView.cs','Economy.Upgrades[index]','model.Definition(index)')
edit('Assets/Scripts/Runtime/GameView.cs','Economy.Upgrades[nextIndex]','model.Definition(nextIndex)')
(r/'Assets/Scripts/Core/Onboarding.cs').write_text('''using System;
namespace ZeroToMillionaire
{
 [Serializable]public sealed class TutorialState{
  public int version=1,step,storySeenMask;public bool completed,skipped;
  public TutorialState Clone(){return new TutorialState{version=version,step=step,storySeenMask=storySeenMask,completed=completed,skipped=skipped};}
 }
 public static class TutorialPolicy{
  public static readonly string[] Steps={
   "Пока здесь неуютно, но всё можно исправить понемногу. Начнём с пола.",
   "Здесь можно получить игровые монеты и фрагменты вещей. Попробуем один спин.",
   "Для покупки нужны и монеты, и фрагменты именно этой вещи. Посмотрим, сколько уже собрано.",
   "На ремонт пола всего хватает. Нажми «Купить», когда будешь готов.",
   "Уже лучше! Следующая задача уже появилась. Двигайся в своём темпе."};
  public static readonly string[] Stories={
   "У комнаты появилась основа. Теперь легче представить, какой она станет.",
   "Теперь комната выглядит совсем иначе. Осталось устроить место для отдыха.",
   "Теперь есть где отдохнуть и почитать при свете. Ещё табурет, и всё будет на своих местах.",
   "Девять квадратных метров стали уютным домом. Спасибо за помощь."};
  public static bool Valid(TutorialState t){return t!=null&&t.version==1&&t.step>=0&&t.step<=4&&t.storySeenMask>=0&&t.storySeenMask<=15;}
  public static int Step(PlayerState s){
   if(s.tutorial.completed||s.tutorial.skipped)return -1;if(s.purchased[0])return 4;
   var d=Economy.For(s,0);if(s.fragments[0]>=d.FragmentGoal&&s.cash>=d.CashPrice)return 3;
   if(s.spins>0)return 2;return Math.Min(s.tutorial.step,1);
  }
  public static bool CanNext(PlayerState s){int step=Step(s);return step==0||step==4||(step==2&&s.tutorial.step<3);}
  public static TutorialState Advance(PlayerState s){var t=s.tutorial.Clone();if(!CanNext(s))return t;int step=Step(s);if(step==4)t.completed=true;else t.step=step==0?1:3;return t;}
  public static TutorialState Skip(PlayerState s){var t=s.tutorial.Clone();t.skipped=true;return t;}
  public static int MetMilestones(PlayerState s){int owned=0;for(int i=0;i<8;i++)if(s.purchased[i])owned|=1<<i;int[] masks={3,28,96,255};int met=0;for(int i=0;i<4;i++)if((owned&masks[i])==masks[i])met|=1<<i;return met;}
  public static int PendingStory(PlayerState s){int mask=MetMilestones(s)&~s.tutorial.storySeenMask;for(int i=3;i>=0;i--)if((mask&(1<<i))!=0)return i;return -1;}
 }
}
''',encoding='utf-8')
print('Core onboarding, persisted requirements and slow curve written')