using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
namespace ZeroToMillionaire.Tests
{
 public sealed class CriticalInvariantTests
 {
  sealed class ItemRandom:IRandomSource{public int Next(int max){return max>1?1:0;}}
  sealed class MemoryStore:IStateStore{
   public PlayerState Data=new PlayerState();public bool Fail;public int Writes;
   public PlayerState Load(out string notice){notice="";return Data.Clone();}
   public bool Save(PlayerState state,out string error){Writes++;error=Fail?"Simulated interrupted write":"";if(Fail)return false;Data=state.Clone();return true;}
  }
  [TestCase(1000,0,false)][TestCase(0,100,false)][TestCase(18,8,true)]
  public void PurchaseRequiresBothBalances(int cash,int parts,bool allowed){
   var s=new PlayerState{cash=cash};s.fragments[0]=parts;var m=new GameModel(s);
   Assert.AreEqual(allowed,m.CanBuy(0));Assert.AreEqual(allowed,m.TryBuy(0));
   Assert.AreEqual(cash-(allowed?18:0),m.Snapshot.cash);Assert.AreEqual(allowed,m.Snapshot.purchased[0]);
  }
  [Test]public void ConcurrentRepeatedPurchaseCommitsOnce(){
   var s=new PlayerState{cash=1000};s.fragments[0]=100;var m=new GameModel(s);int won=0;
   Parallel.For(0,100,_=>{if(m.TryBuy(0))System.Threading.Interlocked.Increment(ref won);});
   Assert.AreEqual(1,won);Assert.AreEqual(982,m.Snapshot.cash);Assert.AreEqual(92,m.Snapshot.fragments[0]);Assert.AreEqual(1,m.ActiveQuest);
  }
  [Test]public void PoolMovesFromDamageToWallpaperOnlyAfterPurchases(){
   var s=new PlayerState{cash=1000};s.fragments[0]=8;s.fragments[1]=12;var m=new GameModel(s);
   Assert.IsFalse(m.CanSpin);Assert.IsTrue(m.TryBuy(0));Assert.IsFalse(m.CanSpin);Assert.IsTrue(m.TryBuy(1));
   CollectionAssert.AreEqual(new[]{(int)ReelSymbol.Cash,(int)ReelSymbol.Wallpaper},m.ActivePool);Assert.AreEqual(2,m.ActiveQuest);Assert.IsFalse(m.Snapshot.purchased[2]);
  }
  [Test]public void Schema3MigrationPreservesAllBalancesAndOutOfOrderOwnership(){
   var s=new PlayerState{version=3,cash=123,spins=9,recoveries=2,heroineU=1.9f,heroineV=2.3f};s.purchased[5]=true;s.fragments[2]=100;s.quests=Economy.QuestRecords(s);
   Assert.IsTrue(GameModel.IsPersistedValid(s,out _));var migrated=GameModel.Migrate(s);
   Assert.AreEqual(4,migrated.version);Assert.AreEqual(s.cash,migrated.cash);Assert.AreEqual(s.spins,migrated.spins);Assert.AreEqual(s.recoveries,migrated.recoveries);Assert.AreEqual(s.freeSpins,migrated.freeSpins);
   CollectionAssert.AreEqual(s.fragments,migrated.fragments);CollectionAssert.AreEqual(s.purchased,migrated.purchased);Assert.IsTrue(RoomGeometry.IsFree(new FloorPoint(migrated.heroineU,migrated.heroineV)));
   Assert.LessOrEqual(FloorPoint.Distance(new FloorPoint(s.heroineU,s.heroineV),new FloorPoint(migrated.heroineU,migrated.heroineV)),.75f);
  }
  [Test]public void DuplicateQuestIdsCannotBeNormalizedIntoValidProgress(){
   var s=new PlayerState();s.quests[1].id=s.quests[0].id;
   Assert.IsFalse(GameModel.IsPersistedValid(s,out _));Assert.Throws<ArgumentException>(()=>new GameModel(s));
  }
  [Test]public void SaveFailureRollsBackSpinAndPurchaseBeforePresentation(){
   var store=new MemoryStore{Fail=true};var app=new GameSession(store,new ItemRandom());var before=app.Model.Snapshot;
   Assert.IsFalse(app.TryBeginSpin(out var spin));Assert.IsNull(spin);Assert.IsFalse(app.PresentingSpin);Assert.AreEqual(before.cash,app.Model.Snapshot.cash);CollectionAssert.AreEqual(before.fragments,app.Model.Snapshot.fragments);
   store.Fail=false;Assert.IsTrue(app.TryBeginSpin(out spin));Assert.IsFalse(app.TryBeginSpin(out _));Assert.IsFalse(app.TryPurchase(0));app.FinishSpinPresentation();
   var ready=app.Model.Snapshot;store.Fail=true;Assert.IsFalse(app.TryPurchase(0));Assert.AreEqual(ready.cash,app.Model.Snapshot.cash);CollectionAssert.AreEqual(ready.fragments,app.Model.Snapshot.fragments);Assert.AreEqual(0,app.Model.PurchasedCount);
   store.Fail=false;Assert.IsTrue(app.TryPurchase(0));Assert.AreEqual(1,app.Model.PurchasedCount);Assert.IsFalse(app.TryPurchase(0));Assert.AreEqual(app.Model.Snapshot.cash,new GameSession(store,new ItemRandom()).Model.Snapshot.cash);
  }
  [Test]public void FullLevelFinishesAndStopsCreatingTasks(){
   var m=new GameModel();var rng=new SeededRandom(57);int attempts=0;
   while(!m.Complete&&attempts++<1000){int q=m.ActiveQuest;if(m.CanBuy(q))Assert.IsTrue(m.TryBuy(q));else if(m.CanRecover)Assert.IsTrue(m.GrantRecovery());else Assert.IsTrue(m.TrySpin(rng,out _));}
   Assert.IsTrue(m.Complete);Assert.AreEqual(8,m.PurchasedCount);Assert.AreEqual(-1,m.ActiveQuest);Assert.IsFalse(m.CanSpin);Assert.IsFalse(m.TryBuy(7));
  }
 }
}