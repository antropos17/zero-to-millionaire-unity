using System;
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
