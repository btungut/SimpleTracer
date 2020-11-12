using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace SimpleTracer
{
    /// <summary>
    /// Provides several properties/filters to describe events which wanted to be listened.<para></para>
    /// Filters except the <see cref="EventId"/> are mandatory.<br></br>
    /// Only one specific event might be listened by setting <see cref="EventId"/> with non-null value.
    /// </summary>
    public interface ISourceDefinition
    {
        /// <summary>
        /// The filter that is used to <b>match exactly</b> against <see cref="System.Diagnostics.Tracing.EventWrittenEventArgs.EventId"/>
        /// <br></br>If it is null, all of the events that matches with other filters will be listened.
        /// </summary>
        int? EventId { get; }

        /// <summary>
        /// The filter that is used to <b>match exactly</b> against <see cref="System.Diagnostics.Tracing.EventWrittenEventArgs.EventSource"/>
        /// </summary>
        string EventSource { get; }

        /// <summary>
        /// The filter that is used to <b>compare</b> against <see cref="System.Diagnostics.Tracing.EventWrittenEventArgs.Level"/>
        /// </summary>
        EventLevel MinimumEventLevel { get; }

        /// <summary>
        /// The filter that is used to <b>match with combined values</b> against <see cref="System.Diagnostics.Tracing.EventWrittenEventArgs.Keywords"/>
        /// <br></br>By using bitwise operators, multiple <see cref="System.Diagnostics.Tracing.EventKeywords"/> could be combined.
        /// </summary>
        EventKeywords EventKeywords { get; }
    }
}
