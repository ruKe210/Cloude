using STS.Meta.Core;

namespace STS.Meta
{
    /// <summary>
    /// Meta 层与战斗层之间的轻量会话数据。
    /// </summary>
    public static class MetaGameSession
    {
        public static string PendingScriptId;
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
