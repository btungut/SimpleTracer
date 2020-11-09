using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;

namespace SimpleTracer
{
    public interface ISubscription : IValidatable
    {
        /// <summary>
        /// Identification value of Subscription. 
        /// It needs to be unique against other Subscriptions.
        /// GUID based random value is being set if it is not set explicitly.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The collection of <see cref="ISourceDefinition"/> instances that describes the events which is being listened.
        /// Following methods could be used to add definitions into it: <para></para>
        /// </summary>
        ReadOnlyCollection<ISourceDefinition> Events { get; }

        /// <summary>
        /// The instance of <see cref="IExecution"/> which <b>needs to be filled</b> by using one of the following methods: <para></para>
        /// </summary>
        IExecution Execution { get; }

        /// <summary>
        /// The instance of <see cref="IOptions"/> which <b>needs to be filled</b> by using one of the following methods: <para></para>
        /// </summary>
        IOptions Options { get; }
    }

    public interface IWithExecution<TParentConfigurator>
    {
        /// <summary>
        /// Sets the <see cref="IExecution"/> with given value.
        /// </summary>
        /// <param name="execution">The value which is being used by Subscription.</param>
        /// <returns>Returns the same builder object</returns>
        TParentConfigurator WithExecution(IExecution execution);

        /// <summary>
        /// Provides a delegate to configure the <see cref="IExecution"/> instance which is created implicitly.<br></br>
        /// <br><inheritdoc cref="IExecution"/></br>
        /// </summary>
        /// <param name="configurator">The delegate which serves methods to get it configured.</param>
        /// <returns>Returns the same builder object</returns>
        TParentConfigurator WithExecution(Action<IExecutionConfigurator> configurator);
    }

    public interface IWithOptions<TParentConfigurator>
    {
        /// <summary>
        /// Configures the <see cref="IOptions"/> with default values.<br></br>
        /// <br><inheritdoc cref="IOptions"/></br>
        /// </summary>
        /// <returns>Returns the same builder object</returns>
        TParentConfigurator WithOptions();

        /// <summary>
        /// Sets the <see cref="IOptions"/> with given value.
        /// </summary>
        /// <param name="options">Explicitly created instance of <see cref="IOptions"/></param>
        /// <returns>Returns the same builder object</returns>
        TParentConfigurator WithOptions(IOptions options);

        /// <summary>
        /// Provides a delegate to configure the <see cref="IOptions"/> instance which is created implicitly.
        /// </summary>
        /// <param name="configurator">The delegate which serves methods to get IOptions configured.</param>
        /// <returns>Returns the same builder object</returns>
        TParentConfigurator WithOptions(Action<IOptionsConfigurator> configurator);
    }

    public interface IWithId<TParentConfigurator>
    {
        /// <summary>
        /// Sets the identifier of Subscription with given value.<para></para>
        /// <see cref="ISubscription.Id"/> : <inheritdoc cref="ISubscription.Id"/>
        /// </summary>
        /// <param name="id">Unique identifier value</param>
        /// <returns>Returns the same builder object</returns>
        TParentConfigurator WithId(string id);
    }

    public interface IWithEvents<TParentConfigurator>
    {
        /// <summary>
        /// Configures the Subscription to listen specified <see cref="ISourceDefinition"/> which is created explicitly.
        /// </summary>
        /// <param name="events">SourceDefitions which will being listened/subscribed.</param>
        /// <returns>Returns the same builder object</returns>
        TParentConfigurator WithEvents(params ISourceDefinition[] events);

        /// <summary>
        /// Configures the Subscription to listen specified <see cref="ISourceDefinition"/> which is predefined and ready to use.
        /// </summary>
        /// <param name="predefinedEventsCollector">The delegate which serves methods to subscribe predefined and commonly used SourceDefinitions</param>
        /// <returns>Returns the same builder object</returns>
        TParentConfigurator WithEvents(Action<PredefinedEventsCollector> predefinedEventsCollector);
    }

    public interface IGeneralizedSubscriptionConfigurator : IWithId<IGeneralizedSubscriptionConfigurator>, IWithExecution<IGeneralizedSubscriptionConfigurator>, IWithOptions<IGeneralizedSubscriptionConfigurator>
    {

    }

    public interface ISubscriptionConfigurator : IWithId<ISubscriptionConfigurator>,IWithEvents<ISubscriptionConfigurator>, IWithExecution<ISubscriptionConfigurator>, IWithOptions<ISubscriptionConfigurator>
    {
        
    }
}