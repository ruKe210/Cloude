using System.Collections.Generic;
using STS.Meta.Data;
using UnityEngine;

namespace STS.Meta.IO
{
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
