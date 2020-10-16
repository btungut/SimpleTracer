using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SimpleTracer.Internal
{
    internal class EventListenerParameters
    {
        private readonly IEnumerable<IInternalSubscriptionHandler> _subscriptionHandlers;

        internal readonly Lazy<IReadOnlyDictionary<string, Registration>> Registrations;
        internal readonly Lazy<ILookup<SourceDefinitionLookup, IInternalSubscriptionHandler>> Lookup;

        internal EventListenerParameters(IEnumerable<IInternalSubscriptionHandler> subscriptionHandlers)
        {
            _subscriptionHandlers = subscriptionHandlers;

            Registrations = new Lazy<IReadOnlyDictionary<string, Registration>>(GenerateRegistrations);
            Lookup = new Lazy<ILookup<SourceDefinitionLookup, IInternalSubscriptionHandler>>(GenerateLookups);
        }

        private ILookup<SourceDefinitionLookup, IInternalSubscriptionHandler> GenerateLookups()
        {
            var eventsWithHandlers = _subscriptionHandlers.SelectMany(handler =>
                    handler.Subscription.Events.Select(@event => new
                    {
                        Definition = @event,
                        Value = handler
                    })
                );

            var result = eventsWithHandlers
                .GroupBy(g => g.Definition.EventId.HasValue)
                .Select(g => g.ToLookup(t => new SourceDefinitionLookup(t.Definition, false), t => t.Value));

            return new MergedLookup<SourceDefinitionLookup, IInternalSubscriptionHandler>(result.ToArray());
        }

        private Dictionary<string, Registration> GenerateRegistrations()
        {
            Dictionary<string, Registration> result = new Dictionary<string, Registration>();

            foreach (ISubscriptionHandler handler in _subscriptionHandlers)
            {
                foreach (ISourceDefinition e in handler.Subscription.Events)
                {
                    if (result.ContainsKey(e.EventSource))
                    {
                        var existing = result[e.EventSource];

                        if (existing.Level < e.MinimumEventLevel)
                            existing.Level = e.MinimumEventLevel;

                        existing.Keywords = existing.Keywords | e.EventKeywords;
                    }
                    else
                    {
                        result.Add(e.EventSource, new Registration(e.EventSource, e.MinimumEventLevel, e.EventKeywords));
                    }
                }
            }

            return result;
        }
    }
}