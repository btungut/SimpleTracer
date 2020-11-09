using System.Diagnostics.Tracing;

namespace SimpleTracer
{
    public class SystemNetHttpEvents : EventsBase
    {
        internal const string EventSource = "Microsoft-System-Net-Http";

        public SystemNetHttpEvents(PredefinedEventsCollector origin) : base(origin, EventSource, EventKeywords.All)
        {
        }
    }
}
