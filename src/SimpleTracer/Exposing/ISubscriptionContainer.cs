using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Text;

namespace SimpleTracer
{
    /// <summary>
    /// Exposes various methods to manage <see cref="ISubscriptionHandler"/> instances which was created and built
    /// </summary>
    public interface ISubscriptionContainer : IReadOnlyDictionary<string, ISubscriptionHandler>, IDisposable
    {
        /// <summary>
        /// Gets started all of the <see cref="ISubscriptionHandler"/> instances to listen and notify.
        /// </summary>
        void Start();

        /// <summary>
        /// Gets paused all of the <see cref="ISubscriptionHandler"/> instances.
        /// </summary>
        void Pause();

        /// <summary>
        /// Gets resumed all of the <see cref="ISubscriptionHandler"/> instances.
        /// </summary>
        void Resume();
    }

    public interface ISubscriptionContainerBuilder
    {
        ISubscriptionContainerBuilder WithSubscription(params ISubscription[] subscriptions);
        ISubscriptionContainerBuilder WithSubscription(Action<ISubscriptionConfigurator> configurator);

        /// <summary>
        /// Creates a GeneralizedSubscription to listen <b>all of the created event sources</b> with specified minimum level.<br></br>
        /// Specifing comprehensive <paramref name="minimumEventLevel"/> like Verbose/Information could cause to unintended consequences. 
        /// </summary>
        /// <param name="minimumEventLevel">The minimum event level which is being applied to all events.</param>
        /// <param name="configurator"></param>
        /// <returns>Returns the same builder object</returns>
        ISubscriptionContainerBuilder WithSubscription(EventLevel minimumEventLevel,Action<IGeneralizedSubscriptionConfigurator> configurator);
        ISubscriptionContainer Build();
    }
}
