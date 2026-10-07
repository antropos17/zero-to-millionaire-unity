using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ZeroToMillionaire
{
    public sealed class Level1StoreV7:ILevel1StoreV7
    {
        public const string SnapshotError="Could not protect the old save before schema7. No changes saved.";
        public const string ChangedError="Save changed on disk. Reload before saving.";
        readonly object gate=new object();readonly string path;readonly ILevel1CodecV7 codec;readonly IStateCodec legacyCodec;
        byte[] primary,backup;bool loaded;
        public bool RecoveryRequired{get;private set;}
        public string PreMigrationSnapshotPath{get;private set;}
        public Level1StoreV7(string path,ILevel1CodecV7 codec,IStateCodec legacyCodec){this.path=path;this.codec=codec;this.legacyCodec=legacyCodec;}
        static byte[] Read(string file)
        {
            try{var attributes=File.GetAttributes(file);if((attributes&FileAttributes.Directory)!=0)throw new IOException();}
            catch(FileNotFoundException){return null;}catch(DirectoryNotFoundException){return null;}
            using(var stream=new FileStream(file,FileMode.Open,FileAccess.Read,FileShare.Read))
            {
                if(stream.Length>65536)throw new IOException("Save exceeds size limit");var bytes=new byte[stream.Length];int at=0,n;
                while(at<bytes.Length&&(n=stream.Read(bytes,at,bytes.Length-at))>0)at+=n;
                if(at!=bytes.Length||stream.ReadByte()!=-1)throw new IOException("Save changed during read");return bytes;
            }
        }
        static string Text(byte[] bytes){var s=new UTF8Encoding(false,true).GetString(bytes);return s.Length>0&&s[0]=='\ufeff'?s.Substring(1):s;}
        static bool Equal(byte[] a,byte[] b){if(a==null||b==null)return a==b;if(a.Length!=b.Length)return false;for(int i=0;i<a.Length;i++)if(a[i]!=b[i])return false;return true;}
        Level1StateV7 Decode(byte[] bytes,out int version,out bool unsupported)
        {
            version=0;unsupported=false;if(bytes==null)return null;
            string text=Text(bytes);version=Schema7Shape.Version(text);unsupported=version>7||version<1;
            if(unsupported)return null;
            if(version==7)return codec.Decode(text);
            var old=legacyCodec.Decode(text);string error;if(!GameModel.IsPersistedValid(old,out error))return null;
            if(old.version!=version)throw new InvalidOperationException("Legacy codec/header mismatch");
            if(version==6)
            {
                var expected=Economy.QuestRecords(old);
                foreach(var q in old.quests){int index=Array.FindIndex(expected,x=>x.id==q.id);if(index<0||q.status!=expected[index].status)return null;}
            }
            return Level1StateV7.MigrateLegacy(old);
        }
        public Level1StateV7 Load(out string notice)
        {
            lock(gate)
            {
                loaded=true;RecoveryRequired=false;notice="";
                try
                {
                    primary=Read(path);backup=Read(path+".bak");int pv,bv;bool pu,bu;
                    var p=Decode(primary,out pv,out pu);var b=Decode(backup,out bv,out bu);
                    if(pu||bu||(primary!=null&&pv==7&&p==null)||(backup!=null&&bv==7&&b==null))
                    {RecoveryRequired=true;notice="Unsupported or malformed save preserved. Writes paused.";return p??b??new Level1StateV7();}
                    if(p!=null||b!=null){if(p==null)notice="Loaded backup; exact originals will be protected before replacement.";else if(pv<7)notice="Schema7 migration pending; original files unchanged.";return (p??b).Clone();}
                    if(primary!=null||backup!=null){RecoveryRequired=true;notice="Save unavailable; original files kept. Writes paused.";}
                    return new Level1StateV7();
                }
                catch(Exception){RecoveryRequired=true;notice="Could not verify local save. Original files kept. Writes paused.";return new Level1StateV7();}
            }
        }
        static void Preserve(string file,byte[] bytes)
        {
            var old=Read(file);if(old!=null){if(bytes==null||!Equal(old,bytes))throw new IOException("Snapshot differs");return;}if(bytes==null)return;
            using(var stream=new FileStream(file,FileMode.CreateNew,FileAccess.Write,FileShare.None)){stream.Write(bytes,0,bytes.Length);stream.Flush(true);}
            if(!Equal(Read(file),bytes))throw new IOException("Snapshot verification failed");
        }
        bool ProtectOriginals(out string error)
        {
            error="";
            try
            {
                int pv,bv;bool pu,bu;var p=Decode(primary,out pv,out pu);var b=Decode(backup,out bv,out bu);
                if(pu||bu){error=ChangedError;return false;}
                int version=pv>0&&pv<7?pv:bv>0&&bv<7?bv:0;
                bool repair=primary!=null&&p==null&&b!=null;
                if(version==0&&!repair)return true;
                if(version==0)version=7;
                string hash;
                using(var memory=new MemoryStream())
                {
                    using(var writer=new BinaryWriter(memory,Encoding.UTF8,true)){writer.Write(version);foreach(var bytes in new[]{primary,backup}){writer.Write(bytes==null?-1:bytes.Length);if(bytes!=null)writer.Write(bytes);}}
                    using(var sha=SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(memory.ToArray())).Replace("-","").ToLowerInvariant();
                }
                string dir=Path.Combine(path+".pre-schema7","v"+version+"-"+hash);Directory.CreateDirectory(dir);
                Preserve(Path.Combine(dir,"primary.json"),primary);Preserve(Path.Combine(dir,"backup.json"),backup);
                string manifest="snapshot-v1\nschema="+version+"\ntarget=7\nprimary="+(primary==null?"absent":"present")+"\nbackup="+(backup==null?"absent":"present")+"\nfingerprint="+hash+"\n";
                Preserve(Path.Combine(dir,"snapshot.txt"),Encoding.UTF8.GetBytes(manifest));PreMigrationSnapshotPath=dir;return true;
            }
            catch(Exception){error=SnapshotError;return false;}
        }
        public bool Save(Level1StateV7 state,out string error)
        {
            lock(gate)
            {
                error="";if(!loaded){error="Load/verify originals before saving.";return false;}if(RecoveryRequired){error="Save requires recovery. Original files kept.";return false;}
                if(!Level1StateV7.Valid(state,out error))return false;
                string temp=path+"."+Guid.NewGuid().ToString("N")+".schema7.tmp";bool owned=false;
                try
                {
                    if(!Equal(primary,Read(path))||!Equal(backup,Read(path+".bak"))){error=ChangedError;return false;}
                    if(!ProtectOriginals(out error))return false;
                    var bytes=new UTF8Encoding(false,true).GetBytes(codec.Encode(state));
                    var decoded=codec.Decode(Text(bytes));string validation;
                    if(bytes.Length>65536||!Level1StateV7.Valid(decoded,out validation)||!EqualCanonical(state,decoded))throw new IOException("Schema7 codec roundtrip failed");
                    Directory.CreateDirectory(Path.GetDirectoryName(path));using(var stream=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None)){owned=true;stream.Write(bytes,0,bytes.Length);stream.Flush(true);}
                    if(!Equal(primary,Read(path))||!Equal(backup,Read(path+".bak"))){error=ChangedError;return false;}
                    if(primary!=null)File.Replace(temp,path,path+".bak");else File.Move(temp,path);
                    if(primary!=null)backup=primary;primary=bytes;return true;
                }
                catch(Exception e)when(e is IOException||e is UnauthorizedAccessException||e is ArgumentException||e is NotSupportedException||e is InvalidOperationException){error="Schema7 save failed. No changes applied.";return false;}
                finally{if(owned)try{File.Delete(temp);}catch(IOException){}catch(UnauthorizedAccessException){}}
            }
        }
        public static bool EqualCanonical(Level1StateV7 a,Level1StateV7 b)
        {
            if(a==null||b==null||a.version!=b.version||a.cash!=b.cash||a.stakeMultiplier!=b.stakeMultiplier||a.freeSpins!=b.freeSpins||a.spins!=b.spins||a.recoveries!=b.recoveries||a.heroineU!=b.heroineU||a.heroineV!=b.heroineV||a.decorStarted!=b.decorStarted)return false;
            for(int i=0;i<8;i++)if(a.fragments[i]!=b.fragments[i]||a.purchased[i]!=b.purchased[i])return false;for(int i=0;i<4;i++)if(a.decorFragments[i]!=b.decorFragments[i]||a.decorPurchased[i]!=b.decorPurchased[i])return false;for(int i=0;i<15;i++)if(a.lastGrid[i]!=b.lastGrid[i])return false;
            if(a.tutorial.version!=b.tutorial.version||a.tutorial.step!=b.tutorial.step||a.tutorial.completed!=b.tutorial.completed||a.tutorial.skipped!=b.tutorial.skipped||a.tutorial.storySeenMask!=b.tutorial.storySeenMask)return false;
            foreach(var q in a.quests){var r=Array.Find(b.quests,x=>x.id==q.id);if(r==null||q.status!=r.status||q.cashPrice!=r.cashPrice||q.fragmentGoal!=r.fragmentGoal)return false;}return true;
        }
    }
}
