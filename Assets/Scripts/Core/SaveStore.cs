using System;
using System.IO;
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
        readonly string path; readonly IStateCodec codec;
        public bool RecoveryRequired { get; private set; }
        public SaveStore(string filePath, IStateCodec stateCodec) { path = filePath; codec = stateCodec; }
        public bool Save(PlayerState state, out string error) {
            error = "";
            if (RecoveryRequired) { error = RecoveryNotice; return false; }
            string reason;
            if (!GameModel.IsValid(state, out reason)) { error = reason; return false; }
            string temp = path + ".tmp";
            try {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream)) { writer.Write(codec.Encode(state)); writer.Flush(); stream.Flush(true); }
                if (File.Exists(path)) File.Replace(temp, path, path + ".bak"); else File.Move(temp, path);
                return true;
            } catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is NotSupportedException) {
                error = "Save failed: " + e.GetType().Name; return false;
            } finally { try { if (File.Exists(temp)) File.Delete(temp); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
        }
        // File.Exists hides access errors. Only a proved absent primary AND backup is a fresh profile.
        static bool Present(string source) {
            try { File.GetAttributes(source); return true; }
            catch (FileNotFoundException) { return false; }
            catch (DirectoryNotFoundException) { return false; }
        }
        public PlayerState Load(out string notice) {
            notice = ""; RecoveryRequired = false;
            try { if (!Present(path) && !Present(path + ".bak")) return new PlayerState(); }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is NotSupportedException) { return Block(out notice); }
            PlayerState state; bool migrated, unsupported;
            if (TryRead(path, out state, out migrated, out unsupported)) { if (migrated) notice = "Save upgraded: all progress kept."; return state; }
            // An older backup must never silently downgrade a future primary.
            if (unsupported) return Block(out notice);
            if (TryRead(path + ".bak", out state, out migrated, out unsupported)) { notice = "Recovered the previous local save."; return state; }
            return Block(out notice);
        }
        PlayerState Block(out string notice) { RecoveryRequired = true; notice = RecoveryNotice; return new PlayerState(); }
        bool TryRead(string source, out PlayerState state, out bool migrated, out bool unsupported) {
            state = null; migrated = unsupported = false;
            try {
                if (!Present(source) || new FileInfo(source).Length > 64 * 1024) return false;
                state = codec.Decode(File.ReadAllText(source));
                if (state != null && state.version > Economy.SaveVersion) { unsupported = true; return false; }
                string reason; if (!GameModel.IsPersistedValid(state, out reason)) return false;
                migrated = state != null && state.version < Economy.SaveVersion;
                state = GameModel.Migrate(state); return GameModel.IsValid(state, out reason);
            } catch (Exception) { state = null; return false; }
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
            RecoveryRequired = false;
            if (Save(state, out error)) return true;
            RecoveryRequired = true; return false;
        }
    }
}
