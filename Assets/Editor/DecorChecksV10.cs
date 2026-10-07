using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ZeroToMillionaire
{
    public static class DecorChecksV10
    {
        static int checks;static void Check(bool ok,string text){System.Threading.Interlocked.Increment(ref checks);if(!ok)throw new Exception("Decor/schema7: "+text);}
        sealed class MemoryStore:ILevel1StoreV7
        {
            public Level1StateV7 State=new Level1StateV7();public bool Fail;public int Writes,FailAt;public bool RecoveryRequired=>false;
            public Level1StateV7 Load(out string notice){notice="";return State.Clone();}
            public bool Save(Level1StateV7 s,out string error){Writes++;bool failed=Fail||Writes==FailAt;error=failed?"private write fault":"";if(failed)return false;State=s.Clone();return true;}
        }
        sealed class FaultyLegacy:IStateCodec{public IStateCodec Inner;public bool Fail;public string Encode(PlayerState s)=>Inner.Encode(s);public PlayerState Decode(string s){if(Fail)throw new InvalidOperationException("private classify fault");return Inner.Decode(s);}}
        sealed class CashOnly:IRandomSource{public int Next(int n)=>0;}
        sealed class SymbolOnly:IRandomSource{public int Next(int n)=>n-1;}
        sealed class BadRandom:IRandomSource{public int Next(int n)=>n;}
        static PlayerState Base(int cash=0,int stake=1,int owned=8)
        {
            var s=new PlayerState{cash=cash,stakeMultiplier=stake,freeSpins=0,spins=58,recoveries=2};for(int i=0;i<owned;i++)s.purchased[i]=true;s.tutorial.completed=true;s.tutorial.storySeenMask=15;s.quests=Economy.QuestRecords(s);return s;
        }
        static Level1StateV7 Active(int cash=0,int stake=1){var s=Level1StateV7.MigrateLegacy(Base(cash,stake));s.decorStarted=true;return s;}
        static bool Bytes(byte[] a,byte[] b){if(a==null||b==null)return a==b;if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;i++)if(a[i]!=b[i])return false;return true;}
        public static string Run(ILevel1CodecV7 codec,IStateCodec legacy,string directory)
        {
            checks=0;string root=Path.Combine(directory,"decor-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
            Check(Economy.SaveVersion==6,"verified runtime schema6 remains unchanged");
            var original=Base(367,3);original.lastGrid[0]=8;var migrated=Level1StateV7.MigrateLegacy(original);var model=new Level1ModelV7(migrated);
            Check(model.Complete&&model.PurchasedCount==8&&!model.DecorComplete&&model.ActiveDecorQuest==-1&&!model.CanSpin,"schema6 completion remains8/8 with inactive optional target");
            Check(migrated.cash==367&&migrated.spins==58&&migrated.stakeMultiplier==3&&migrated.lastGrid[0]==8&&!migrated.decorStarted,"schema6 cash/stake/spins/grid preserved");
            Check(model.TryStartDecorating()&&!model.TryStartDecorating()&&model.Complete&&model.ActiveDecorQuest==0,"explicit start only once and no base completion regression");
            Check(!new Level1ModelV7(Level1StateV7.MigrateLegacy(Base(0,1,7))).TryStartDecorating(),"optional start locked before8/8");
            var escaped=model.Snapshot;escaped.decorPurchased[0]=true;Check(!model.Snapshot.decorPurchased[0],"deep snapshot isolation");
            var stalled=new Level1ModelV7(Active());Check(!stalled.CanSpin&&stalled.CanRecover&&stalled.TryRecover()&&!stalled.TryRecover(),"zero-cash optional recovery path");
            SpinResult reward;Check(stalled.TrySpin(new CashOnly(),out reward)&&reward.Free&&reward.RewardMultiplier==1&&reward.Grid[14]==9&&reward.Fragments[8]==1,"free recovery one visible matching icon equals one part");
            for(int stake=1;stake<=3;stake++)
            {
                var paid=new Level1ModelV7(Active(100,stake));Check(paid.TrySpin(new SymbolOnly(),out reward),"paid decor spin");
                int cashIcons=0,parts=0,triples=0;foreach(int symbol in reward.Grid){Check(symbol==0||symbol==9,"pool only cash/current appended symbol9");if(symbol==0)cashIcons++;else parts++;}
                for(int x=0;x<5;x++)if(reward.Grid[x]==reward.Grid[x+5]&&reward.Grid[x]==reward.Grid[x+10])triples++;
                Check(reward.Cost==5*stake&&reward.Payout==(12+2*cashIcons+10*triples)*stake&&reward.Fragments[8]==parts&&paid.Snapshot.decorFragments[0]==parts,"paid stake scales only cash");
                Check(paid.Complete&&paid.PurchasedCount==8,"decor spin does not unsetbase completion");
            }
            var caps=Active(100);caps.decorFragments[0]=17;var capped=new Level1ModelV7(caps);Check(capped.TrySpin(new SymbolOnly(),out reward)&&reward.Fragments[8]==1&&Array.FindAll(reward.Grid,x=>x==9).Length==1,"goal cap converts extra icons to cash without invisible surplus");
            Check(capped.CanBuyDecor(0)&&!capped.CanSpin&&!capped.CanBuyDecor(1),"both-resource current-only gate and explicit buy");
            var both=Active(89);both.decorFragments[0]=18;Check(!new Level1ModelV7(both).TryDecorPurchase(0),"parts-only cannot buy");both.cash=90;both.decorFragments[0]=17;Check(!new Level1ModelV7(both).TryDecorPurchase(0),"cash-only cannot buy");both.decorFragments[0]=18;
            var repeated=new Level1ModelV7(both);int successes=0;Parallel.For(0,100,_=>{if(repeated.TryDecorPurchase(0))System.Threading.Interlocked.Increment(ref successes);});Check(successes==1&&repeated.Snapshot.cash==0&&repeated.Snapshot.decorFragments[0]==0&&repeated.DecorPurchasedCount==1,"concurrent double-buy/overdraw blocked");Check(!repeated.TryDecorPurchase(-1)&&!repeated.TryDecorPurchase(4),"invalid purchase indices rejected");
            var invalid=Active();invalid.decorPurchased[1]=true;string reason;Check(!Level1StateV7.Valid(invalid,out reason),"out-of-order ownership rejected");invalid=Active();invalid.decorFragments[1]=1;Check(!Level1StateV7.Valid(invalid,out reason),"unearned future item parts rejected");
            invalid=Active(int.MaxValue);var overflow=new Level1ModelV7(invalid);var before=overflow.Snapshot;Check(!overflow.TrySpin(new CashOnly(),out reward)&&Level1StoreV7.EqualCanonical(before,overflow.Snapshot),"cash overflow rollback");
            var bad=new Level1ModelV7(Active(100));before=bad.Snapshot;bool threw=false;try{bad.TrySpin(new BadRandom(),out reward);}catch(ArgumentOutOfRangeException){threw=true;}Check(threw&&Level1StoreV7.EqualCanonical(before,bad.Snapshot),"bad random cannot partly mutate resources");
            var memory=new MemoryStore{State=Active(90)};memory.State.decorFragments[0]=18;var session=new Level1SessionV7(memory,new CashOnly());memory.Fail=true;before=session.Model.Snapshot;Check(!session.TryDecorPurchase(0)&&Level1StoreV7.EqualCanonical(before,session.Model.Snapshot),"failed purchase commit not published");int writes=memory.Writes;string error=session.Error;Check(session.TrySelectStake(1)&&memory.Writes==writes&&session.Error==error,"stake no-op retains failure without write");memory.Fail=false;Check(session.TryDecorPurchase(0)&&session.Error==""&&memory.State.decorPurchased[0],"retry publishes ownership only after durable success");
            memory=new MemoryStore{State=Active(100)};session=new Level1SessionV7(memory,new SymbolOnly());Check(session.TryBeginSpin(out reward),"guarded spin starts");Check(!session.TryBeginSpin(out reward)&&!session.TryDecorPurchase(0)&&!session.TrySelectStake(2),"presentation lease gates repeated spin/buy/stake");session.FinishSpinPresentation();
            var pacing=new List<int>();
            for(int stake=1;stake<=3;stake++)for(int seed=0;seed<1000;seed++)
            {
                var game=new Level1ModelV7(Active(seed%20,stake));var rng=new SeededRandom(seed);int spins=0;
                while(!game.DecorComplete&&spins<200){if(!game.CanSpin&&game.CanRecover)Check(game.TryRecover(),"simulated recovery");if(!game.CanSpin&&game.Snapshot.cash>=5&&game.Snapshot.cash<5*game.Snapshot.stakeMultiplier)Check(game.TrySelectStake(1),"transparent lower-stake recovery from unaffordable selected stake");if(game.CanSpin){Check(game.TrySpin(rng,out reward),"simulated decor spin");spins++;}for(int i=0;i<4;i++)game.TryDecorPurchase(i);Check(game.Complete&&game.PurchasedCount==8&&game.Snapshot.cash>=0,"base8/8 and nonnegative cash throughout");}
                Check(game.DecorComplete&&game.DecorPurchasedCount==4,"all four optional items complete");pacing.Add(spins);
            }
            // Exact original pairs are copied before the first schema7 write; loading never writes.
            for(int version=1;version<=6;version++)
            {
                string dir=Path.Combine(root,"legacy"+version);Directory.CreateDirectory(dir);string path=Path.Combine(dir,"level1-v1.json");var old=Base(300,version>=6?3:1);old.version=version;if(version==1)Array.Clear(old.lastGrid,0,15);
                byte[] raw=new UTF8Encoding(true).GetPreamble();byte[] json=Encoding.UTF8.GetBytes(legacy.Encode(old));byte[] bytes=new byte[raw.Length+json.Length];Buffer.BlockCopy(raw,0,bytes,0,raw.Length);Buffer.BlockCopy(json,0,bytes,raw.Length,json.Length);File.WriteAllBytes(path,bytes);
                var backupOld=Base(200);backupOld.version=version;File.WriteAllText(path+".bak",legacy.Encode(backupOld));var bak=File.ReadAllBytes(path+".bak");string pre6=path+".pre-schema6";Directory.CreateDirectory(pre6);File.WriteAllText(Path.Combine(pre6,"keep.txt"),"old snapshot remains");
                var store=new Level1StoreV7(path,codec,legacy);var loaded=store.Load(out _);Check(!store.RecoveryRequired&&Bytes(bytes,File.ReadAllBytes(path))&&Bytes(bak,File.ReadAllBytes(path+".bak")),"legacy load does not migrate on disk");
                Check(loaded.version==7&&!loaded.decorStarted&&loaded.cash==300&&Array.TrueForAll(loaded.decorPurchased,b=>!b),"schemas1–6 initialize optional data inactive");
                Check(store.Save(loaded,out error),"schema7 migration durable commit: "+error);Check(Bytes(bytes,File.ReadAllBytes(Path.Combine(store.PreMigrationSnapshotPath,"primary.json")))&&Bytes(bak,File.ReadAllBytes(Path.Combine(store.PreMigrationSnapshotPath,"backup.json"))),"exact primary/backup includingBOM preserved");
                Check(File.ReadAllText(Path.Combine(pre6,"keep.txt"))=="old snapshot remains","pre-schema6 history untouched");var reopened=new Level1StoreV7(path,codec,legacy);Check(Level1StoreV7.EqualCanonical(loaded,reopened.Load(out _)),"schema7 reload matches migrated values");Check(reopened.Save(loaded,out error)&&Bytes(bytes,File.ReadAllBytes(Path.Combine(store.PreMigrationSnapshotPath,"primary.json"))),"repeated commit retains nonrotating original snapshot");
            }
            string blockedPath=Path.Combine(root,"copy-failure","level1-v1.json");Directory.CreateDirectory(Path.GetDirectoryName(blockedPath));var six=Encoding.UTF8.GetBytes(legacy.Encode(Base(50,3)));File.WriteAllBytes(blockedPath,six);File.WriteAllText(blockedPath+".pre-schema7","owned private snapshot blocker");var blockedStore=new Level1StoreV7(blockedPath,codec,legacy);var candidate=blockedStore.Load(out _);candidate.decorStarted=true;Check(!blockedStore.Save(candidate,out error)&&error==Level1StoreV7.SnapshotError&&Bytes(six,File.ReadAllBytes(blockedPath)),"required snapshot-copy failure refuses replacement");
            string changedPath=Path.Combine(root,"changed","level1-v1.json");Directory.CreateDirectory(Path.GetDirectoryName(changedPath));File.WriteAllBytes(changedPath,six);var changedStore=new Level1StoreV7(changedPath,codec,legacy);candidate=changedStore.Load(out _);File.WriteAllText(changedPath,"{\"version\":8}");var futureBytes=File.ReadAllBytes(changedPath);Check(!changedStore.Save(candidate,out error)&&error==Level1StoreV7.ChangedError&&Bytes(futureBytes,File.ReadAllBytes(changedPath)),"external future original cannot be overwritten");var futureStore=new Level1StoreV7(changedPath,codec,legacy);futureStore.Load(out _);Check(futureStore.RecoveryRequired&&!futureStore.Save(candidate,out error),"future-version load fails closed");
            string backupPath=Path.Combine(root,"backup-source","level1-v1.json");Directory.CreateDirectory(Path.GetDirectoryName(backupPath));File.WriteAllBytes(backupPath+".bak",six);var backupStore=new Level1StoreV7(backupPath,codec,legacy);candidate=backupStore.Load(out _);Check(!backupStore.RecoveryRequired&&backupStore.Save(candidate,out error)&&Bytes(six,File.ReadAllBytes(Path.Combine(backupStore.PreMigrationSnapshotPath,"backup.json")))&&!File.Exists(Path.Combine(backupStore.PreMigrationSnapshotPath,"primary.json")),"absent-primary backup source copied exactly");
            string unreadPath=Path.Combine(root,"unreadable","level1-v1.json");Directory.CreateDirectory(Path.GetDirectoryName(unreadPath));File.WriteAllBytes(unreadPath,six);using(var lease=new FileStream(unreadPath,FileMode.Open,FileAccess.Read,FileShare.None)){var u=new Level1StoreV7(unreadPath,codec,legacy);u.Load(out _);Check(u.RecoveryRequired&&!u.Save(candidate,out error),"unreadable originals cannot be treated as absent");}
            string failedCommitPath=Path.Combine(root,"commit-failure","level1-v1.json");Directory.CreateDirectory(Path.GetDirectoryName(failedCommitPath));File.WriteAllBytes(failedCommitPath,six);File.WriteAllBytes(failedCommitPath+".bak",six);
            var failedCommitStore=new Level1StoreV7(failedCommitPath,codec,legacy);var failedCommitSession=new Level1SessionV7(failedCommitStore,new CashOnly());before=failedCommitSession.Model.Snapshot;
            using(var lease=new FileStream(failedCommitPath,FileMode.Open,FileAccess.Read,FileShare.Read))Check(!failedCommitSession.TryStartDecorating()&&Level1StoreV7.EqualCanonical(before,failedCommitSession.Model.Snapshot)&&Bytes(six,File.ReadAllBytes(failedCommitPath))&&Bytes(six,File.ReadAllBytes(failedCommitPath+".bak")),"replace/sharing failure after snapshot/temp does not publish or touch either original");
            Check(failedCommitSession.TryStartDecorating()&&failedCommitSession.Model.Snapshot.decorStarted,"commit retry after sharing lease released");
            string classifyPath=Path.Combine(root,"classification-failure","level1-v1.json");Directory.CreateDirectory(Path.GetDirectoryName(classifyPath));File.WriteAllBytes(classifyPath,six);var faulty=new FaultyLegacy{Inner=legacy};var classifyStore=new Level1StoreV7(classifyPath,codec,faulty);candidate=classifyStore.Load(out _);faulty.Fail=true;
            Check(!classifyStore.Save(candidate,out error)&&error==Level1StoreV7.SnapshotError&&Bytes(six,File.ReadAllBytes(classifyPath)),"codec/classification fault cannot infer copying unnecessary");
            string tamperPath=Path.Combine(root,"tampered-snapshot","level1-v1.json");Directory.CreateDirectory(Path.GetDirectoryName(tamperPath));File.WriteAllBytes(tamperPath,six);var tamper=new Level1StoreV7(tamperPath,codec,legacy);candidate=tamper.Load(out _);Check(tamper.Save(candidate,out error),"private snapshot prepared before tamper check");
            string snapshot=Path.Combine(tamper.PreMigrationSnapshotPath,"primary.json");File.WriteAllText(snapshot,"wrong copied bytes");File.WriteAllBytes(tamperPath,six);if(File.Exists(tamperPath+".bak"))File.Delete(tamperPath+".bak");tamper=new Level1StoreV7(tamperPath,codec,legacy);candidate=tamper.Load(out _);
            Check(!tamper.Save(candidate,out error)&&error==Level1StoreV7.SnapshotError&&Bytes(six,File.ReadAllBytes(tamperPath)),"existing nonrotating snapshot mismatch refuses replacement");
            string futureBakPath=Path.Combine(root,"future-backup","level1-v1.json");Directory.CreateDirectory(Path.GetDirectoryName(futureBakPath));File.WriteAllBytes(futureBakPath,six);File.WriteAllText(futureBakPath+".bak","{\"version\":8}");var futureBak=new Level1StoreV7(futureBakPath,codec,legacy);candidate=futureBak.Load(out _);
            Check(futureBak.RecoveryRequired&&!futureBak.Save(candidate,out error)&&Bytes(six,File.ReadAllBytes(futureBakPath)),"future backup blocks schema7 write despite valid legacy primary");
            foreach(int value in new[]{1,2,3}){var legacyStake=Base(90,value);var s=Level1StateV7.MigrateLegacy(legacyStake);Check(s.stakeMultiplier==value&&s.cash==legacyStake.cash&&s.spins==legacyStake.spins&&s.recoveries==legacyStake.recoveries,"schema6 stake/counters preserved"+value);}
            memory=new MemoryStore{State=Active(90),FailAt=2};session=new Level1SessionV7(memory,new CashOnly());Check(session.TryReset(true)&&session.Model.PurchasedCount==0&&!session.Model.Snapshot.decorStarted&&memory.State.freeSpins==3&&session.Notice.Contains("backup update pending"),"secondary reset backup failure cannot resurrect old live state");
            string validText=codec.Encode(Active(100));Check(Schema7Shape.Valid(validText)&&codec.Decode(validText)!=null,"strict schema7 shape roundtrip");
            foreach(string key in new[]{"decorStarted","decorFragments","decorPurchased","cash","spins","stakeMultiplier","heroineU","tutorial"})
            {
                // Remove a required field with a strict structural helper supplied by the test codec.
                string wrong=validText.Replace("\""+key+"\"","\"removed-"+key+"\"");Check(!Schema7Shape.Valid(wrong)&&codec.Decode(wrong)==null,"missing mandatory field refused: "+key);
            }
            Check(!Schema7Shape.Valid(validText.TrimEnd().Substring(0,validText.TrimEnd().Length-1)+",\"cash\":0}"),"duplicate keys refused");Check(!Schema7Shape.Valid(validText+" true"),"trailing JSON refused");Check(!Schema7Shape.Valid(validText.TrimEnd().Substring(0,validText.TrimEnd().Length-1)+",\"futureExtra\":1}"),"unknown schema7 fields fail closed instead of silently dropping future data");
            string cyclePath=Path.Combine(root,"committed-cycle","level1-v1.json");Directory.CreateDirectory(Path.GetDirectoryName(cyclePath));File.WriteAllBytes(cyclePath,Encoding.UTF8.GetBytes(legacy.Encode(Base(0,3))));
            var cycleStore=new Level1StoreV7(cyclePath,codec,legacy);var cycle=new Level1SessionV7(cycleStore,new SeededRandom(31));before=cycle.Model.Snapshot;
            Check(!cycle.TryBeginSpin(out reward)&&Level1StoreV7.EqualCanonical(before,cycle.Model.Snapshot),"completed legacy level does not silently resume spins");Check(cycle.TryStartDecorating(),"committed explicit start protects legacy original");
            while(!cycle.Model.DecorComplete)
            {
                if(cycle.Model.CanRecover)Check(cycle.TryRecover(),"committed zero-cash recovery");
                var s=cycle.Model.Snapshot;if(!cycle.Model.CanSpin&&s.cash>=5&&s.cash<5*s.stakeMultiplier)Check(cycle.TrySelectStake(1),"committed lower stake");
                if(cycle.Model.CanSpin){Check(cycle.TryBeginSpin(out reward),"committed optional spin");cycle.FinishSpinPresentation();}
                int q=cycle.Model.ActiveDecorQuest;if(cycle.Model.CanBuyDecor(q)){before=cycle.Model.Snapshot;var d=DecorEconomyV7.Items[q];Check(cycle.TryDecorPurchase(q)&&!cycle.TryDecorPurchase(q),"committed purchase once");Check(cycle.Model.Snapshot.cash==before.cash-d.CashPrice&&cycle.Model.Snapshot.decorFragments[q]==0,"exact shared-cash/own-parts debit");}
                var reloaded=new Level1StoreV7(cyclePath,codec,legacy).Load(out _);Check(Level1StoreV7.EqualCanonical(cycle.Model.Snapshot,reloaded),"every optional commit survives actual file reload");
            }
            before=cycle.Model.Snapshot;Check(before.version==7&&cycle.Model.Complete&&cycle.Model.DecorPurchasedCount==4&&!cycle.Model.CanSpin,"optional complete retains base8/8");Check(!cycle.TryReset(false)&&Level1StoreV7.EqualCanonical(before,cycle.Model.Snapshot),"unconfirmed reset changes nothing");Check(cycle.TryReset(true)&&cycle.Model.Snapshot.freeSpins==3&&!cycle.Model.Snapshot.decorStarted&&cycle.Model.PurchasedCount==0,"confirmed reset clears base/optional resources");Check(new Level1StoreV7(cyclePath,codec,legacy).Load(out _).spins==0,"reset survives reload");
            Check(Directory.GetFiles(root,"*.schema7.tmp",SearchOption.AllDirectories).Length==0,"owned temporary files cleaned");pacing.Sort();
            return "PASS "+checks+" decor/schema7 assertions; 3000optional completions preservingbase8/8; decor spins min="+pacing[0]+" median="+pacing[1500]+" p95="+pacing[2850]+" max="+pacing[2999]+"; all files private; runtime/art integration absent";
        }
    }
}
