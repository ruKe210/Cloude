using System;
using System.Collections.Generic;
using STS.Meta.Data;

namespace STS.Meta.Core
{
    /// <summary>line 节点推送给 UI 的展示数据。</summary>
    public class NarrativeLinePayload
    {
        public string Speaker;
        public string Text;
        public float CharsPerSecond;
        public bool Skippable;
    }

    /// <summary>choice 节点推送给 UI 的展示数据。</summary>
    public class NarrativeChoicePayload
    {
        public string Prompt;
        public List<NarrativeOptionEntry> Options = new List<NarrativeOptionEntry>();
    }

    public class NarrativeEndedPayload
    {
        public string ScriptId;
    }

    /// <summary>跳转战斗时携带的场景名与战后回归节点。</summary>
    public class NarrativeBattlePayload
    {
        public string SceneName;
        public string ReturnNodeId;
    }

    /// <summary>
    /// Engine → Presentation 的状态变更事件。
    /// 根据 State 只填充对应 Payload 字段之一。
    /// </summary>
    public class NarrativeStateChangedEventArgs : EventArgs
    {
        public NarrativeState State;
        public NarrativeLinePayload Line;
        public NarrativeChoicePayload Choice;
        public NarrativeEndedPayload Ended;
        public NarrativeBattlePayload Battle;
    }
}
