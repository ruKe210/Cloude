using System;
using System.Collections.Generic;
using STS.Meta.Data;

namespace STS.Meta.Core
{
    /// <summary>
    /// 剧本运行时核心：解析 JSON 节点、维护状态机、处理跳转与 Flag。
    /// 纯 C# 类，不依赖 MonoBehaviour；UI 通过 StateChanged 事件订阅。
    /// </summary>
    public class NarrativeEngine
    {
        private readonly Dictionary<string, NarrativeNodeEntry> _nodes;
        private readonly Dictionary<string, string> _anchors;
        /// <summary>当前 choice 节点经过条件过滤后，真正展示给玩家的选项。</summary>
        private readonly List<NarrativeOptionEntry> _visibleOptions = new List<NarrativeOptionEntry>();

        public NarrativeContext Context { get; private set; }
        public NarrativeState State { get; private set; } = NarrativeState.Idle;

        public event Action<NarrativeStateChangedEventArgs> StateChanged;

        public NarrativeEngine(NarrativeScriptDefinition script)
        {
            _nodes = script.BuildNodeMap();
            _anchors = script.BuildAnchorMap();
            Context = new NarrativeContext(script);
        }

        /// <summary>从 startNodeId 开始播放剧本。</summary>
        public void Start()
        {
            EnterNode(Context.Script.startNodeId);
        }

        /// <summary>从指定节点恢复（战斗返回 Meta 时使用）。</summary>
        public void ResumeFromNode(string nodeId)
        {
            EnterNode(nodeId);
        }

        /// <summary>左键跳过打字机：PlayingLine → WaitingLineAdvance。</summary>
        public void SkipTyping()
        {
            if (State != NarrativeState.PlayingLine)
            {
                return;
            }

            State = NarrativeState.WaitingLineAdvance;
            NotifyState();
        }

        /// <summary>全文显示后左键确认，沿当前 line 节点的 next 跳转。</summary>
        public void Advance()
        {
            if (State != NarrativeState.WaitingLineAdvance || Context.CurrentNode == null)
            {
                return;
            }

            FollowJump(Context.CurrentNode.next);
        }

        /// <summary>玩家点选选项：写入 Flag 并跳转。</summary>
        public void SelectOption(int index)
        {
            if (State != NarrativeState.WaitingChoice || index < 0 || index >= _visibleOptions.Count)
            {
                return;
            }

            var option = _visibleOptions[index];
            Context.Flags.AddRange(option.setFlags);
            FollowJump(option.next);
        }

        /// <summary>
        /// 进入节点并根据 type 分发。
        /// set / if 节点会立即自动跳转，不等待玩家输入。
        /// </summary>
        private void EnterNode(string nodeId)
        {
            if (string.IsNullOrEmpty(nodeId))
            {
                EndScript();
                return;
            }

            if (!_nodes.TryGetValue(nodeId, out var node))
            {
                UnityEngine.Debug.LogError($"[NarrativeEngine] Node not found: {nodeId}");
                EndScript();
                return;
            }

            Context.CurrentNodeId = nodeId;
            Context.CurrentNode = node;

            switch (node.GetNodeType())
            {
                case NarrativeNodeType.Line:
                    PlayLine(node);
                    break;
                case NarrativeNodeType.Choice:
                    ShowChoice(node);
                    break;
                case NarrativeNodeType.Set:
                    ApplySet(node);
                    FollowJump(node.next);
                    break;
                case NarrativeNodeType.If:
                    FollowJump(ResolveIf(node));
                    break;
            }
        }

        private void PlayLine(NarrativeNodeEntry node)
        {
            State = NarrativeState.PlayingLine;
            NotifyState(new NarrativeStateChangedEventArgs
            {
                State = State,
                Line = new NarrativeLinePayload
                {
                    Speaker = node.speaker,
                    Text = node.text ?? string.Empty,
                    CharsPerSecond = node.charsPerSecond <= 0f ? 30f : node.charsPerSecond,
                    Skippable = node.skippable
                }
            });
        }

