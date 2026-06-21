namespace STS.Meta.Core
{
    /// <summary>
    /// 剧本跳转目标的解析结果。JSON 中 next 字段的统一语法见 Parse 方法。
    /// </summary>
    public readonly struct NarrativeJumpTarget
    {
        public JumpKind Kind { get; }
        public string Value { get; }

        public NarrativeJumpTarget(JumpKind kind, string value)
        {
            Kind = kind;
            Value = value;
        }

        /// <summary>
        /// 解析跳转字符串：
        /// "n001" → 普通节点；
        /// "anchor:xxx" → 铆点；
        /// "battle:Demo0" → 战斗场景；
        /// "end" → 剧本结束。
        /// </summary>
        public static NarrativeJumpTarget Parse(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return new NarrativeJumpTarget(JumpKind.None, string.Empty);
            }

            if (raw == "end")
            {
                return new NarrativeJumpTarget(JumpKind.End, string.Empty);
            }

            if (raw.StartsWith("anchor:"))
            {
                return new NarrativeJumpTarget(JumpKind.Anchor, raw.Substring("anchor:".Length));
            }

            if (raw.StartsWith("battle:"))
            {
                return new NarrativeJumpTarget(JumpKind.Battle, raw.Substring("battle:".Length));
            }

            return new NarrativeJumpTarget(JumpKind.Node, raw);
        }
    }

    public enum JumpKind
    {
        None,
        Node,
        Anchor,
        End,
        Battle
    }
}
