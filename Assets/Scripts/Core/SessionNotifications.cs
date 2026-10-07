using System;
using System.Collections.Generic;

namespace ZeroToMillionaire
{
    public enum SessionEvent { Spin, Purchase, Ready, Completion, Recovery, Profile, Setting, Reset, Stake }
    public sealed class SessionNotice
    {
        public readonly SessionEvent Event;public readonly int ItemId;public readonly string Text;
        public SessionNotice(SessionEvent kind,int itemId,string text){Event=kind;ItemId=itemId;Text=text;}
    }
    public sealed class SessionNotifications
    {
        public const int HistoryLimit = 20, QueueLimit = 4;
        readonly Queue<string> history = new Queue<string>();
        readonly Queue<SessionNotice> pending = new Queue<SessionNotice>();
        readonly long[] last = new long[9];
        readonly string[] critical = new string[2];
        int generation;
        public int Generation => generation;
        public bool OptionalEnabled { get; private set; } = true;
        public int PendingCount => pending.Count;
        public SessionNotice[] Pending => pending.ToArray();
        public string[] History => history.ToArray();
        public string Critical => !string.IsNullOrEmpty(critical[0]) ? critical[0] : critical[1] ?? "";
        public void Reset(PlayerState state, long localRevision)
        {
            generation++; history.Clear(); pending.Clear(); Array.Clear(last,0,last.Length);
            last[(int)SessionEvent.Spin] = state.spins;
            last[(int)SessionEvent.Purchase] = Count(state.purchased);
            last[(int)SessionEvent.Recovery] = state.recoveries;
            last[(int)SessionEvent.Profile] = last[(int)SessionEvent.Setting] = localRevision;
        }
        static int Count(bool[] flags) { int n=0; foreach(bool b in flags) if(b)n++; return n; }
        void AddHistory(string text) { while(history.Count>=HistoryLimit)history.Dequeue(); history.Enqueue(text); }
        public void RebaseLocal(long revision) { last[(int)SessionEvent.Profile]=last[(int)SessionEvent.Setting]=revision; }
        public void SetOptional(bool enabled) { OptionalEnabled=enabled; if(!enabled)pending.Clear(); }
        public bool Record(int expectedGeneration, SessionEvent channel, long id, string text, int itemId = -1)
        {
            int index=(int)channel;
            if(expectedGeneration!=generation || index<0 || index>=last.Length || id<=last[index] || string.IsNullOrWhiteSpace(text))return false;
            if(channel==SessionEvent.Ready&&(itemId<0||itemId>=Economy.SymbolCount))return false;
            last[index]=id;
            if(channel==SessionEvent.Purchase&&itemId>=0)InvalidateReady(itemId);
            AddHistory(text);
            if(OptionalEnabled){while(pending.Count>=QueueLimit)pending.Dequeue();pending.Enqueue(new SessionNotice(channel,itemId,text));}
            return true;
        }
        public void SetCritical(int source, string message)
        {
            if(source<0||source>=critical.Length)throw new ArgumentOutOfRangeException(nameof(source));
            message = message ?? "";
            if(message==critical[source])return;
            critical[source]=message;
            if(message.Length>0)AddHistory("ERROR · "+message);
        }
        public void InvalidateReady(int itemId)
        {
            int count=pending.Count;
            for(int i=0;i<count;i++){var n=pending.Dequeue();if(n.Event!=SessionEvent.Ready||n.ItemId!=itemId)pending.Enqueue(n);}
        }
        public SessionNotice TakeNotice(Func<SessionNotice,bool> valid = null)
        {
            while(pending.Count>0){var n=pending.Dequeue();if(valid==null||valid(n))return n;}
            return null;
        }
        public string Take() => TakeNotice()?.Text;
    }
}
