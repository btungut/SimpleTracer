using System.Diagnostics.Tracing;

namespace SimpleTracer
{
    public class GCEvents : EventsBase
    {
        internal const string EventSource = "Microsoft-Windows-DotNETRuntime";
        internal const int Keyword = 0x1;

        public GCEvents(PredefinedEventsCollector origin) : base(origin, EventSource, (EventKeywords)Keyword)
        {
        }

        public PredefinedEventsCollector AllocationTick() =>    With(10, EventLevel.Verbose);
        public PredefinedEventsCollector Start() =>             With(1, EventLevel.Informational);
        public PredefinedEventsCollector End() =>               With(2, EventLevel.Informational);
    }
}
