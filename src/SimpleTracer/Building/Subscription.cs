using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;

namespace SimpleTracer
{
    public class Subscription : ISubscription, ISubscriptionConfigurator
    {
        private readonly List<ISourceDefinition> _events;

        internal Subscription()
        {
            _events = new List<ISourceDefinition>();
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; private set; }
        public ReadOnlyCollection<ISourceDefinition> Events => _events.AsReadOnly();
        public IExecution Execution { get; private set; }
        public IOptions Options { get; private set; }

        public void Validate()
        {
            ValidateId();
            ValidateEvents();
            ValidateExecution();
            ValidateOptions();

            Execution.Validate();
            Options.Validate();
        }

        internal void ValidateOptions()
        {
            if (Options == null)
                throw new ArgumentNullException(nameof(Options),
                    $"Subscription couldn't be created without '{nameof(Options)}'." +
                    $"Use '{nameof(ISubscriptionConfigurator.WithOptions)}' method to get '{nameof(Options)}' is configured.");
        }

        internal void ValidateExecution()
        {
            if (Execution == null)
                throw new ArgumentNullException(nameof(Execution),
                    $"Subscription couldn't be created without '{nameof(Execution)}'." +
                    $"Use '{nameof(ISubscriptionConfigurator.WithExecution)}' method to get '{nameof(Execution)}' is configured.");
        }

        internal void ValidateEvents()
        {
            if (Events.Count == 0)
                throw new ArgumentNullException(nameof(Events),
                    $"Subscription should have at least one Event to be listened. " +
                    $"Use '{nameof(ISubscriptionConfigurator.WithEvents)}' method to listen an Event.");
        }

        internal void ValidateId()
        {
            if (string.IsNullOrEmpty(Id))
                throw new ArgumentNullException(nameof(Id),
                    $"Subscription Id couldn't be null or empty." +
                    $"Use '{nameof(ISubscriptionConfigurator.WithId)}' method to set valid Id.");
        }

        public ISubscriptionConfigurator WithOptions()
        {
            Options = new Options();
            return this;
        }

        ISubscriptionConfigurator IWithEvents<ISubscriptionConfigurator>.WithEvents(params ISourceDefinition[] events)
        {
            _events.AddRange(events);
            return this;
        }

        ISubscriptionConfigurator IWithEvents<ISubscriptionConfigurator>.WithEvents(Action<PredefinedEventsCollector> action)
        {
            var predefinedEvents = new PredefinedEventsCollector();
            action(predefinedEvents);
            _events.AddRange(predefinedEvents.SourceDefinitions);
            return this;
        }



        ISubscriptionConfigurator IWithExecution<ISubscriptionConfigurator>.WithExecution(IExecution execution)
        {
            Execution = execution;
            return this;
        }

        ISubscriptionConfigurator IWithExecution<ISubscriptionConfigurator>.WithExecution(Action<IExecutionConfigurator> configurator)
        {
            var created = new Execution();
            configurator(created);
            Execution = created;

            return this;
        }

        ISubscriptionConfigurator IWithId<ISubscriptionConfigurator>.WithId(string id)
        {
            Id = id;
            return this;
        }

        ISubscriptionConfigurator IWithOptions<ISubscriptionConfigurator>.WithOptions(IOptions options)
        {
            Options = options;
            return this;
        }

        ISubscriptionConfigurator IWithOptions<ISubscriptionConfigurator>.WithOptions(Action<IOptionsConfigurator> configurator)
        {
            var created = new Options();
            configurator(created);
            Options = created;

            return this;
        }
    }

    

    public class GeneralizedSubscription : ISubscription, IGeneralizedSubscriptionConfigurator
    {
        internal Subscription ProxiedSubscription { get; private set; }
        internal EventLevel MinimumEventLevel { get; private set; }

        internal GeneralizedSubscription(EventLevel minimumEventLevel)
        {
            MinimumEventLevel = minimumEventLevel;
            ProxiedSubscription = new Subscription();
        }

        public string Id => ProxiedSubscription.Id;

        public ReadOnlyCollection<ISourceDefinition> Events => ProxiedSubscription.Events;

        public IExecution Execution => ProxiedSubscription.Execution;

        public IOptions Options => ProxiedSubscription.Options;

        public void Validate()
        {
            //Do not validate Events, it should be empty at this phase.
            ProxiedSubscription.ValidateId();
            ProxiedSubscription.ValidateExecution();
            ProxiedSubscription.ValidateOptions();
        }

        IGeneralizedSubscriptionConfigurator IWithExecution<IGeneralizedSubscriptionConfigurator>.WithExecution(IExecution execution)
        {
            ((IWithExecution<ISubscriptionConfigurator>)ProxiedSubscription).WithExecution(execution);
            return this;
        }

        IGeneralizedSubscriptionConfigurator IWithExecution<IGeneralizedSubscriptionConfigurator>.WithExecution(Action<IExecutionConfigurator> configurator)
        {
            ((IWithExecution<ISubscriptionConfigurator>)ProxiedSubscription).WithExecution(configurator);
            return this;
        }

        IGeneralizedSubscriptionConfigurator IWithId<IGeneralizedSubscriptionConfigurator>.WithId(string id)
        {
            ((IWithId<ISubscriptionConfigurator>)ProxiedSubscription).WithId(id);
            return this;
        }

        IGeneralizedSubscriptionConfigurator IWithOptions<IGeneralizedSubscriptionConfigurator>.WithOptions()
        {
            ((IWithOptions<ISubscriptionConfigurator>)ProxiedSubscription).WithOptions();
            return this;
        }

        IGeneralizedSubscriptionConfigurator IWithOptions<IGeneralizedSubscriptionConfigurator>.WithOptions(IOptions options)
        {
            ((IWithOptions<ISubscriptionConfigurator>)ProxiedSubscription).WithOptions(options);
            return this;
        }

        IGeneralizedSubscriptionConfigurator IWithOptions<IGeneralizedSubscriptionConfigurator>.WithOptions(Action<IOptionsConfigurator> configurator)
        {
            ((IWithOptions<ISubscriptionConfigurator>)ProxiedSubscription).WithOptions(configurator);
            return this;
        }

        internal class GeneralizedSourceDefinition : ISourceDefinition
        {
            internal const string FixedEventSource = "ALL";
            private readonly EventLevel _minimumEventLevel;

            internal GeneralizedSourceDefinition(EventLevel minimumEventLevel)
            {
                _minimumEventLevel = minimumEventLevel;
            }

            public int? EventId => null;

            public string EventSource => FixedEventSource;

            public EventLevel MinimumEventLevel => _minimumEventLevel;

            public EventKeywords EventKeywords => EventKeywords.All;
        }
    }
}
