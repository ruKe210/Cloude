using System;
using System.Collections.Generic;

namespace STS.Meta.Data
{
    [Serializable]
    public class NarrativeScriptDefinition
    {
        public string scriptId;
        public int version;
        public string title;
        public string startNodeId;
        public NarrativeAnchorEntry[] anchors;
        public NarrativeNodeEntry[] nodes;

        public Dictionary<string, string> BuildAnchorMap()
        {
            var map = new Dictionary<string, string>();
            if (anchors == null)
            {
                return map;
            }

            foreach (var anchor in anchors)
            {
                if (anchor == null || string.IsNullOrEmpty(anchor.id))
                {
                    continue;
                }

                map[anchor.id] = anchor.nodeId;
            }

            return map;
        }

        public Dictionary<string, NarrativeNodeEntry> BuildNodeMap()
        {
            var map = new Dictionary<string, NarrativeNodeEntry>();
            if (nodes == null)
            {
                return map;
            }

            foreach (var node in nodes)
            {
                if (node == null || string.IsNullOrEmpty(node.id))
                {
                    continue;
                }

                map[node.id] = node;
            }

            return map;
        }
    }
}
