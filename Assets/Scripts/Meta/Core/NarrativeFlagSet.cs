using System.Collections.Generic;

namespace STS.Meta.Core
{
    /// <summary>
    /// 剧本分支标记集合。用于记录玩家选择，并驱动条件分支与选项可见性。
    /// </summary>
    public class NarrativeFlagSet
    {
        private readonly HashSet<string> _flags = new HashSet<string>();

        public bool Has(string flag)
        {
            return !string.IsNullOrEmpty(flag) && _flags.Contains(flag);
        }

        /// <summary>requireFlags 为空视为无条件满足。</summary>
        public bool HasAll(string[] requiredFlags)
        {
            if (requiredFlags == null || requiredFlags.Length == 0)
            {
                return true;
            }

            foreach (var flag in requiredFlags)
            {
                if (!Has(flag))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>requireFlagsNone 为空视为无禁止项。</summary>
        public bool HasNone(string[] forbiddenFlags)
        {
            if (forbiddenFlags == null || forbiddenFlags.Length == 0)
            {
                return true;
            }

            foreach (var flag in forbiddenFlags)
            {
                if (Has(flag))
                {
                    return false;
                }
            }

            return true;
        }

        public void AddRange(string[] flags)
        {
            if (flags == null)
            {
                return;
            }

            foreach (var flag in flags)
            {
                if (!string.IsNullOrEmpty(flag))
                {
                    _flags.Add(flag);
                }
            }
        }

        public void RemoveRange(string[] flags)
        {
            if (flags == null)
            {
                return;
            }

            foreach (var flag in flags)
            {
                if (!string.IsNullOrEmpty(flag))
                {
                    _flags.Remove(flag);
                }
            }
        }

        /// <summary>战斗返回 Meta 时恢复 Flag 状态。</summary>
        public void CopyFrom(NarrativeFlagSet other)
        {
            _flags.Clear();
            if (other == null)
            {
                return;
            }

            foreach (var flag in other._flags)
            {
                _flags.Add(flag);
            }
        }
    }
}