        /// <summary>按 requireFlags / requireFlagsNone 过滤不可见选项。</summary>
        private void ShowChoice(NarrativeNodeEntry node)
        {
            _visibleOptions.Clear();
            if (node.options != null)
            {
                foreach (var option in node.options)
                {
                    if (option == null)
                    {
                        continue;
                    }

                    if (!Context.Flags.HasAll(option.requireFlags))
                    {
                        continue;
                    }

                    if (!Context.Flags.HasNone(option.requireFlagsNone))
                    {
                        continue;
                    }

                    _visibleOptions.Add(option);
                }
            }

            if (_visibleOptions.Count == 0)
            {
                UnityEngine.Debug.LogError($"[NarrativeEngine] Choice node {node.id} has no visible options.");
                EndScript();
                return;
            }

            State = NarrativeState.WaitingChoice;
            NotifyState(new NarrativeStateChangedEventArgs
            {
                State = State,
                Choice = new NarrativeChoicePayload
                {
                    Prompt = node.prompt ?? node.text ?? string.Empty,
                    Options = new List<NarrativeOptionEntry>(_visibleOptions)
                }
            });
        }

        private void ApplySet(NarrativeNodeEntry node)
        {
            Context.Flags.AddRange(node.setFlags);
            Context.Flags.RemoveRange(node.clearFlags);
        }

        /// <summary>按 conditions 顺序匹配，首个满足的 next 生效；否则走 fallback。</summary>
        private string ResolveIf(NarrativeNodeEntry node)
        {
            if (node.conditions != null)
            {
                foreach (var condition in node.conditions)
                {
                    if (condition == null)
                    {
                        continue;
                    }

                    if (Context.Flags.HasAll(condition.requireFlags))
                    {
                        return condition.next;
                    }
                }
            }

            return node.fallback;
        }

        /// <summary>
        /// 统一跳转入口。anchor 用于多分支汇合；battle 暂停剧本并通知 Presentation 切场景。
        /// </summary>
        private void FollowJump(string rawTarget)
        {
            var target = NarrativeJumpTarget.Parse(rawTarget);
            switch (target.Kind)
            {
                case JumpKind.None:
                    EndScript();
                    break;
                case JumpKind.End:
                    EndScript();
                    break;
                case JumpKind.Node:
                    EnterNode(target.Value);
                    break;
                case JumpKind.Anchor:
                    if (_anchors.TryGetValue(target.Value, out var anchorNodeId))
                    {
                        EnterNode(anchorNodeId);
                    }
                    else
                    {
                        UnityEngine.Debug.LogError($"[NarrativeEngine] Anchor not found: {target.Value}");
                        EndScript();
                    }

                    break;
                case JumpKind.Battle:
                    State = NarrativeState.BattleHandoff;
                    NotifyState(new NarrativeStateChangedEventArgs
                    {
                        State = State,
                        Battle = new NarrativeBattlePayload
                        {
                            SceneName = target.Value,
                            // battleReturn 写在触发战斗的 line 节点上，战后从此节点继续
                            ReturnNodeId = string.IsNullOrEmpty(Context.CurrentNode?.battleReturn)
                                ? null
                                : Context.CurrentNode.battleReturn
                        }
                    });
                    break;
            }
        }

        private void EndScript()
        {
            State = NarrativeState.Ended;
            NotifyState(new NarrativeStateChangedEventArgs
            {
                State = State,
                Ended = new NarrativeEndedPayload
                {
                    ScriptId = Context.Script.scriptId
                }
            });
        }

        /// <summary>打字机自然播完时由 Presentation 调用。</summary>
        public void MarkWaitingAdvance()
        {
            if (State != NarrativeState.PlayingLine)
            {
                return;
            }

            State = NarrativeState.WaitingLineAdvance;
            NotifyState();
        }

        private void NotifyState(NarrativeStateChangedEventArgs args = null)
        {
            args ??= new NarrativeStateChangedEventArgs { State = State };
            args.State = State;
            StateChanged?.Invoke(args);
        }
    }
}
