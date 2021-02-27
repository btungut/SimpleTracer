using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleTracer.Internal
{
    internal class CentralEventListener : EventListener
    {
        internal static ConcurrentDictionary<Guid, CentralEventListener> Instances = new ConcurrentDictionary<Guid, CentralEventListener>();

        private bool _initiated;
        private ConcurrentQueue<EventSource> _createdEventSources;

        private IReadOnlyDictionary<string, Registration> _registrations;
        private ILookup<SourceDefinitionLookup, IInternalSubscriptionHandler> _lookup;

        internal CentralEventListener(EventListenerParameters parameters)
        {
            _registrations = parameters.Registrations.Value;
            _lookup = parameters.Lookup.Value;

            _initiated = true;

            if(_createdEventSources != null)
                while (_createdEventSources.TryDequeue(out EventSource eventSource))
                    EnableEventsIfRegistered(eventSource);

            Instances[Guid.NewGuid()] = this;
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (!_initiated)
            {
                if (_createdEventSources == null)
                    _createdEventSources = new ConcurrentQueue<EventSource>();

                _createdEventSources.Enqueue(eventSource);
            }
            else
            {
                EnableEventsIfRegistered(eventSource);
            }
        }

        private void EnableEventsIfRegistered(EventSource eventSource)
        {
            if (_registrations.ContainsKey(eventSource.Name))
            {
                Registration registration = _registrations[eventSource.Name];
                EnableEvents(eventSource, registration.Level, registration.Keywords);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            

            SourceDefinitionLookup lookup = new SourceDefinitionLookup(eventData);

            foreach (var handler in _lookup[lookup])
            {
                Enqueue(handler, eventData);
            }
        }

        private void Enqueue(IInternalSubscriptionHandler handler, EventWrittenEventArgs eventData)
        {
            try
            {
                Registration registration = _registrations[eventData.EventSource.Name];
                IEventEntry entry = new EventEntry(eventData, registration, DateTime.UtcNow);
                handler.Add(entry);
            }
            catch (Exception) { }//TODO : internal logging
        }
    }


}
