using System.Diagnostics.Tracing;

namespace SimpleTracer
{
    public class TplEvents : EventsBase
    {
        internal const string EventSource = "System.Threading.Tasks.TplEventSource";

        public TplEvents(PredefinedEventsCollector origin) : base(origin, EventSource, EventKeywords.All)
        {
        }
    }
}
