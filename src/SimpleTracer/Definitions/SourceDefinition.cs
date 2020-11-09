using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace SimpleTracer
{
    /// <inheritdoc cref="ISourceDefinition"/>
    public class SourceDefinition : ISourceDefinition
    {
        /// <summary>
        /// Creates a SourceDefinition to listen <b>many events</b> by specified filters, without specific EventId.
        /// </summary>
        public SourceDefinition(string eventSource, EventLevel minimumEventLevel, EventKeywords eventKeywords)
            :this(null, eventSource, minimumEventLevel, eventKeywords)
        {
        }

        /// <summary>
        /// Creates a SourceDefinition to listen <b>only one event</b> by specified filters.
        /// </summary>
        public SourceDefinition(int? eventId, string eventSource, EventLevel minimumEventLevel, EventKeywords eventKeywords)
        {
            EventId = eventId;
            EventSource = eventSource;
            MinimumEventLevel = minimumEventLevel;
            EventKeywords = eventKeywords;
        }

        public int? EventId { get; private set; }
        public string EventSource { get; private set; }
        public EventLevel MinimumEventLevel { get; private set; }
        public EventKeywords EventKeywords { get; private set; }
    }
}
