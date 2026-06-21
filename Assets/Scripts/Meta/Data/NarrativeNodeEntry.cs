using System;

namespace STS.Meta.Data
{
    /// <summary>
    /// 单个剧本节点 DTO。type 决定 Engine 分发逻辑：line / choice / set / if。
    /// battleReturn 仅 line 节点在 next 为 battle: 时有效，指定战后回归节点。
    /// </summary>
    [Serializable]
    public class NarrativeNodeEntry
    {
        public string id;
        public string type;
        public string speaker;
        public string text;
        public string next;
        public string prompt;
        public string fallback;
        public string battleReturn;
        public float charsPerSecond = 30f;
        public bool skippable = true;
        public string[] setFlags;
        public string[] clearFlags;
        public NarrativeOptionEntry[] options;
        public NarrativeConditionEntry[] conditions;

        public NarrativeNodeType GetNodeType()
        {
            if (string.IsNullOrEmpty(type))
            {
                return NarrativeNodeType.Line;
            }

            return type.ToLowerInvariant() switch
            {
                "line" => NarrativeNodeType.Line,
                "choice" => NarrativeNodeType.Choice,
                "set" => NarrativeNodeType.Set,
                "if" => NarrativeNodeType.If,
                _ => NarrativeNodeType.Line
            };
        }
    }
}
