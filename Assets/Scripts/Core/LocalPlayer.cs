using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ZeroToMillionaire
{
    [Serializable] public sealed class LocalPlayer
    {
        public int version = 1;
        public long revision;
        public string displayName = "Player", avatarId = "heroine", animationMode = "full", notificationMode = "on";
        public LocalPlayer Clone() => (LocalPlayer)MemberwiseClone();
        public bool Calm => animationMode == "calm";
        public bool Notifications => notificationMode == "on";
        public static bool NormalizeName(string value, out string normalized)
        {
            normalized = "";
            if (value == null || value.Length > 40) return false;
            try { normalized = value.Normalize(NormalizationForm.FormC).Trim(); }
            catch (ArgumentException) { return false; }
            int count = new StringInfo(normalized).LengthInTextElements;
            return count >= 1 && count <= 20 && Regex.IsMatch(normalized, @"\A[\p{L}\p{M}\p{Nd} '\-]+\z");
        }
        public static bool Valid(LocalPlayer p, bool persisted)
        {
            string n;
            return p != null && p.version == 1 && p.revision >= (persisted ? 1 : 0) &&
                NormalizeName(p.displayName, out n) && n == p.displayName &&
                (p.avatarId == "heroine" || p.avatarId == "lamp" || p.avatarId == "stool") &&
                (p.animationMode == "full" || p.animationMode == "calm") &&
                (p.notificationMode == "on" || p.notificationMode == "off");
        }
        public bool SameValues(LocalPlayer other) => other != null && displayName == other.displayName && avatarId == other.avatarId && animationMode == other.animationMode && notificationMode == other.notificationMode;
    }
    public interface ILocalPlayerCodec { string Encode(LocalPlayer p); LocalPlayer Decode(string text); }

    // This file is separate from the game save; preference failures cannot debit resources.
    public sealed class LocalPlayerStore
    {
        readonly object gate = new object();
        readonly string path, legacyPath;
        readonly ILocalPlayerCodec codec;
        LocalPlayer state = new LocalPlayer();
        byte[] original, backup, legacy;
        bool persisted, blocked;
        public string Error { get; private set; } = "";
        public bool Blocked { get { lock(gate) return blocked; } }
        public LocalPlayer Snapshot { get { lock(gate) return state.Clone(); } }
        public LocalPlayerStore(string directory, ILocalPlayerCodec codec)
        {
            path = Path.Combine(directory, "local-player-v1.json");
            legacyPath = Path.Combine(directory, "presentation-v1.json");
            this.codec = codec; Reload();
        }
        static byte[] Read(string file)
        {
            try
            {
                var attributes = File.GetAttributes(file);
                if ((attributes & FileAttributes.Directory) != 0) throw new IOException("Directory at file path");
            }
            catch (FileNotFoundException) { return null; }
            catch (DirectoryNotFoundException) { return null; }
            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length > 16384) throw new IOException("Preference file too large");
                var bytes = new byte[stream.Length]; int at = 0, n;
                while (at < bytes.Length && (n = stream.Read(bytes, at, bytes.Length-at)) > 0) at += n;
                if (at != bytes.Length || stream.ReadByte() != -1) throw new IOException("Preference file changed during read");
                return bytes;
            }
        }
        static bool Equal(byte[] a, byte[] b)
        {
            if (a == null || b == null) return a == b;
            if (a.Length != b.Length) return false;
            for (int i=0;i<a.Length;i++) if(a[i]!=b[i]) return false;
            return true;
        }
        LocalPlayer Decode(byte[] bytes)
        {
            if (bytes == null) return null;
            try { var p = codec.Decode(new UTF8Encoding(false,true).GetString(bytes)); return LocalPlayer.Valid(p,true) ? p : null; }
            catch (Exception) { return null; }
        }
        static bool Expected(Exception e) => e is IOException || e is UnauthorizedAccessException || e is ArgumentException || e is NotSupportedException || e is InvalidOperationException;
        public bool Reload()
        {
            lock(gate)
            {
                try
                {
                    var p = Read(path); var b = Read(path+".bak");
                    var decoded = Decode(p); var decodedBackup = Decode(b);
                    if ((p != null && decoded == null) || (b != null && decodedBackup == null) || (p == null && b != null))
                    {
                        blocked = true;
                        Error = "Profile/settings unavailable. Original files kept. Retry local data.";
                        return false;
                    }
                    var next = decoded ?? new LocalPlayer(); byte[] oldMotion = null;
                    if (decoded == null)
                    {
                        oldMotion = Read(legacyPath);
                        if(oldMotion != null)
                        {
                            var m = Regex.Match(new UTF8Encoding(false,true).GetString(oldMotion), "\\A\\s*\\{\\s*\"reducedMotion\"\\s*:\\s*(true|false)\\s*\\}\\s*\\z");
                            if(!m.Success) throw new IOException("Legacy setting cannot be read");
                            next.animationMode = m.Groups[1].Value == "true" ? "calm" : "full";
                        }
                    }
                    state = next; original = p; backup = b; legacy = oldMotion; persisted = decoded != null;
                    blocked = false; Error = ""; return true;
                }
                catch(Exception e) when(Expected(e))
                {
                    blocked = true; Error = "Could not read profile/settings. Original files kept. Retry local data."; return false;
                }
            }
        }
        public bool Save(LocalPlayer candidate, out bool changed)
        {
            lock(gate)
            {
                changed = false;
                if(blocked) return false;
                if(!LocalPlayer.Valid(candidate,false)) { Error = "Use a name of 1–20 letters or numbers, spaces, apostrophes or hyphens."; return false; }
                if(persisted && state.SameValues(candidate)) return true; // No-op retains earlier write failure.
                if(state.revision == long.MaxValue) { Error = "Profile revision limit reached."; return false; }
                string temp = path+"."+Guid.NewGuid().ToString("N")+".tmp"; bool owned = false;
                try
                {
                    if(!Equal(original,Read(path)) || !Equal(backup,Read(path+".bak")) || (!persisted && !Equal(legacy,Read(legacyPath))))
                    { blocked = true; Error = "Profile/settings changed on disk. Retry local data."; return false; }
                    var next = candidate.Clone(); next.revision = state.revision+1;
                    var bytes = new UTF8Encoding(false,true).GetBytes(codec.Encode(next));
                    var roundtrip = Decode(bytes);
                    if(bytes.Length>16384 || roundtrip == null || !roundtrip.SameValues(next) || roundtrip.revision!=next.revision) throw new InvalidOperationException("Preference codec roundtrip");
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    using(var stream = new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None))
                    { owned = true; stream.Write(bytes,0,bytes.Length); stream.Flush(true); }
                    // Recheck after preparation, before replacing either original stream.
                    if(!Equal(original,Read(path)) || !Equal(backup,Read(path+".bak")) || (!persisted && !Equal(legacy,Read(legacyPath))))
                    { blocked = true; Error = "Profile/settings changed on disk. Retry local data."; return false; }
                    if(original!=null) File.Replace(temp,path,path+".bak"); else File.Move(temp,path);
                    backup = original; original = bytes; persisted = true; legacy = null; state = next;
                    Error = ""; changed = true; return true;
                }
                catch(Exception e) when(Expected(e))
                { Error = "Profile/settings not saved. Please retry."; return false; }
                finally
                {
                    if(owned) try { File.Delete(temp); } catch(IOException) {} catch(UnauthorizedAccessException) {}
                }
            }
        }
    }
}
