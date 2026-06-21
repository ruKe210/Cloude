using System;

namespace STS.Meta.Data
{
    /// <summary>if 节点的单条条件分支：requireFlags 全满足时走 next。</summary>
    [Serializable]
    public class NarrativeConditionEntry
    {
        public string[] requireFlags;
        public string next;
    }
}
