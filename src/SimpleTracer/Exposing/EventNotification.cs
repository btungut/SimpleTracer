using System;
using System.Collections.Generic;

namespace SimpleTracer
{
    /// <inheritdoc cref="IEventNotification"/>
    public class EventNotification : EventArgs, IEventNotification
    {
        public IReadOnlyCollection<IEventEntry> Events { get; private set; }
        public DateTime LastExecution { get; private set; }
        public int RemainingEventCount { get; private set; }


        internal EventNotification(IEventEntry[] events, DateTime lastExecution, int remainingEventCount)
        {
            Events = Array.AsReadOnly(events);
            LastExecution = lastExecution;
            RemainingEventCount = remainingEventCount;
        }
    }
}
