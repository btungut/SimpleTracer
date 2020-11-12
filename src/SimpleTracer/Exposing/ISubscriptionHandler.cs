using System;

namespace SimpleTracer
{
    /// <summary>
    /// Wrapper object of <see cref="ISubscription"/> with its <see cref="SubscriptionTokenSource"/> which provides methods to get Task is manipulated in safe way.
    /// </summary>
    public interface ISubscriptionHandler : IDisposable
    {
        /// <summary>
        /// The Subscription object which was built.
        /// </summary>
        ISubscription Subscription { get; }

        /// <summary>
        /// The object which provides methods to get Task is manipulated that is reponsible to collect and notify listened events.
        /// </summary>
        SubscriptionTokenSource SubscriptionTokenSource { get; }

        /// <summary>
        /// Gets started the instance to listen and notify events.
        /// </summary>
        void Start();
    }

    internal interface IInternalSubscriptionHandler : ISubscriptionHandler
    {
        void Add(IEventEntry entry);
    }
}