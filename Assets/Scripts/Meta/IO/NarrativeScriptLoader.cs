using System.Collections.Generic;
using STS.Meta.Data;
using UnityEngine;

namespace STS.Meta.IO
{
    /// <summary>
    /// 从 Resources/MetaScripts/{scriptId}.json 加载剧本。
    /// 加载后会运行校验器，错误仅打 Log 不阻断（便于策划迭代 JSON）。
    /// </summary>
    public static class NarrativeScriptLoader
    {
        private const string ResourceFolder = "MetaScripts";

        public static NarrativeScriptDefinition Load(string scriptId)
        {
            var path = $"{ResourceFolder}/{scriptId}";
            var asset = Resources.Load<TextAsset>(path);
            if (asset == null)
            {
                Debug.LogError($"[NarrativeScriptLoader] Script not found: {path}");
                return null;
            }

            // JsonUtility 不支持 Dictionary，节点以数组形式存储，加载后 BuildNodeMap 建索引
            var script = JsonUtility.FromJson<NarrativeScriptDefinition>(asset.text);
            if (script == null)
            {
                Debug.LogError($"[NarrativeScriptLoader] Failed to parse script: {scriptId}");
                return null;
            }

            var errors = NarrativeScriptValidator.Validate(script);
            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    Debug.LogError($"[NarrativeScriptValidator] {error}");
                }
            }

            return script;
        }
    }
}
