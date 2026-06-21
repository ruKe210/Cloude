using System;

namespace STS.Meta.Data
{
    /// <summary>剧本铆点：多分支汇合时 next 写 anchor:id，此处映射到实际 nodeId。</summary>
    [Serializable]
    public class NarrativeAnchorEntry
    {
        public string id;
        public string nodeId;
    }
}
