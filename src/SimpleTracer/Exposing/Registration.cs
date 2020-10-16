using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace SimpleTracer
{
    public class Registration
    {
        public string Source { get; internal set; }
        public EventLevel Level { get; internal set; }
        public EventKeywords Keywords { get; internal set; }

        internal Registration(string source, EventLevel level, EventKeywords keywords)
        {
            Source = source;
            Level = level;
            Keywords = keywords;
        }
    }
}
