namespace STS.Meta.Core
{
    /// <summary>
    /// 剧本运行时状态。由 NarrativeEngine 驱动，Presentation 层只读并响应输入。
    /// </summary>
    public enum NarrativeState
    {
        Idle,
        PlayingLine,
        WaitingLineAdvance,
        WaitingChoice,
        Ended,
        BattleHandoff
    }
}
