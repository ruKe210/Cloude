using STS.Meta.Data;

namespace STS.Meta.Core
{
    /// <summary>
    /// 单场剧本的运行时上下文，类似战斗层的 BattleContext。
    /// 持有剧本定义、Flag 集合与当前节点指针，不包含 UI 状态。
    /// </summary>
    public class NarrativeContext
    {
        public NarrativeScriptDefinition Script { get; }
        /// <summary>分支选择写入的标记，供 if 节点与选项条件过滤使用。</summary>
        public NarrativeFlagSet Flags { get; } = new NarrativeFlagSet();
        public string CurrentNodeId { get; set; }
        public NarrativeNodeEntry CurrentNode { get; set; }

        public NarrativeContext(NarrativeScriptDefinition script)
        {
            Script = script;
        }
    }
}
