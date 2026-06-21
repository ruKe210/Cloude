using STS.Meta.Core;

namespace STS.Meta
{
    /// <summary>
    /// Meta 层与战斗层之间的跨场景会话数据（静态字段，类似轻量单例）。
    /// 进入战斗前 Save，战斗结束回到 MetaStory 时 Restore。
    /// </summary>
    public static class MetaGameSession
    {
        public static string PendingScriptId;
        /// <summary>战后 ResumeFromNode 的目标节点 ID。</summary>
        public static string ReturnNodeId;
        public static NarrativeFlagSet SavedFlags;

        public static void SaveForBattle(string scriptId, string returnNodeId, NarrativeFlagSet flags)
        {
            PendingScriptId = scriptId;
            ReturnNodeId = returnNodeId;
            SavedFlags = flags;
        }

        public static bool HasPendingReturn => !string.IsNullOrEmpty(PendingScriptId);

        public static void Clear()
        {
            PendingScriptId = null;
            ReturnNodeId = null;
            SavedFlags = null;
        }
    }
}
