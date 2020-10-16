using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleTracer.Internal
{


    internal class SubscriptionHandler : ISubscriptionHandler, IDisposable, IInternalSubscriptionHandler
    {
        private readonly List<IEventEntry> _queue;
        private readonly LockBasedThreadSafety<List<IEventEntry>> _safeQueue;

        private object _taskSync = new object();
        private Task _task;

        private DateTime _lastExecuted;
        private bool _isDisposed = false;

        public SubscriptionTokenSource SubscriptionTokenSource { get; private set; }
        public ISubscription Subscription { get; private set; }
        public IReadOnlyList<IEventEntry> Queue => _queue.AsReadOnly();

        internal SubscriptionHandler(ISubscription subscription)
        {
            Subscription = subscription;
            _queue = new List<IEventEntry>(subscription.Options.QueueCapacity);
            _safeQueue = new LockBasedThreadSafety<List<IEventEntry>>(_queue);
        }

        private void InitiateTask(SubscriptionTokenSource subscriptionTokenSource)
        {
            Func<bool> state = () => _task == null || _task.IsCompleted;

            if (state())
            {
                lock (_taskSync)
                {
                    if (state())
                    {
                        SubscriptionTokenSource?.Dispose();
                        SubscriptionTokenSource = subscriptionTokenSource;

                        _task = Task.Run(async () =>
                        {
                            //Initial delay
                            await Task.Delay(Subscription.Execution.Interval).ConfigureAwait(false);

                            while (!SubscriptionTokenSource.IsCancelled)
                            {
                                try
                                {
                                    await SubscriptionTokenSource.PauseIfRequested().ConfigureAwait(false);
                                    await OnTaskRun().ConfigureAwait(false);
                                }
                                catch (Exception e)
                                {
                                    //Catch all exceptions and never allow to throw that causes application to crash.
                                    //TODO : internal logging
                                }
                                finally
                                {
                                    _lastExecuted = DateTime.UtcNow;
                                    await Task.Delay(Subscription.Execution.Interval).ConfigureAwait(false);
                                }
                            }
                        }, SubscriptionTokenSource.CancellationToken);
                    }
                }
            }
        }

        private bool IsAddingPossible()
        {
            return 
                SubscriptionTokenSource != null && 
                !SubscriptionTokenSource.IsCancelled &&
                _queue.Capacity - _queue.Count > 0;
        }

        public void Add(IEventEntry item)
        {
            if (!IsAddingPossible())
                return;

            _safeQueue.SafeAccess(collection => collection.Add(item));
        }

        public void Start()
        {
            Start(new SubscriptionTokenSource());
        }

        internal void Start(SubscriptionTokenSource subscriptionTokenSource)
        {
            InitiateTask(subscriptionTokenSource);
        }

        private Task OnTaskRun()
        {
            int remainingCount = 0;

            IEventEntry[] takenItems = _safeQueue.SafeAccess(queue =>
            {
                IEventEntry[] value = null;

                if (queue.Count == 0)
                {
                    value = Array.Empty<IEventEntry>();
                }
                else if (queue.Count >= Subscription.Execution.MaxTake)
                {
                    value = GetAndRemoveItems(queue, Subscription.Execution.MaxTake);
                }
                else
                {
                    value = GetAndRemoveItems(queue, queue.Count);
                }

                remainingCount = queue.Count;

                return value;
            });

            //Execute the delegate although it doesn't include any item
            //Otherwise developer who uses this library might think delegate is not working properly even the given interval is up.
            //So checking the count of collection and deciding what should be done is completely out of scope.
            // 3/26/2020 10:37 (UTC+3) Turkey, one of the self COVID-19 quarantine day
            try
            {
                var notification = new EventNotification(takenItems, _lastExecuted, remainingCount);
                return Subscription.Execution.Delegate(notification);
            }
            catch (Exception e)
            {
                //TODO : internal logging
                return Task.CompletedTask;
            }
        }

        private IEventEntry[] GetAndRemoveItems(List<IEventEntry> collection, int countFromBegining)
        {
            IEventEntry[] items = new IEventEntry[countFromBegining];
            collection.CopyTo(0, items, 0, countFromBegining);
            collection.RemoveRange(0, countFromBegining);

            return items;
        }


        public void Dispose()
        {
            if (!_isDisposed)
            {
                SubscriptionTokenSource?.Dispose();
                _isDisposed = true;

                GC.SuppressFinalize(this);
            }
        }
    }
}
