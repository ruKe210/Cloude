using System;
using System.Collections.Generic;
using STS.Meta.Data;

namespace STS.Meta.Core
{
    public class NarrativeEngine
    {
        private readonly Dictionary<string, NarrativeNodeEntry> _nodes;
        private readonly Dictionary<string, string> _anchors;
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

        public void Start()
        {
            EnterNode(Context.Script.startNodeId);
        }

        public void ResumeFromNode(string nodeId)
        {
            EnterNode(nodeId);
        }

        public void SkipTyping()
        {
            if (State != NarrativeState.PlayingLine)
            {
                return;
            }

            State = NarrativeState.WaitingLineAdvance;
            NotifyState();
        }

        public void Advance()
        {
            if (State != NarrativeState.WaitingLineAdvance || Context.CurrentNode == null)
            {
                return;
            }

            FollowJump(Context.CurrentNode.next);
        }

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
