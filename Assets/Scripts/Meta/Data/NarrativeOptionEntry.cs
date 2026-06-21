using System;

namespace STS.Meta.Data
{
    /// <summary>choice 节点的单个选项，含可见性条件与选中后写入的 Flag。</summary>
    [Serializable]
    public class NarrativeOptionEntry
    {
        public string text;
        public string next;
        public string[] setFlags;
        public string[] requireFlags;
        public string[] requireFlagsNone;
    }
}
