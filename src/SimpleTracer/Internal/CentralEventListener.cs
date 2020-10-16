using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleTracer.Internal
{
    internal class CentralEventListener : EventListener
    {
        private static ReadWriteLockThreadSafety<List<string>> _createdEventSources = new ReadWriteLockThreadSafety<List<string>>(new List<string>());
        internal static IReadOnlyCollection<string> CreatedEventSources => _createdEventSources.Read(list => list.AsReadOnly());


        private static CentralEventListener _instance;
        private static object _syncRoot = new object();

        private static IReadOnlyDictionary<string, Registration> _registrations;
        private static ILookup<SourceDefinitionLookup, IInternalSubscriptionHandler> _lookup;

        private CentralEventListener()
        {
        }


        internal static void CreateOrUpdate(EventListenerParameters parameters)
        {
            if (_instance == null)
            {
                lock (_syncRoot)
                {
                    if (_instance == null)
                    {
                        _registrations = parameters.Registrations.Value;
                        _lookup = parameters.Lookup.Value;
                        _instance = new CentralEventListener();
                        return;
                    }
                }
            }

            throw new NotSupportedException("Currently, creating more than one SubscriptionContainer/EventListener is not supported.");
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            if (_registrations.ContainsKey(eventSource.Name))
            {
                Registration registration = _registrations[eventSource.Name];
                EnableEvents(eventSource, registration.Level, registration.Keywords);
            }

            _createdEventSources.Write(list => list.Add(eventSource.Name));
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
