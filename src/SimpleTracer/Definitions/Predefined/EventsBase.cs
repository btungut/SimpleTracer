using System.Diagnostics.Tracing;

namespace SimpleTracer
{
    public abstract class EventsBase
    {
        private readonly PredefinedEventsCollector _origin;
        private readonly string _eventSource;
        private readonly EventKeywords _keywords;
        private readonly EventKeywords _filterlessKeywords;

        internal EventsBase(PredefinedEventsCollector origin, string eventSource, EventKeywords keywords)
        {
            _origin = origin;
            _eventSource = eventSource;
            _keywords = keywords;
        }


        public PredefinedEventsCollector With(int eventId, EventLevel level) => 
            _origin.Add(new SourceDefinition(eventId, _eventSource, level, _keywords));

        public PredefinedEventsCollector AllErrors() => 
            _origin.Add(new SourceDefinition(_eventSource, EventLevel.Error, _keywords));

        public PredefinedEventsCollector AllInformationals() =>
            _origin.Add(new SourceDefinition(_eventSource, EventLevel.Informational, _keywords));

        public PredefinedEventsCollector AllVerboses() =>
            _origin.Add(new SourceDefinition(_eventSource, EventLevel.Verbose, _keywords));

        public PredefinedEventsCollector AllWarnings() =>
            _origin.Add(new SourceDefinition(_eventSource, EventLevel.Warning, _keywords));

        public PredefinedEventsCollector AllCriticals() =>
            _origin.Add(new SourceDefinition(_eventSource, EventLevel.Critical, _keywords));
    }
}
