using System.Collections.Generic;
using STS.Meta.Core;
using STS.Meta.Data;

namespace STS.Meta.IO
{
    /// <summary>
    /// 剧本静态校验：检查 startNode、锚点、所有 next 引用是否可达。
    /// 在 Editor 与运行时加载时均可调用，避免 JSON 拼写错误导致运行时崩溃。
    /// </summary>
    public static class NarrativeScriptValidator
    {
        public static List<string> Validate(NarrativeScriptDefinition script)
        {
            var errors = new List<string>();
            if (script == null)
            {
                errors.Add("Script is null.");
                return errors;
            }

            var nodes = script.BuildNodeMap();
            var anchors = script.BuildAnchorMap();

            if (string.IsNullOrEmpty(script.startNodeId))
            {
                errors.Add("startNodeId is empty.");
            }
            else if (!nodes.ContainsKey(script.startNodeId))
            {
                errors.Add($"startNodeId not found: {script.startNodeId}");
            }

            foreach (var pair in anchors)
            {
                if (!nodes.ContainsKey(pair.Value))
                {
                    errors.Add($"Anchor '{pair.Key}' points to missing node '{pair.Value}'.");
                }
            }

            foreach (var pair in nodes)
            {
                ValidateNodeReferences(pair.Value, pair.Key, nodes, anchors, errors);
            }

            return errors;
        }

        private static void ValidateNodeReferences(
            NarrativeNodeEntry node,
            string nodeId,
            Dictionary<string, NarrativeNodeEntry> nodes,
            Dictionary<string, string> anchors,
            List<string> errors)
        {
            ValidateJump(node.next, nodeId, "next", nodes, anchors, errors);

            if (node.options != null)
            {
                for (var i = 0; i < node.options.Length; i++)
                {
                    var option = node.options[i];
                    if (option == null)
                    {
                        continue;
                    }

                    ValidateJump(option.next, nodeId, $"options[{i}].next", nodes, anchors, errors);
                }
            }

            if (node.conditions != null)
            {
                for (var i = 0; i < node.conditions.Length; i++)
                {
                    var condition = node.conditions[i];
                    if (condition == null)
                    {
                        continue;
                    }

                    ValidateJump(condition.next, nodeId, $"conditions[{i}].next", nodes, anchors, errors);
                }
            }

            ValidateJump(node.fallback, nodeId, "fallback", nodes, anchors, errors);
        }

        private static void ValidateJump(
            string rawTarget,
            string nodeId,
            string fieldName,
            Dictionary<string, NarrativeNodeEntry> nodes,
            Dictionary<string, string> anchors,
            List<string> errors)
        {
            if (string.IsNullOrEmpty(rawTarget))
            {
                return;
            }

            var target = NarrativeJumpTarget.Parse(rawTarget);
            switch (target.Kind)
            {
                case JumpKind.Node:
                    if (!nodes.ContainsKey(target.Value))
                    {
                        errors.Add($"Node '{nodeId}' field '{fieldName}' points to missing node '{target.Value}'.");
                    }

                    break;
                case JumpKind.Anchor:
                    if (!anchors.ContainsKey(target.Value))
                    {
                        errors.Add($"Node '{nodeId}' field '{fieldName}' points to missing anchor '{target.Value}'.");
                    }

                    break;
            }
        }
    }
}
