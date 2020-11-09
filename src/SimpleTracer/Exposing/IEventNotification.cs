using System;
using System.Collections.Generic;

namespace SimpleTracer
{
    /// <summary>
    /// The object which is being served into <see cref="IExecution.Delegate"/> when it is invoked.
    /// </summary>
    public interface IEventNotification
    {
        /// <summary>
        /// Collection of listened events based on the filters provided by <see cref="ISourceDefinition"/>
        /// </summary>
        IReadOnlyCollection<IEventEntry> Events { get; }

        /// <summary>
        /// Indicates the Utc based DateTime of previous invocation.<br></br>
        /// This value is set by internally when <see cref="IExecution.Delegate"/> is finished.
        /// Therefore, it also includes the time taken in specified delegate.
        /// </summary>
        DateTime LastExecution { get; }

        /// <summary>
        /// Indicates the count of events which is listened then enqueued into internal queue and to be notified in same way.
        /// </summary>
        int RemainingEventCount { get; }
    }
}
