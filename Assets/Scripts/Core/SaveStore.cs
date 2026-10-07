using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ZeroToMillionaire
{
    public interface IStateCodec { string Encode(PlayerState state); PlayerState Decode(string text); }
    public interface IStateStore { bool Save(PlayerState state, out string error); PlayerState Load(out string notice); }
    public interface IRecoverableStateStore : IStateStore {
        bool RecoveryRequired { get; }
        bool TryStartFresh(PlayerState state, out string error);
    }
    public sealed class SaveStore : IRecoverableStateStore
    {
        public const string RecoveryNotice = "Local save needs recovery; original files preserved.";
        public const string MigrationSnapshotError = "Could not protect the old save. No changes were saved.";
        public const string SaveChangedNotice = "Save changed on disk. Close and reopen the game.";
        readonly string path; readonly IStateCodec codec;
        bool migrationRequired, migrationCaptureFailed, migrationPrimarySource;
        int migrationSourceVersion;
        byte[] migrationPrimary, migrationBackup, recoveryProtectedBackup;
        public bool RecoveryRequired { get; private set; }
        public string PreMigrationSnapshotPath { get; private set; }
        public SaveStore(string filePath, IStateCodec stateCodec) { path = filePath; codec = stateCodec; }
        public bool Save(PlayerState state, out string error) { return Save(state, out error, false); }
        bool Save(PlayerState state, out string error, bool recoveryOriginalsProtected) {
            error = "";
            if (RecoveryRequired) { error = RecoveryNotice; return false; }
            string reason;
            if (!GameModel.IsValid(state, out reason)) { error = reason; return false; }
            string temp = path + ".tmp";
            try {
                if (!recoveryOriginalsProtected && !PreserveBeforeMigration(out error)) return false;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream)) { writer.Write(codec.Encode(state)); writer.Flush(); stream.Flush(true); }
                if (File.Exists(path)) File.Replace(temp, path, path + ".bak"); else File.Move(temp, path);
                ClearMigrationRequirement(); recoveryProtectedBackup = null;
                return true;
            } catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is NotSupportedException) {
                error = "Save failed: " + e.GetType().Name; return false;
            } finally { try { if (File.Exists(temp)) File.Delete(temp); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
        }

        // Capture the exact on-disk byte streams before replacing either live file.
        // Content-addressed schema directories are nonrotating and never overwritten.
        // This cannot prevent a previously loaded old executable from writing later.
        bool PreserveBeforeMigration(out string error) {
            error = "";
            try {
                // Read originals strictly. A missing file is distinct from a read/access
                // failure; inability to verify a present file never permits a write.
                byte[] primary = ReadOriginal(path), backup = ReadOriginal(path + ".bak");
                if (migrationRequired) {
                    if (migrationCaptureFailed) { error = MigrationSnapshotError; return false; }
                    if (!Equal(migrationPrimary, primary) || !Equal(migrationBackup, backup)) { error = SaveChangedNotice; return false; }
                }
                PlayerState old; bool primaryMigrated, backupMigrated, primaryUnsupported, backupUnsupported;
                int primaryVersion, backupVersion;
                bool primaryValid = DecodeOriginal(primary, out old, out primaryMigrated, out primaryUnsupported, out primaryVersion);
                bool backupValid = DecodeOriginal(backup, out old, out backupMigrated, out backupUnsupported, out backupVersion);
                // A confirmed recovery reset already copied its exact old streams.
                // Permit only rotation of that protected old backup behind a valid
                // current primary; never permit an unknown future primary/backup.
                bool protectedResetBackup = primaryValid && primaryVersion == Economy.SaveVersion && Equal(backup, recoveryProtectedBackup);
                if (primaryUnsupported || (backupUnsupported && !protectedResetBackup)) { error = SaveChangedNotice; return false; }
                bool primarySource = primaryValid;
                if (!primaryValid && !backupValid) {
                    if (primary == null && backup == null && !migrationRequired) return true;
                    error = migrationRequired ? MigrationSnapshotError : RecoveryNotice; return false;
                }
                bool migrated = primarySource ? primaryMigrated : backupMigrated;
                int sourceVersion = primarySource ? primaryVersion : backupVersion;
                if (migrationRequired && (!migrated || primarySource != migrationPrimarySource || sourceVersion != migrationSourceVersion)) {
                    error = MigrationSnapshotError; return false;
                }
                if (!migrated) return true;
                string fingerprint;
                using (var buffer = new MemoryStream()) {
                    using (var writer = new BinaryWriter(buffer, Encoding.UTF8, true)) {
                        writer.Write(sourceVersion);
                        foreach (var bytes in new[] { primary, backup }) { writer.Write(bytes == null ? -1 : bytes.Length); if (bytes != null) writer.Write(bytes); }
                    }
                    using (var hash = SHA256.Create()) fingerprint = BitConverter.ToString(hash.ComputeHash(buffer.ToArray())).Replace("-", "").ToLowerInvariant();
                }
                string directory = Path.Combine(path + ".pre-schema" + Economy.SaveVersion, "v" + sourceVersion + "-" + fingerprint);
                Directory.CreateDirectory(directory);
                PreserveFile(Path.Combine(directory, "primary.json"), primary);
                PreserveFile(Path.Combine(directory, "backup.json"), backup);
                string manifest = "snapshot-v1\nschema=" + sourceVersion + "\nsource=" + (primarySource ? "primary" : "backup") +
                    "\nprimary=" + (primary == null ? "absent" : "present") + "\nbackup=" + (backup == null ? "absent" : "present") + "\nfingerprint=" + fingerprint + "\n";
                PreserveFile(Path.Combine(directory, "snapshot.txt"), Encoding.UTF8.GetBytes(manifest));
                PreMigrationSnapshotPath = directory;
                return true;
            } catch (Exception) {
                // Codec/classification faults are failures, never proof that no copy is needed.
                error = MigrationSnapshotError; return false;
            }
        }
        void ClearMigrationRequirement() {
            migrationRequired = migrationCaptureFailed = false;
            migrationPrimary = migrationBackup = null; migrationSourceVersion = 0;
        }
        void RememberMigration(bool primarySource, int version, byte[] sourceBytes) {
            migrationRequired = true; migrationPrimarySource = primarySource; migrationSourceVersion = version;
            try {
                migrationPrimary = ReadOriginal(path); migrationBackup = ReadOriginal(path + ".bak");
                if (!Equal(sourceBytes, primarySource ? migrationPrimary : migrationBackup)) throw new IOException("Migration source changed during load");
            } catch (Exception) { migrationCaptureFailed = true; }
        }
        static byte[] ReadOriginal(string source) {
            if (!Present(source)) return null;
            // Saves are bounded; refuse to migrate if an original cannot be safely copied.
            if (new FileInfo(source).Length > 64 * 1024) throw new IOException("Original save exceeds size limit");
            var bytes = File.ReadAllBytes(source);
            if (bytes.Length > 64 * 1024) throw new IOException("Original save changed size");
            return bytes;
        }
        static bool Equal(byte[] a, byte[] b) {
            if (a == null || b == null) return a == b;
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
            return true;
        }
        static void PreserveFile(string destination, byte[] bytes) {
            if (Present(destination)) {
                if (bytes == null || !Equal(File.ReadAllBytes(destination), bytes)) throw new IOException("Existing snapshot differs");
                return;
            }
            if (bytes == null) return;
            using (var stream = new FileStream(destination, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
                stream.Write(bytes, 0, bytes.Length); stream.Flush(true);
            }
            if (!Equal(File.ReadAllBytes(destination), bytes)) throw new IOException("Snapshot verification failed");
        }
        // File.Exists hides access errors. Only a proved absent primary AND backup is a fresh profile.
        static bool Present(string source) {
            try { File.GetAttributes(source); return true; }
            catch (FileNotFoundException) { return false; }
            catch (DirectoryNotFoundException) { return false; }
        }
        public PlayerState Load(out string notice) {
            notice = ""; RecoveryRequired = false; ClearMigrationRequirement();
            try { if (!Present(path) && !Present(path + ".bak")) return new PlayerState(); }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is NotSupportedException) { return Block(out notice); }
            PlayerState state; bool migrated, unsupported; byte[] bytes; int version;
            if (TryRead(path, out state, out migrated, out unsupported, out bytes, out version)) {
                if (migrated) { RememberMigration(true, version, bytes); notice = migrationCaptureFailed ? MigrationSnapshotError : "Save upgraded: all progress kept."; }
                return state;
            }
            // An older backup must never silently downgrade a future primary.
            if (unsupported) return Block(out notice);
            if (TryRead(path + ".bak", out state, out migrated, out unsupported, out bytes, out version)) {
                if (migrated) RememberMigration(false, version, bytes);
                notice = migrationCaptureFailed ? MigrationSnapshotError : "Recovered the previous local save."; return state;
            }
            return Block(out notice);
        }
        PlayerState Block(out string notice) { RecoveryRequired = true; notice = RecoveryNotice; return new PlayerState(); }
        bool TryRead(string source, out PlayerState state, out bool migrated, out bool unsupported, out byte[] bytes, out int version) {
            state = null; migrated = unsupported = false; bytes = null; version = 0;
            try { bytes = ReadOriginal(source); return DecodeOriginal(bytes, out state, out migrated, out unsupported, out version); }
            catch (Exception) { state = null; return false; }
        }
        bool DecodeOriginal(byte[] bytes, out PlayerState state, out bool migrated, out bool unsupported, out int version) {
            state = null; migrated = unsupported = false; version = 0;
            if (bytes == null) return false;
            try {
                using (var stream = new MemoryStream(bytes))
                using (var reader = new StreamReader(stream, Encoding.UTF8, true)) state = codec.Decode(reader.ReadToEnd());
                version = state == null ? 0 : state.version;
                if (state != null && state.version > Economy.SaveVersion) { unsupported = true; return false; }
                string reason; if (!GameModel.IsPersistedValid(state, out reason)) return false;
                migrated = state != null && state.version < Economy.SaveVersion;
                state = GameModel.Migrate(state); return GameModel.IsValid(state, out reason);
            } catch (Exception e) when (e is ArgumentException || e is FormatException || e.GetType().Name == "JsonException") {
                // These identify malformed input. Unexpected codec/IO failures escape
                // to the strict pre-write path and block the transaction.
                state = null; return false;
            }
        }
        // Invoked only by a confirmed reset. Preserve both original byte streams before any write.
        public bool TryStartFresh(PlayerState state, out string error) {
            error = ""; if (!RecoveryRequired) return Save(state, out error);
            string suffix = ".recovery-" + DateTime.UtcNow.Ticks;
            try {
                foreach (string source in new[] { path, path + ".bak" }) if (Present(source)) File.Copy(source, source + suffix, false);
            } catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is NotSupportedException) {
                error = "Save failed: " + e.GetType().Name; return false;
            }
            byte[] protectedBackup;
            try {
                protectedBackup = Present(path + suffix) ? ReadOriginal(path + suffix) : ReadOriginal(path + ".bak" + suffix);
            } catch (Exception) { error = MigrationSnapshotError; return false; }
            RecoveryRequired = false;
            // This explicitly confirmed reset already preserved BOTH originals.
            if (Save(state, out error, true)) { recoveryProtectedBackup = protectedBackup; return true; }
            RecoveryRequired = true; return false;
        }
    }
}
