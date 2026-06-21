using System;
using System.Collections.Generic;
using STS.Meta.Data;

namespace STS.Meta.Core
{
    public class NarrativeLinePayload
    {
        public string Speaker;
        public string Text;
        public float CharsPerSecond;
        public bool Skippable;
    }

    public class NarrativeChoicePayload
    {
        public string Prompt;
        public List<NarrativeOptionEntry> Options = new List<NarrativeOptionEntry>();
    }

    public class NarrativeEndedPayload
    {
        public string ScriptId;
    }

    public class NarrativeBattlePayload
    {
        public string SceneName;
        public string ReturnNodeId;
    }

    public class NarrativeStateChangedEventArgs : EventArgs
    {
        public NarrativeState State;
        public NarrativeLinePayload Line;
        public NarrativeChoicePayload Choice;
        public NarrativeEndedPayload Ended;
        public NarrativeBattlePayload Battle;
    }
}
