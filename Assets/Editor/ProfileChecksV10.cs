using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ZeroToMillionaire
{
    public static class ProfileChecksV10
    {
        static int assertions;
        static void Check(bool ok,string message){System.Threading.Interlocked.Increment(ref assertions);if(!ok)throw new Exception("v10 profile: "+message);}
        sealed class FailingCodec : ILocalPlayerCodec
        {
            public ILocalPlayerCodec Inner;public bool Fail;
            public string Encode(LocalPlayer p){if(Fail)throw new ArgumentException("private codec fault");return Inner.Encode(p);}
            public LocalPlayer Decode(string s)=>Inner.Decode(s);
        }
        static bool Equal(byte[] a,byte[] b){if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;i++)if(a[i]!=b[i])return false;return true;}
        public static string Run(ILocalPlayerCodec codec,string directory)
        {
            assertions=0;string root=Path.Combine(directory,"v10-profile-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
            var faulty=new FailingCodec{Inner=codec};var store=new LocalPlayerStore(root,faulty);string file=Path.Combine(root,"local-player-v1.json");bool changed;
            Check(!store.Blocked&&store.Snapshot.displayName=="Player"&&!File.Exists(file),"defaults have no launch writes");
            var escaped=store.Snapshot;escaped.displayName="Foreign";Check(store.Snapshot.displayName=="Player","snapshot cannot mutate committed profile");
            string normalized;
            foreach(string name in new[]{"Player","O'Brien","Anne-Marie","  Sofia  ","Ирина","Zoë","A\u0301","123"})Check(LocalPlayer.NormalizeName(name,out normalized),"valid international name "+name);
            foreach(string name in new[]{"","  ","<b>A</b>","A\nB","A\u202eB","😀",new string('a',21),new string('a',41)})Check(!LocalPlayer.NormalizeName(name,out normalized),"invalid name rejected");
            var candidate=store.Snapshot;candidate.displayName="Sofia";candidate.avatarId="lamp";candidate.animationMode="calm";candidate.notificationMode="off";
            Check(store.Save(candidate,out changed)&&changed&&store.Snapshot.revision==1,"first atomic profile commit");
            byte[] first=File.ReadAllBytes(file);var reload=new LocalPlayerStore(root,codec);
            Check(reload.Snapshot.SameValues(candidate)&&reload.Snapshot.revision==1,"profile/name/avatar/calm/notices survive relaunch");
            faulty.Fail=true;var failed=store.Snapshot;failed.displayName="Nope";
            Check(!store.Save(failed,out changed)&&!changed&&store.Snapshot.displayName=="Sofia"&&Equal(first,File.ReadAllBytes(file)),"failed codec leaves disk/visible state unchanged");
            string error=store.Error;Check(store.Save(store.Snapshot,out changed)&&!changed&&store.Error==error,"no-op preserves earlier failure");
            faulty.Fail=false;
            using(var lease=new FileStream(file,FileMode.Open,FileAccess.Read,FileShare.None))Check(!store.Save(failed,out changed)&&store.Snapshot.displayName=="Sofia","unreadable original refuses commit");
            Check(store.Save(failed,out changed)&&changed&&store.Error==""&&store.Snapshot.revision==2,"only successful retry publishes/clears failure");
            Check(Equal(first,File.ReadAllBytes(file+".bak")),"atomic replacement retains exact previous bytes");
            int actualChanges=0;var repeated=store.Snapshot;repeated.avatarId="stool";
            Parallel.For(0,100,_=>{bool c;Check(store.Save(repeated,out c),"parallel same profile save valid");if(c)System.Threading.Interlocked.Increment(ref actualChanges);});
            Check(actualChanges==1&&store.Snapshot.revision==3,"repeated save commits once");
            var current=File.ReadAllBytes(file);var currentBak=File.ReadAllBytes(file+".bak");File.WriteAllText(file,"{\"version\":2,\"revision\":1}");var future=File.ReadAllBytes(file);
            var foreign=store.Snapshot;foreign.displayName="Else";
            Check(!store.Save(foreign,out changed)&&store.Blocked&&Equal(future,File.ReadAllBytes(file))&&Equal(currentBak,File.ReadAllBytes(file+".bak")),"external future data preserved/refuses overwrite");
            Check(!store.Reload()&&store.Blocked,"unsupported version remains protected on retry");
            File.WriteAllBytes(file,current);Check(store.Reload()&&!store.Blocked&&store.Snapshot.revision==3,"retry after original restored");
            foreach(string malformed in new[]{"{}","{bad","{\"version\":1,\"revision\":1,\"displayName\":\"Player\",\"avatarId\":\"heroine\",\"animationMode\":\"full\"}","{\"version\":1,\"revision\":1,\"displayName\":\"Player\",\"avatarId\":\"fake\",\"animationMode\":\"full\",\"notificationMode\":\"on\"}"})
            {
                string dir=Path.Combine(root,"invalid-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);string path=Path.Combine(dir,"local-player-v1.json");File.WriteAllText(path,malformed);byte[] exact=File.ReadAllBytes(path);var blocked=new LocalPlayerStore(dir,codec);
                Check(blocked.Blocked&&!blocked.Save(new LocalPlayer(),out changed)&&Equal(exact,File.ReadAllBytes(path)),"malformed/absent mandatory fields cannot be silently defaulted");
            }
            string legacyDir=Path.Combine(root,"legacy");Directory.CreateDirectory(legacyDir);string legacyPath=Path.Combine(legacyDir,"presentation-v1.json");File.WriteAllText(legacyPath,"{\"reducedMotion\":true}");byte[] old=File.ReadAllBytes(legacyPath);var imported=new LocalPlayerStore(legacyDir,codec);
            Check(imported.Snapshot.Calm&&!File.Exists(Path.Combine(legacyDir,"local-player-v1.json")),"legacy calm imported without writes");
            Check(imported.Save(imported.Snapshot,out changed)&&changed&&Equal(old,File.ReadAllBytes(legacyPath)),"explicit first save retains untouched legacy file");
            string damaged=Path.Combine(root,"damaged-legacy");Directory.CreateDirectory(damaged);File.WriteAllText(Path.Combine(damaged,"presentation-v1.json"),"{}");Check(new LocalPlayerStore(damaged,codec).Blocked,"malformed legacy settings are surfaced/protected");
            Check(Directory.GetFiles(root,"*.tmp",SearchOption.AllDirectories).Length==0,"owned temporary files cleaned after failures");

            var events=new SessionNotifications();var state=new PlayerState{spins=15,recoveries=2};state.purchased[0]=true;events.Reset(state,5);int generation=events.Generation;
            Check(!events.Record(generation,SessionEvent.Spin,15,"old spin")&&!events.Record(generation,SessionEvent.Purchase,1,"old buy"),"reload baselines prevent fake history");
            for(int i=16;i<66;i++)Check(events.Record(generation,SessionEvent.Spin,i,"Spin "+i),"committed session entry");
            Check(events.History.Length==20&&events.PendingCount==4,"bounded history and queue");
            Check(!events.Record(generation,SessionEvent.Spin,65,"duplicate"),"callback duplicate ignored");
            events.SetOptional(false);Check(events.PendingCount==0,"muting clears stale optional queue");
            Check(events.Record(generation,SessionEvent.Profile,6,"Profile saved")&&events.PendingCount==0,"muted optional still has honest history");
            events.SetCritical(1,"Profile failed");Check(events.Critical=="Profile failed","critical error cannot be muted");var count=events.History.Length;events.SetCritical(1,"Profile failed");Check(events.History.Length==count,"repeated critical text deduplicated");
            events.SetCritical(0,"Game failed");Check(events.Critical=="Game failed","game error priority over profile error");events.SetCritical(0,"");Check(events.Critical=="Profile failed","clearing game error retains profile error");
            events.Reset(new PlayerState(),6);Check(events.History.Length==0&&events.PendingCount==0&&events.Critical=="Profile failed","confirmed reset clears history but retains unresolved local error");
            Check(!events.Record(generation,SessionEvent.Spin,66,"stale callback"),"old generation cannot leak after reset");
            events.RebaseLocal(1);Check(events.Record(events.Generation,SessionEvent.Profile,2,"Saved after local reload"),"local reload rebases revision dedup without inventing history");
            events.SetOptional(true);Check(events.Record(events.Generation,SessionEvent.Reset,1,"Fresh start"),"actual reset can be logged once");Check(events.Take()=="Fresh start"&&events.Take()==null,"queue drains once");
            return "PASS "+assertions+" local profile/session notification assertions";
        }
    }
}
