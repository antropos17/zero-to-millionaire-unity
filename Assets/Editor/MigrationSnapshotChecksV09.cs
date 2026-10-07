using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ZeroToMillionaire
{
    public static class MigrationSnapshotChecksV09
    {
        sealed class FaultCodec : IStateCodec {
            readonly IStateCodec inner; public int FailDecode;
            public FaultCodec(IStateCodec value) { inner = value; }
            public string Encode(PlayerState state) { return inner.Encode(state); }
            public PlayerState Decode(string text) {
                if (FailDecode == 1) throw new InvalidOperationException("Private classification fault");
                if (FailDecode == 2) throw new IOException("Private classification IO fault");
                return inner.Decode(text);
            }
        }
        static bool shareProbeReplacement;
        static int checks;
        static void Check(bool ok, string why) { checks++; if (!ok) throw new Exception("MIGRATION SNAPSHOT: " + why); }
        static PlayerState Old(int version, int cash = 123) {
            var s = new PlayerState { version = version, stakeMultiplier = 0, cash = cash };
            s.tutorial.completed = true; return s;
        }
        static byte[] Bytes(PlayerState s, IStateCodec codec) => Encoding.UTF8.GetBytes(codec.Encode(s));
        static string PathFor(string root, string name) { string d = Path.Combine(root, name); Directory.CreateDirectory(d); return Path.Combine(d, "level1-v1.json"); }
        static void Same(byte[] wanted, string actual, string why) { Check(File.ReadAllBytes(actual).SequenceEqual(wanted), why); }
        public static string Run(IStateCodec codec, string directory) {
            checks = 0; string root = Path.Combine(directory, "migration-snapshots");
            if (Directory.Exists(root)) throw new Exception("Fresh snapshot audit root required");
            for (int version = 1; version <= 5; version++) {
                string p = PathFor(root, "schema" + version); var old = Old(version);
                byte[] primary = version == 5 ? Encoding.Unicode.GetPreamble().Concat(Encoding.Unicode.GetBytes(codec.Encode(old))).ToArray() : Bytes(old, codec);
                byte[] backup = Bytes(Old(version, 37), codec);
                File.WriteAllBytes(p, primary); File.WriteAllBytes(p + ".bak", backup);
                var store = new SaveStore(p, codec); var state = store.Load(out _);
                Check(state.version == 6 && state.cash == 123 && state.stakeMultiplier == 1, "schema" + version + " load preserves progress");
                Check(store.Save(state, out var error), "first migration save " + error);
                string snap = store.PreMigrationSnapshotPath;
                Check(snap != null && Directory.Exists(snap) && snap.Contains("pre-schema6"), "durable versioned snapshot created");
                Same(primary, Path.Combine(snap, "primary.json"), "exact original primary including encoding/BOM");
                Same(backup, Path.Combine(snap, "backup.json"), "exact original backup");
                var files = Directory.GetFiles(snap); var times = files.Select(File.GetLastWriteTimeUtc).ToArray();
                for (int repeat = 0; repeat < 4; repeat++) { state.cash++; Check(store.Save(state, out error), "repeat save"); }
                Check(Directory.GetDirectories(p + ".pre-schema6").Length == 1, "ordinary backup rotation never adds/replaces snapshot");
                Check(files.Select(File.GetLastWriteTimeUtc).SequenceEqual(times), "snapshot files never rewritten");
                Same(primary, Path.Combine(snap, "primary.json"), "primary remains available after repeated saves");
                Same(backup, Path.Combine(snap, "backup.json"), "backup remains available after repeated saves");
                var reopened = new SaveStore(p, codec); Check(reopened.Save(reopened.Load(out _), out error), "fresh store schema6 save");
                Same(primary, Path.Combine(snap, "primary.json"), "snapshot survives store relaunch");
            }
            {
                string p = PathFor(root, "blocked-snapshot"); byte[] primary = Bytes(Old(5), codec), backup = Bytes(Old(5, 17), codec);
                File.WriteAllBytes(p, primary); File.WriteAllBytes(p + ".bak", backup); File.WriteAllText(p + ".pre-schema6", "private blocker");
                var session = new GameSession(new SaveStore(p, codec), new SeededRandom(7)); var before = codec.Encode(session.Model.Snapshot);
                Check(!session.TrySelectStake(3) && session.Error == SaveStore.MigrationSnapshotError, "failed required backup is visible");
                Check(codec.Encode(session.Model.Snapshot) == before, "failed required backup cannot publish changed resources/preference");
                Same(primary, p, "backup failure preserves live primary"); Same(backup, p + ".bak", "backup failure preserves live backup");
                File.Move(p + ".pre-schema6", p + ".private-blocker"); Check(session.TrySelectStake(3), "retry succeeds after private blocker removed");
            }
            {
                string p = PathFor(root, "failed-live-write"); byte[] primary = Bytes(Old(5), codec), backup = Bytes(Old(5, 19), codec);
                File.WriteAllBytes(p, primary); File.WriteAllBytes(p + ".bak", backup); var store = new SaveStore(p, codec); var state = store.Load(out _);
                using (var held = new FileStream(p + ".tmp", FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                    Check(!store.Save(state, out var error) && error.StartsWith("Save failed"), "live write fails after required snapshot");
                    Same(primary, p, "failed live commit preserves original primary"); Same(backup, p + ".bak", "failed live commit preserves original backup");
                }
                string snap = store.PreMigrationSnapshotPath; Check(snap != null, "snapshot survives failed live write");
                var firstTimes = Directory.GetFiles(snap).Select(File.GetLastWriteTimeUtc).ToArray();
                Check(store.Save(state, out _), "retry reuses existing valid snapshot");
                Check(Directory.GetFiles(snap).Select(File.GetLastWriteTimeUtc).SequenceEqual(firstTimes), "retry never overwrites valid snapshot");
                Same(primary, Path.Combine(snap, "primary.json"), "failed write original still recoverable");
            }
            {
                string p = PathFor(root, "snapshot-write-failure"); byte[] primary = Bytes(Old(5), codec), backup = Bytes(Old(5, 22), codec);
                File.WriteAllBytes(p, primary); File.WriteAllBytes(p + ".bak", backup); var store = new SaveStore(p, codec); var state = store.Load(out _);
                using (var held = new FileStream(p + ".tmp", FileMode.CreateNew, FileAccess.Write, FileShare.None)) Check(!store.Save(state, out _), "prepare failed live write fixture");
                string snap = store.PreMigrationSnapshotPath, backupCopy = Path.Combine(snap, "backup.json");
                File.Delete(backupCopy); Directory.CreateDirectory(backupCopy);
                Check(!store.Save(state, out var error) && error == SaveStore.MigrationSnapshotError, "incomplete/unwritable required snapshot blocks live commit");
                Same(primary, p, "snapshot write failure preserves original primary"); Same(backup, p + ".bak", "snapshot write failure preserves original backup");
                Same(primary, Path.Combine(snap, "primary.json"), "valid existing snapshot component not overwritten");
                Directory.Delete(backupCopy); Check(store.Save(state, out _), "partial snapshot can finish without replacing valid component");
                Same(backup, backupCopy, "required backup exact after retry");
            }
            foreach (bool missingPrimary in new[] { false, true }) {
                string p = PathFor(root, missingPrimary ? "backup-only" : "corrupt-primary"); byte[] bad = Encoding.UTF8.GetBytes("broken original primary\n"), backup = Bytes(Old(5, 81), codec);
                if (!missingPrimary) File.WriteAllBytes(p, bad); File.WriteAllBytes(p + ".bak", backup);
                var store = new SaveStore(p, codec); var state = store.Load(out var notice);
                Check(state.cash == 81 && notice.Contains("Recovered"), "migration from legacy backup preserves resources");
                Check(store.Save(state, out _), "backup-source migration commits after protecting both originals"); string snap = store.PreMigrationSnapshotPath;
                Check(snap != null, "backup-source snapshot exists"); Same(backup, Path.Combine(snap, "backup.json"), "legacy recovery source byte-identical");
                if (!missingPrimary) Same(bad, Path.Combine(snap, "primary.json"), "corrupt primary retained for rollback");
                else Check(!File.Exists(Path.Combine(snap, "primary.json")) && File.ReadAllText(Path.Combine(snap, "snapshot.txt")).Contains("primary=absent"), "original absence is recorded");
                Check(store.Save(state, out _), "backup-source repeat save"); Same(backup, Path.Combine(snap, "backup.json"), "backup-source snapshot stays nonrotating");
            }
            {
                string p = PathFor(root, "another-legacy-episode"); byte[] first = Bytes(Old(5, 123), codec); File.WriteAllBytes(p, first);
                var store = new SaveStore(p, codec); Check(store.Save(store.Load(out _), out _), "first historical snapshot"); string original = store.PreMigrationSnapshotPath;
                File.WriteAllBytes(p, Bytes(Old(5, 321), codec)); var later = new SaveStore(p, codec); Check(later.Save(later.Load(out _), out _), "different legacy input gets its own snapshot");
                Check(original != later.PreMigrationSnapshotPath && Directory.GetDirectories(p + ".pre-schema6").Length == 2, "nonrotating content versions retain both episodes");
                Same(first, Path.Combine(original, "primary.json"), "first valid snapshot never overwritten by later migration");
            }
            foreach(int fault in new[]{1,2}) {
                string p=PathFor(root,"decode-fault-"+fault);byte[] original=Bytes(Old(5),codec),backup=Bytes(Old(5,41),codec);
                File.WriteAllBytes(p,original);File.WriteAllBytes(p+".bak",backup);
                var failing=new FaultCodec(codec);var store=new SaveStore(p,failing);var state=store.Load(out _);
                Check(state.version==6,"legacy load succeeded before classification fault");failing.FailDecode=fault;
                Check(!store.Save(state,out var error)&&error==SaveStore.MigrationSnapshotError,"unexpected decode fault cannot skip required snapshot");
                Same(original,p,"classification fault preserves live legacy primary");Same(backup,p+".bak","classification fault preserves live backup");
                Check(!File.Exists(p+".tmp"),"classification fault refuses before temp write");failing.FailDecode=0;
                Check(store.Save(state,out _),"pending migration requirement survives failed decode and retry");
                Same(original,Path.Combine(store.PreMigrationSnapshotPath,"primary.json"),"retry still snapshots original legacy source");
            }
            {
                string p=PathFor(root,"read-denied-delete-shared");byte[] original=Bytes(Old(5),codec);File.WriteAllBytes(p,original);
                var store=new SaveStore(p,codec);var state=store.Load(out _);
                using(var held=new FileStream(p,FileMode.Open,FileAccess.Write,FileShare.Write|FileShare.Delete)) {
                    bool readDenied=false;try{File.ReadAllBytes(p);}catch(IOException){readDenied=true;}
                    Check(readDenied,"private primary read actually denied while delete-share configured");
                    Check(!store.Save(state,out var error)&&error==SaveStore.MigrationSnapshotError,"read-denied original blocks before write even with pending legacy requirement");
                    Check(!File.Exists(p+".tmp")&&!Directory.Exists(p+".pre-schema6"),"read-denied failure cannot reach temp/snapshot write");
                }
                Same(original,p,"read-denied failure preserves original primary after handle release");
                Check(store.Save(state,out _),"read-denied retry retains required snapshot");
                Same(original,Path.Combine(store.PreMigrationSnapshotPath,"primary.json"),"read-denied retry protects exact source");
                string probe=Path.Combine(Path.GetDirectoryName(p),"separate-replace-probe"),temp=probe+".tmp";
                File.WriteAllText(probe,"private probe original");File.WriteAllText(temp,"private probe replacement");
                shareProbeReplacement=false;
                using(var held=new FileStream(probe,FileMode.Open,FileAccess.Write,FileShare.Write|FileShare.Delete)) {
                    try{File.Replace(temp,probe,probe+".bak");shareProbeReplacement=true;}catch(IOException){}catch(UnauthorizedAccessException){}
                }
                // Record the observed OS primitive; do not assume replacement succeeds.
                Check(File.ReadAllText(probe)==(shareProbeReplacement?"private probe replacement":"private probe original"),"separate delete-shared replace probe outcome recorded honestly");
            }
            foreach(int foreignVersion in new[]{6,7}) {
                string p=PathFor(root,"external-version-"+foreignVersion);byte[] original=Bytes(Old(5),codec);File.WriteAllBytes(p,original);
                var store=new SaveStore(p,codec);var migrated=store.Load(out _);
                var foreign=new PlayerState{version=foreignVersion,cash=321,stakeMultiplier=3};byte[] foreignBytes=Bytes(foreign,codec);File.WriteAllBytes(p,foreignBytes);
                Check(!store.Save(migrated,out var error)&&error==SaveStore.SaveChangedNotice,"external current/future primary cannot erase pending migration requirement");
                Same(foreignBytes,p,"external changed primary stays untouched");Check(!Directory.Exists(p+".pre-schema6"),"stale loaded state cannot snapshot/rewrite another generation");
                File.WriteAllBytes(p,original);Check(store.Save(migrated,out _),"restored exact originals permit original pending migration");
                Same(original,Path.Combine(store.PreMigrationSnapshotPath,"primary.json"),"original pending source is protected after external-change refusal");
            }
            {
                string p=PathFor(root,"deleted-originals");File.WriteAllBytes(p,Bytes(Old(5),codec));var store=new SaveStore(p,codec);var state=store.Load(out _);File.Delete(p);
                Check(!store.Save(state,out var error)&&error==SaveStore.SaveChangedNotice,"removed originals are not mistaken for a fresh migration-free profile");
                Check(!File.Exists(p),"deleted originals remain absent after refused stale write");
            }
            foreach(bool futureBackup in new[]{false,true}) {
                string p=PathFor(root,futureBackup?"unsupported-backup":"unsupported-primary");
                var future=new PlayerState{version=7,cash=246,stakeMultiplier=2};byte[] current=Bytes(new PlayerState(),codec),foreign=Bytes(future,codec);
                File.WriteAllBytes(p,futureBackup?current:foreign);if(futureBackup)File.WriteAllBytes(p+".bak",foreign);
                var store=new SaveStore(p,codec);Check(!store.Save(new PlayerState(),out var error)&&error==SaveStore.SaveChangedNotice,"unsupported present originals block direct saves before any copy/replace");
                Same(futureBackup?current:foreign,p,"unsupported guard preserves primary");if(futureBackup)Same(foreign,p+".bak","unsupported guard preserves future backup");
            }
            {
                string p=PathFor(root,"confirmed-future-reset");var future=new PlayerState{version=7,cash=400,stakeMultiplier=3};byte[] primary=Bytes(future,codec);future.cash=200;byte[] backup=Bytes(future,codec);
                File.WriteAllBytes(p,primary);File.WriteAllBytes(p+".bak",backup);var store=new SaveStore(p,codec);store.Load(out _);
                Check(store.RecoveryRequired,"future primary enters explicit protected recovery");
                Check(store.TryStartFresh(new PlayerState(),out var error),"confirmed recovery reset still commits after copying future originals: "+error);
                Check(store.Save(new PlayerState(),out error),"confirmed reset may rotate only its already-protected future backup: "+error);
                Check(store.Load(out _).cash==0&&File.ReadAllText(p+".bak")==codec.Encode(new PlayerState()),"reset primary and backup stay fresh");
                var copies=Directory.GetFiles(Path.GetDirectoryName(p),"*.recovery-*");
                Check(copies.Length==2&&copies.Any(x=>File.ReadAllBytes(x).SequenceEqual(primary))&&copies.Any(x=>File.ReadAllBytes(x).SequenceEqual(backup)),"confirmed reset retains both exact original future streams");
            }
            return "PASS " + checks + " durable migration snapshot checks; schema1-5, exact bytes/BOM, repeat/relaunch, backup-source migration, required-copy/commit failure and nonrotating history; fail-closed decode/read/external/future checks; separate delete-shared replace primitive observed="+shareProbeReplacement;
        }
    }
}
