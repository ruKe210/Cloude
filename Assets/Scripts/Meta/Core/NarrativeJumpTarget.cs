namespace STS.Meta.Core
{
    public readonly struct NarrativeJumpTarget
    {
        public JumpKind Kind { get; }
        public string Value { get; }

        public NarrativeJumpTarget(JumpKind kind, string value)
        {
            Kind = kind;
            Value = value;
        }

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
