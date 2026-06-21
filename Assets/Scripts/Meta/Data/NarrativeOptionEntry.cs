using System;

namespace STS.Meta.Data
{
    [Serializable]
    public class NarrativeOptionEntry
    {
        public string text;
        public string next;
        public string[] setFlags;
        public string[] requireFlags;
        public string[] requireFlagsNone;
    }
}
