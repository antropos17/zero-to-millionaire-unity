using System;
using System.IO;
using System.Threading.Tasks;

namespace ZeroToMillionaire
{
    public static class SessionChecks
    {
        sealed class ConstantRandom:IRandomSource
        {public int Next(int max){return max>1?1:0;}}
        sealed class SequenceRandom:IRandomSource
        {int n;public int Next(int max){return n++%max;}}
        sealed class MemoryStore:IStateStore
        {
            public PlayerState Primary=new PlayerState(),Backup;
            public int Calls,FailCall=-1;
            public bool Save(PlayerState state,out string error){
                Calls++;if(Calls==FailCall){error="Simulated interrupted write";return false;}
                Backup=Primary.Clone();Primary=state.Clone();error="";return true;
            }
            public PlayerState Load(out string notice){notice="";return Primary.Clone();}
        }
        static int assertions;
        static void Check(bool condition,string message)
        {assertions++;if(!condition)throw new Exception("SESSION FAILED: "+message);}
        static bool Same(PlayerState a,PlayerState b,IStateCodec codec){return codec.Encode(a)==codec.Encode(b);}

        public static string Run(IStateCodec codec,string directory)
        {
            assertions=0;string error,notice,reason;SpinResult spin;
            // Old active-task economics remain compatible across schema4 migration.
            var initial=new PlayerState{version=4};
            var store=new MemoryStore();var app=new GameSession(store,new ConstantRandom(),initial);
            var before=app.Model.Snapshot;store.FailCall=1;
            Check(!app.TryBeginSpin(out spin)&&spin==null,"failed spin has no displayed result");
            Check(Same(before,app.Model.Snapshot,codec)&&!app.PresentingSpin,"failed spin does not debit, grant, advance or leave lease");
            store.FailCall=-1;
            Check(app.TryBeginSpin(out spin)&&app.PresentingSpin,"spin commits then holds presentation lease");
            Check(store.Primary.spins==1&&store.Primary.cash==26&&store.Primary.fragments[0]==8,"exact spin grid and resources committed before cosmetics");
            SpinResult second;Check(!app.TryBeginSpin(out second),"double spin rejected during presentation");
            Check(!app.TryPurchase(0)&&!app.TryReset()&&!app.TryRecover(),"buy/reset/recovery blocked during presentation");
            var resumed=new GameSession(store,new ConstantRandom());
            Check(resumed.Model.Snapshot.spins==1&&resumed.Model.Snapshot.cash==26&&!resumed.PresentingSpin,"relaunch midspin restores committed result without replay charge");
            Check(resumed.Model.Snapshot.lastGrid[0]==spin.Grid[0]&&resumed.Model.Snapshot.fragments[0]==8,"relaunch keeps exact grid and parts");
            app.FinishSpinPresentation();before=app.Model.Snapshot;store.FailCall=store.Calls+1;
            Check(!app.TryPurchase(0)&&Same(before,app.Model.Snapshot,codec),"failed purchase leaves both balances, ownership and next quest unchanged");
            store.FailCall=-1;int wins=0;
            Parallel.For(0,100,_=>{if(app.TryPurchase(0))System.Threading.Interlocked.Increment(ref wins);});
            Check(wins==1&&app.Model.Snapshot.cash==8&&app.Model.PurchasedCount==1&&app.Model.ActiveQuest==1,"100 repeated/concurrent installs commit once");
            resumed=new GameSession(store,new ConstantRandom());
            Check(resumed.Model.PurchasedCount==1&&resumed.Model.ActiveQuest==1&&resumed.Model.Snapshot.cash==8,"purchase and next quest survive relaunch");

            var zero=new PlayerState{cash=0,freeSpins=0};store=new MemoryStore{Primary=zero};app=new GameSession(store,new ConstantRandom());
            store.FailCall=1;Check(!app.TryRecover()&&app.Model.Snapshot.freeSpins==0&&app.Model.Snapshot.recoveries==0,"failed recovery does not mint allowance");
            store.FailCall=-1;wins=0;Parallel.For(0,30,_=>{if(app.TryRecover())System.Threading.Interlocked.Increment(ref wins);});
            Check(wins==1&&app.Model.Snapshot.freeSpins==1&&app.Model.Snapshot.recoveries==1,"repeated free recovery grants once");
            Check(app.TryBeginSpin(out spin)&&spin.Cost==0,"recovery remains a free progress path");

            var rich=new PlayerState{cash=99};rich.purchased[0]=true;rich.fragments[1]=44;
            store=new MemoryStore{Primary=new GameModel(rich).Snapshot};app=new GameSession(store,new ConstantRandom());
            before=app.Model.Snapshot;store.FailCall=1;
            Check(!app.TryReset()&&Same(before,app.Model.Snapshot,codec),"failed primary reset preserves live progress");
            store.FailCall=store.Calls+2;
            Check(app.TryReset()&&app.Model.PurchasedCount==0&&app.Model.Snapshot.freeSpins==3,"second reset-save failure keeps already committed fresh model");
            Check(store.Primary.cash==0&&store.Primary.spins==0&&app.Notice.Contains("backup"),"primary reset is durable and backup failure is explicit");
            resumed=new GameSession(store,new ConstantRandom());
            Check(resumed.Model.PurchasedCount==0&&resumed.Model.Snapshot.cash==0,"relaunch after backup failure keeps reset");
            store.FailCall=-1;Check(app.Save()&&store.Backup.cash==0&&!store.Backup.purchased[0],"later save repairs reset backup");

            // Raw schema3 records must be rejected before statuses are normalized.
            var good=new GameModel(rich).Snapshot;
            Directory.CreateDirectory(directory);
            string path=Path.Combine(directory,"session-save.json");
            foreach(string suffix in new[]{"",".bak",".tmp"})if(File.Exists(path+suffix))File.Delete(path+suffix);
            var disk=new SaveStore(path,codec);
            Check(disk.Save(good,out error)&&disk.Save(good,out error),"valid primary and backup established");
            Action<PlayerState>[] corrupt={
                s=>s.quests[1].id=s.quests[0].id,
                s=>s.quests[1].id="unknown-renovation",
                s=>s.quests[1].id="FLOOR-PATCHES",
                s=>s.quests[1]=null,
                s=>s.quests=new QuestRecord[7],
                s=>s.quests=null,
                s=>s.quests[1].status=(QuestStatus)99
            };
            foreach(var mutation in corrupt){
                var invalid=good.Clone();mutation(invalid);
                Check(!GameModel.IsPersistedValid(invalid,out reason),"raw ID/shape corruption rejected");
                Check(!GameModel.IsValid(GameModel.Migrate(invalid),out reason),"migration cannot repair away corrupt IDs");
                File.WriteAllText(path,codec.Encode(invalid));
                var recovered=disk.Load(out notice);
                Check(Same(good,recovered,codec)&&notice.Contains("Recovered"),"corrupt quest shape selects exact valid backup");
            }
            string missing="{\"version\":3,\"cash\":0,\"freeSpins\":3,\"spins\":0,\"recoveries\":0,\"heroineU\":1.85,\"heroineV\":1.45,\"fragments\":[0,0,0,0,0,0,0,0],\"purchased\":[false,false,false,false,false,false,false,false],\"lastGrid\":[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]}";
            File.WriteAllText(path,missing);
            Check(Same(good,disk.Load(out notice),codec)&&notice.Contains("Recovered"),"missing quests JSON field is not filled with constructor defaults");
            Check(disk.Save(good,out error),"primary restored before interruption test");
            File.WriteAllText(path+".tmp","interrupted temporary bytes");
            Check(Same(good,disk.Load(out notice),codec),"orphaned partial temp cannot replace a committed save");

            // Real file-sharing failure tests the atomic writer, not only a fake.
            var collecting=good.Clone();collecting.fragments[1]=1;collecting.quests=Economy.QuestRecords(collecting);
            Check(disk.Save(collecting,out error),"collecting primary established for actual write interruption");
            app=new GameSession(disk,new ConstantRandom());before=app.Model.Snapshot;
            Check(app.Model.CanSpin,"interruption test really reaches the disk writer");
            using(var held=new FileStream(path+".tmp",FileMode.OpenOrCreate,FileAccess.Write,FileShare.None)){
                Check(!app.TryBeginSpin(out spin)&&Same(before,app.Model.Snapshot,codec)&&app.Error.Contains("Save failed"),"locked temp write cannot publish candidate resources");
            }
            Check(Same(collecting,disk.Load(out notice),codec),"interrupted actual write leaves original primary intact");

            var legacy=good.Clone();legacy.version=2;legacy.quests=null;legacy.purchased[6]=true;legacy.fragments[0]=91;
            File.WriteAllText(path,codec.Encode(legacy));resumed=new GameSession(disk,new ConstantRandom());
            Check(resumed.Model.Snapshot.fragments[0]==91&&resumed.Model.Snapshot.fragments[1]==44&&resumed.Model.Snapshot.purchased[6]&&resumed.Model.ActiveQuest==1,"legacy surplus and out-of-order ownership preserved by session reload");
            Check(resumed.Save()&&new GameSession(disk,new ConstantRandom()).Model.Snapshot.version==Economy.SaveVersion,"migrated stable records commit and reload");

            var nearLimit=new GameModel(new PlayerState{cash=int.MaxValue-26,freeSpins=0});
            Check(nearLimit.TrySpin(new SequenceRandom(),out spin)&&nearLimit.Snapshot.cash==int.MaxValue-3,"valid near-limit NET cash accepted");
            nearLimit=new GameModel(new PlayerState{cash=int.MaxValue-20,freeSpins=0});before=nearLimit.Snapshot;
            Check(!nearLimit.TrySpin(new SequenceRandom(),out spin)&&Same(before,nearLimit.Snapshot,codec),"overflow rejects entire transaction without mutation");
            foreach(string suffix in new[]{"",".bak",".tmp"})if(File.Exists(path+suffix))File.Delete(path+suffix);
            return "PASS "+assertions+" offline session checks; raw IDs/shape, interruption/rollback, concurrent taps, relaunch, reset backup and net overflow";
        }
    }
}
