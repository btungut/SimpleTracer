using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace SimpleTracer.Internal
{
    internal struct SourceDefinitionLookup : IEquatable<SourceDefinitionLookup>
    {
        internal const int NULL_EVENTID = int.MinValue;

        private int _sourceHashCode;
        private int _eventId;
        private bool _isCreatedForLookup;

        internal SourceDefinitionLookup(ISourceDefinition definition, bool isCreatingForLookup)
            : this(definition.EventSource, definition.EventId.GetValueOrDefault(NULL_EVENTID), isCreatingForLookup)
        {
        }

        internal SourceDefinitionLookup(EventWrittenEventArgs args) 
            : this(args.EventSource.Name, args.EventId, true)
        {
        }

        internal SourceDefinitionLookup(string eventSource, int eventId, bool isCreatingForLookup)
        {
            _isCreatedForLookup = isCreatingForLookup;
            _sourceHashCode = eventSource.GetHashCode();
            _eventId = eventId;
        }

        public bool Equals(SourceDefinitionLookup other)
        {
            return
                (_sourceHashCode == other._sourceHashCode) &&
                (
                    (_eventId == other._eventId) ||
                    (other._isCreatedForLookup &&  (_eventId == NULL_EVENTID))
                );
        }

        public override int GetHashCode()
        {
            return _sourceHashCode;
        }
    }

    
}
