using SimpleTracer.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace SimpleTracer
{
    public class SubscriptionContainerBuilder : ISubscriptionContainerBuilder
    {
        private readonly List<ISubscription> _subscriptions;

        private SubscriptionContainerBuilder()
        {
            _subscriptions = new List<ISubscription>();
        }

        public static ISubscriptionContainerBuilder New() => new SubscriptionContainerBuilder();

        public ISubscriptionContainerBuilder WithSubscription(params ISubscription[] subscriptions)
        {
            _subscriptions.AddRange(subscriptions);

            return this;
        }

        public ISubscriptionContainerBuilder WithSubscription(Action<ISubscriptionConfigurator> configurator)
        {
            Subscription subscription = new Subscription();
            configurator(subscription);
            WithSubscription(subscription);
            return this;
        }

        public ISubscriptionContainerBuilder WithSubscription(EventLevel minimumEventLevel, Action<IGeneralizedSubscriptionConfigurator> configurator)
        {
            GeneralizedSubscription subscription = new GeneralizedSubscription(minimumEventLevel);
            configurator(subscription);
            WithSubscription(subscription);
            return this;
        }


        public ISubscriptionContainer Build()
        {
            if (_subscriptions.Count == 0)
                throw new ArgumentException("At least one subscription needs to be registered");

            var handlersAsInternal = new List<IInternalSubscriptionHandler>();
            var handlers = new List<ISubscriptionHandler>();

            foreach (ISubscription subscription in _subscriptions)
            {
                subscription.Validate();
                var handler = new SubscriptionHandler(subscription);
                handlers.Add(handler);

                if (subscription is GeneralizedSubscription)
                {
                    GeneralizedEventListener.CreateOrUpdate((GeneralizedSubscription)subscription, handler);
                }
                else
                {
                    handlersAsInternal.Add(handler);
                }
            }

            if(handlersAsInternal.Count > 0)
                new CentralEventListener(new EventListenerParameters(handlersAsInternal));

            return new SubscriptionContainer(handlers);
        }
    }
}
