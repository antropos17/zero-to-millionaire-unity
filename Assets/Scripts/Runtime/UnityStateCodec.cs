using System;
using UnityEngine;
namespace ZeroToMillionaire
{
    public sealed class UnityStateCodec : IStateCodec
    {
        public string Encode(PlayerState state) { return JsonUtility.ToJson(state, true); }
        [Serializable] sealed class RawShape
        {public int version;public int[] fragments,lastGrid;public bool[] purchased;public QuestRecord[] quests;public TutorialState tutorial;}
        public PlayerState Decode(string text)
        {
            var state=JsonUtility.FromJson<PlayerState>(text);var raw=JsonUtility.FromJson<RawShape>(text);
            if(state==null||raw==null)return null;
            state.version=raw.version;state.fragments=raw.fragments;state.purchased=raw.purchased;
            state.lastGrid=raw.lastGrid;state.quests=raw.quests;state.tutorial=raw.tutorial;return state;
        }
    }
}
