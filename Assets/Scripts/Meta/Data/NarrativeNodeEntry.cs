using System;

namespace STS.Meta.Data
{
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
