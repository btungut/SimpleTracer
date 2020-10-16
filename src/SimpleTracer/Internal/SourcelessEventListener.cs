using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace SimpleTracer.Internal
{
    internal class GeneralizedEventListener : EventListener
    {
        private static GeneralizedEventListener _instance;
        private static object _syncRoot = new object();

        private static GeneralizedSubscription _subscription;
        private static IInternalSubscriptionHandler _handler;
        private static ConcurrentDictionary<string, Registration> _registrations;

        private GeneralizedEventListener()
        {
        }


        internal static void CreateOrUpdate(GeneralizedSubscription subscription, IInternalSubscriptionHandler handler)
        {
            if (_instance == null)
            {
                lock (_syncRoot)
                {
                    if (_instance == null)
                    {
                        _subscription = subscription;
                        _handler = handler;
                        _registrations = new ConcurrentDictionary<string, Registration>();
                        _instance = new GeneralizedEventListener();
                        return;
                    }
                }
            }

            throw new NotSupportedException("Currently, creating more than one SubscriptionContainer/EventListener is not supported.");
        }

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            EventLevel level = _subscription.MinimumEventLevel;
            EventKeywords keywords = EventKeywords.All;

            //Create SourceDefinition instance by passing values
            var definition = new SourceDefinition(eventSource.Name, level, keywords);

            //Add this eventSource into subscription
            ((ISubscriptionConfigurator)_subscription.ProxiedSubscription).WithEvents(definition);

            //Add this also into registration dictionary
            _registrations.AddOrUpdate(
                eventSource.Name,
                (source) => new Registration(source, level, keywords),
                (source, _) => new Registration(source, level, keywords)
                );
            
            //Enable to listen
            EnableEvents(eventSource, _subscription.Events[0].MinimumEventLevel, EventKeywords.All);
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            Enqueue(_handler, eventData);
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
