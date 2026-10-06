using System;
using NUnit.Framework;
namespace ZeroToMillionaire.Tests
{
 public sealed class OnboardingTests
 {
  sealed class CashRandom:IRandomSource{public int Next(int max){return 0;}}
  sealed class Store:IStateStore{public PlayerState data=new PlayerState();public bool fail;public PlayerState Load(out string notice){notice="";return data.Clone();}public bool Save(PlayerState value,out string error){error=fail?"failed":"";if(fail)return false;data=value.Clone();return true;}}
  [Test]public void FreshNextThenSpinThenMetersResume(){
   var s=new PlayerState();Assert.AreEqual(0,TutorialPolicy.Step(s));s.tutorial=TutorialPolicy.Advance(s);Assert.AreEqual(1,TutorialPolicy.Step(s));
   var m=new GameModel(s);Assert.IsTrue(m.TrySpin(new CashRandom(),out var result));Assert.AreEqual(1,result.Fragments[0]);s=m.Snapshot;Assert.AreEqual(2,TutorialPolicy.Step(s));s.tutorial=TutorialPolicy.Advance(s);
   Assert.AreEqual(2,TutorialPolicy.Step(s));Assert.IsFalse(TutorialPolicy.CanNext(s));Assert.AreEqual(3,new GameModel(s).Snapshot.tutorial.step);
  }
  [Test]public void DialogueMutationsCannotGrantResourcesAndPersistSkip(){
   var store=new Store();var session=new GameSession(store,new CashRandom());var s=session.Model.Snapshot;
   Assert.IsTrue(session.TrySetTutorial(TutorialPolicy.Advance(s)));Assert.AreEqual(s.cash,session.Model.Snapshot.cash);Assert.AreEqual(s.spins,session.Model.Snapshot.spins);CollectionAssert.AreEqual(s.fragments,session.Model.Snapshot.fragments);
   Assert.IsTrue(session.TrySetTutorial(TutorialPolicy.Skip(session.Model.Snapshot)));Assert.AreEqual(-1,TutorialPolicy.Step(new GameSession(store,new CashRandom()).Model.Snapshot));
   store.fail=true;Assert.IsFalse(session.TrySetTutorial(new TutorialState()));Assert.IsTrue(session.Model.Snapshot.tutorial.skipped);
  }
  [Test]public void ReadyAndCommittedPurchaseSelectRelevantTutorial(){
   var s=new PlayerState{cash=1000};s.fragments[0]=20;var m=new GameModel(s);Assert.AreEqual(3,TutorialPolicy.Step(m.Snapshot));Assert.IsFalse(TutorialPolicy.CanNext(m.Snapshot));
   Assert.IsTrue(m.TryBuy(0));Assert.AreEqual(4,TutorialPolicy.Step(m.Snapshot));m.SetTutorial(TutorialPolicy.Advance(m.Snapshot));Assert.AreEqual(-1,TutorialPolicy.Step(m.Snapshot));Assert.AreEqual(1,m.PurchasedCount);
  }
  [Test]public void ExistingReadyTaskKeepsOldRequirementsAndNoForcedTutorial(){
   var s=new PlayerState{version=4,cash=18};s.fragments[0]=8;foreach(var q in s.quests){q.cashPrice=q.fragmentGoal=0;}s.quests[0].status=QuestStatus.ReadyToBuy;s.tutorial=null;
   var m=new GameModel(s);Assert.IsTrue(m.CanBuy(0));Assert.AreEqual(18,m.Definition(0).CashPrice);Assert.AreEqual(8,m.Definition(0).FragmentGoal);Assert.IsTrue(m.Snapshot.tutorial.completed);
   Assert.IsTrue(m.TryBuy(0));Assert.AreEqual(30,m.Definition(1).FragmentGoal);Assert.AreEqual(115,m.Definition(1).CashPrice);Assert.AreEqual(0,m.Snapshot.cash);
  }
  [Test]public void ExistingOutOfOrderAndExcessPreservedAndPastStoryViewed(){
   var s=new PlayerState{version=4,cash=400,spins=9};s.purchased[0]=s.purchased[1]=s.purchased[5]=true;s.fragments[2]=100;foreach(var q in s.quests){q.cashPrice=q.fragmentGoal=0;}s.tutorial=null;
   var m=new GameModel(s);Assert.AreEqual(400,m.Snapshot.cash);Assert.AreEqual(100,m.Snapshot.fragments[2]);Assert.IsTrue(m.Snapshot.purchased[5]);Assert.AreEqual(2,m.ActiveQuest);Assert.AreEqual(-1,TutorialPolicy.PendingStory(m.Snapshot));
   Assert.IsTrue(m.TryBuy(2));Assert.AreEqual(86,m.Snapshot.fragments[2]);Assert.AreEqual(320,m.Snapshot.cash);
  }
  [Test]public void MilestonesAreFiniteAndSeenMaskIsIdempotent(){
   var s=new PlayerState();s.tutorial.completed=true;s.purchased[0]=s.purchased[1]=true;Assert.AreEqual(0,TutorialPolicy.PendingStory(s));s.tutorial.storySeenMask|=TutorialPolicy.MetMilestones(s);Assert.AreEqual(-1,TutorialPolicy.PendingStory(s));
   for(int i=0;i<8;i++)s.purchased[i]=true;Assert.AreEqual(3,TutorialPolicy.PendingStory(s));s.tutorial.storySeenMask|=TutorialPolicy.MetMilestones(s);Assert.AreEqual(-1,TutorialPolicy.PendingStory(new GameModel(s).Snapshot));
  }
  [Test]public void AllCashRollStillMakesFiniteVisiblePartProgressWithoutSoftlock(){
   var m=new GameModel();int n=0;while(!m.CanBuy(0)&&n++<100){if(m.CanRecover)m.GrantRecovery();Assert.IsTrue(m.TrySpin(new CashRandom(),out var spin));Assert.AreEqual(1,spin.Fragments[0]);Assert.AreEqual(1,Array.FindAll(spin.Grid,x=>x==(int)ReelSymbol.FloorPatch).Length);}
   Assert.IsTrue(m.CanBuy(0));Assert.AreEqual(20,m.Snapshot.fragments[0]);Assert.IsTrue(m.TryBuy(0));Assert.IsFalse(m.TryBuy(0));
  }
 }
}