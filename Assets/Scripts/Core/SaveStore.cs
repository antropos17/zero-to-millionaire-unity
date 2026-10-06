using System;
using System.IO;

namespace ZeroToMillionaire
{
    public interface IStateCodec { string Encode(PlayerState state); PlayerState Decode(string text); }
    public interface IStateStore
    {
        bool Save(PlayerState state,out string error);
        PlayerState Load(out string notice);
    }

    public sealed class SaveStore : IStateStore
    {
        private readonly string path;
        private readonly IStateCodec codec;
        public SaveStore(string filePath, IStateCodec stateCodec) { path = filePath; codec = stateCodec; }

        public bool Save(PlayerState state, out string error)
        {
            error = "";
            string reason;
            if (!GameModel.IsValid(state, out reason)) { error = reason; return false; }
            string temp = path + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                // Flush file bytes before replacing the previous committed state.
                using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(stream))
                { writer.Write(codec.Encode(state)); writer.Flush(); stream.Flush(true); }
                if (File.Exists(path)) File.Replace(temp, path, path + ".bak");
                else File.Move(temp, path);
                return true;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is NotSupportedException)
            { error = "Save failed: " + e.GetType().Name; return false; }
            finally { try { if (File.Exists(temp)) File.Delete(temp); } catch (IOException) { } }
        }

        public PlayerState Load(out string notice)
        {
            notice = "";
            if (!File.Exists(path) && !File.Exists(path + ".bak")) return new PlayerState();
            PlayerState state;
            bool migrated;
            if (TryRead(path, out state, out migrated)) { if (migrated) notice = "Save upgraded: all progress kept."; return state; }
            if (TryRead(path + ".bak", out state, out migrated)) { notice = "Recovered the previous local save."; return state; }
            notice = "Local save could not be read. Starting a new room.";
            return new PlayerState();
        }
        private bool TryRead(string source, out PlayerState state, out bool migrated)
        {
            state = null; migrated = false;
            try
            {
                if (!File.Exists(source) || new FileInfo(source).Length > 64 * 1024) return false;
                state = codec.Decode(File.ReadAllText(source));
                string rawReason;if(!GameModel.IsPersistedValid(state,out rawReason))return false;
                migrated = state != null && state.version < Economy.SaveVersion;
                state = GameModel.Migrate(state);
                string reason;
                return GameModel.IsValid(state, out reason);
            }
            catch (Exception) { state = null; return false; }
        }
    }
}
