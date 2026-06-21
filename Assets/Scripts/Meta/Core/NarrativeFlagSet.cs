using System.Collections.Generic;

namespace STS.Meta.Core
{
    public class NarrativeFlagSet
    {
        private readonly HashSet<string> _flags = new HashSet<string>();

        public bool Has(string flag)
        {
            return !string.IsNullOrEmpty(flag) && _flags.Contains(flag);
        }

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
