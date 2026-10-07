using System;
namespace ZeroToMillionaire
{
 [Serializable]public sealed class TutorialState{
  public int version=1,step,storySeenMask;public bool completed,skipped;
  public TutorialState Clone(){return new TutorialState{version=version,step=step,storySeenMask=storySeenMask,completed=completed,skipped=skipped};}
 }
 public static class TutorialPolicy{
  public static readonly string[] Steps={
   "It is rough in here, but we can fix it little by little. Let us start with the floor.",
   "Spins give coins and parts for the current upgrade. Try your first free spin.",
   "Every upgrade needs BOTH coins and matching parts. Check the two progress bars below.",
   "You have enough to patch the floor. Press BUY whenever you are ready.",
   "That is better! Your next task is ready. Keep going at your own pace."};
  public static readonly string[] Stories={
   "The floor and walls feel solid again. Now I can picture what this room could become.",
   "What a difference! The room is ready. Next, I need somewhere to rest.",
   "A place to sleep and a little light. One simple stool will finish our small room.",
   "Nine square metres have become a cosy home. Thank you for helping me."};
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
