using STS.Meta.Data;

namespace STS.Meta.Core
{
    public class NarrativeContext
    {
        public NarrativeScriptDefinition Script { get; }
        public NarrativeFlagSet Flags { get; } = new NarrativeFlagSet();
        public string CurrentNodeId { get; set; }
        public NarrativeNodeEntry CurrentNode { get; set; }

        public NarrativeContext(NarrativeScriptDefinition script)
        {
            Script = script;
        }
    }
}
