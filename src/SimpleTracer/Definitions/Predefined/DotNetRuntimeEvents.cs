using System.Diagnostics.Tracing;

namespace SimpleTracer
{
    public class DotNetRuntimeEvents : EventsBase
    {
        internal const string EventSource = "Microsoft-Windows-DotNETRuntime";

        public readonly GCEvents GC;

        public DotNetRuntimeEvents(PredefinedEventsCollector origin) : base(origin, EventSource, EventKeywords.All)
        {
            GC = new GCEvents(origin);
        }
    }
}
