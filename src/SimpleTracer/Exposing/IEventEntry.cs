using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace SimpleTracer
{
    /// <summary>
    /// Describes the listened event
    /// </summary>
    public interface IEventEntry
    {
        /// <summary>
        /// Indicates the value that equivalent of <see cref="System.Diagnostics.Tracing.EventWrittenEventArgs.EventId"/>
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Indicates the value that equivalent of <see cref="System.Diagnostics.Tracing.EventWrittenEventArgs.EventName"/>
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Indicates the value that equivalent of <see cref="System.Diagnostics.Tracing.EventWrittenEventArgs.Keywords"/>
        /// </summary>
        EventKeywords Keywords { get; }

        /// <summary>
        /// Indicates the value that equivalent of <see cref="System.Diagnostics.Tracing.EventWrittenEventArgs.Level"/>
        /// </summary>
        EventLevel Level { get; }

        /// <summary>
        /// Indicates the value that equivalent of <see cref="System.Diagnostics.Tracing.EventWrittenEventArgs.Message"/>
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Indicates the value that equivalent of <see cref="System.Diagnostics.Tracing.EventWrittenEventArgs.Version"/>
        /// </summary>
        byte Version { get; }

        /// <summary>
        /// The object that describes the filters to get this event listened.
        /// </summary>
        Registration Registration { get; }
#if !NETSTANDARD2_0
        /// <summary>
        /// Indicates the creation time (in UTC) of event.<br></br>
        /// Equivalent of <see cref="System.Diagnostics.Tracing.EventWrittenEventArgs.TimeStamp"/>
        /// </summary>
        DateTime CreatedOn { get; }
#endif

        /// <summary>
        /// Indicates the listened time (in UTC) of event by internal listener.<br></br>
        /// </summary>
        DateTime ListenedOn { get; }

        /// <summary>
        /// Indicates the properties as key-value of event which was provided by its source.<br></br>
        /// Equivalent of <see cref="System.Diagnostics.Tracing.EventWrittenEventArgs.EventName"/>
        /// </summary>
        IReadOnlyDictionary<string, object> Properties { get; }
    }
}
