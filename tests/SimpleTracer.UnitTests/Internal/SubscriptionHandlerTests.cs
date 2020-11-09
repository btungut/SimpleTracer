using SimpleTracer.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SimpleTracer.UnitTests.Internal
{
    //[Collection("Non-Parallel Collection")]
    public class SubscriptionHandlerTests
    {
        [Fact]
        public void Add_ValidStatements_EventEntryShouldBeEnqueued()
        {
            var subscription = Mock.Of<ISubscription>(s =>
                s.Options == Mock.Of<IOptions>(o =>
                    o.QueueCapacity == 10)
                );

            var handler = new SubscriptionHandler(subscription);
            var sts = StartWithPausedState(handler);

            var eventEntry = Mock.Of<IEventEntry>();
            handler.Add(eventEntry);
            Assert.True(handler.Queue.Count == 1);
            Assert.True(handler.Queue[0] == eventEntry);
        }

        //Dispose task=null
        //Dispose task is completed
        //Dispose task is not completed

        [Fact]
        public void Add_AfterDisposing_EventEntryShouldNotBeEnqueued()
        {
            var subscription = Mock.Of<ISubscription>(s =>
                s.Options == Mock.Of<IOptions>(o =>
                    o.QueueCapacity == 10)
                );

            var handler = new SubscriptionHandler(subscription);
            handler.Dispose();

            handler.Add(Mock.Of<IEventEntry>());
            Assert.True(handler.Queue.Count == 0);
        }

        [Fact]
        public void Add_CapacityIsReached_EventEntryShouldNotBeEnqueued()
        {
            var subscription = Mock.Of<ISubscription>(s =>
                s.Options == Mock.Of<IOptions>(o =>
                    o.QueueCapacity == 10)
                );

            var handler = new SubscriptionHandler(subscription);
            var sts = StartWithPausedState(handler);

            for (int i = 0; i < subscription.Options.QueueCapacity * 2; i++)
                handler.Add(Mock.Of<IEventEntry>());

            Assert.True(handler.Queue.Count == subscription.Options.QueueCapacity);
        }

        

        [Fact]
        public void Start_WithoutEnqueuedItem_TaskWillBeTriggered()
        {
            IEventNotification toBeFilled = null;
            Func<IEventNotification, Task> func = @event =>
            {
                if (toBeFilled == null)
                    toBeFilled = @event;
                return Task.CompletedTask;
            };

            var subscription = Mock.Of<ISubscription>(s =>
                s.Execution == Mock.Of<IExecution>(e =>
                    e.Delegate == func &&
                    e.Interval == TimeSpan.FromMilliseconds(100)) &&
                s.Options == Mock.Of<IOptions>(o =>
                    o.QueueCapacity == 10)
                );

            var handler = new SubscriptionHandler(subscription);
            handler.Start();

            Assert.True(Common.WaitUntil(() => toBeFilled != null, TimeSpan.FromSeconds(10)));
            Assert.True(toBeFilled.Events.Count == 0);
            Assert.True(toBeFilled.RemainingEventCount == 0);
            Assert.True(toBeFilled.LastExecution == default);
            toBeFilled = null;

            Assert.True(Common.WaitUntil(() => toBeFilled != null, TimeSpan.FromSeconds(10)));
            Assert.True(toBeFilled.LastExecution != default);
        }

        [Fact]
        public void Start_EnqueuedCountMoreThanExecutionCapacity_NotificationsShouldNotExceedTheExecutionCapacity()
        {
            List<IEventNotification> notifications = new List<IEventNotification>();
            Func<IEventNotification, Task> func = @event =>
            {
                if (@event.Events.Count > 0)
                    notifications.Add(@event);
                return Task.CompletedTask;
            };

            var subscription = Mock.Of<ISubscription>(s =>
                s.Execution == Mock.Of<IExecution>(e =>
                    e.Delegate == func &&
                    e.MaxTake == 10 &&
                    e.Interval == TimeSpan.FromMilliseconds(100)) &&
                s.Options == Mock.Of<IOptions>(o =>
                    o.QueueCapacity == 20)
                );

            var handler = new SubscriptionHandler(subscription);
            var sts = StartWithPausedState(handler);

            for (int i = 0; i < subscription.Options.QueueCapacity; i++)
                handler.Add(Mock.Of<IEventEntry>());

            sts.Resume();

            //Wait until all enqueued items are dequeued and added into the notification list to assert.
            Assert.True(Common.WaitUntil(() => handler.Queue.Count == 0, TimeSpan.FromSeconds(3)));
            Assert.True(notifications.Count == subscription.Options.QueueCapacity / subscription.Execution.MaxTake);
            Assert.True(notifications.TrueForAll(n => n.Events.Count <= subscription.Execution.MaxTake));
        }

        [Fact]
        public void Start_EnqueuedCountLowerThanExecutionCapacity_NotificationShouldContainAllEvents()
        {
            List<IEventNotification> notifications = new List<IEventNotification>();
            Func<IEventNotification, Task> func = @event =>
            {
                if (@event.Events.Count > 0)
                    notifications.Add(@event);
                return Task.CompletedTask;
            };

            var subscription = Mock.Of<ISubscription>(s =>
                s.Execution == Mock.Of<IExecution>(e =>
                    e.Delegate == func &&
                    e.MaxTake == 10 &&
                    e.Interval == TimeSpan.FromMilliseconds(1000)) &&
                s.Options == Mock.Of<IOptions>(o =>
                    o.QueueCapacity == 20)
                );

            var handler = new SubscriptionHandler(subscription);
            var sts = StartWithPausedState(handler);

            for (int i = 0; i < subscription.Execution.MaxTake / 2; i++)
                handler.Add(Mock.Of<IEventEntry>());

            sts.Resume();

            //Wait until all enqueued items are dequeued and added into the notification list to assert.
            Assert.True(Common.WaitUntil(() => handler.Queue.Count == 0, TimeSpan.FromSeconds(3)));
            Assert.True(notifications.Count == 1);
            Assert.True(notifications[0].Events.Count == subscription.Execution.MaxTake / 2);
        }

        [Fact]
        public void Start_CancelledTask_NewTaskShouldBeCreated()
        {
            int count = 0;
            Func<IEventNotification, Task> func = @event =>
            {
                count++;
                return Task.CompletedTask;
            };

            var subscription = Mock.Of<ISubscription>(s =>
                s.Execution == Mock.Of<IExecution>(e =>
                    e.Delegate == func &&
                    e.MaxTake == 100 &&
                    e.Interval == TimeSpan.FromMilliseconds(100)) &&
                s.Options == Mock.Of<IOptions>(o =>
                    o.QueueCapacity == 1000)
                );

            var handler = new SubscriptionHandler(subscription);
            var sts = new SubscriptionTokenSource();
            sts.Cancel();

            handler.Start(sts);
            Assert.False(Common.WaitUntil(() => count > 0, TimeSpan.FromSeconds(1)));

            handler.Start();
            Assert.True(Common.WaitUntil(() => count > 0, TimeSpan.FromSeconds(3)));
        }

        [Fact]
        public void Start_ThrowingExceptionInExecution_ExceptionShouldNotBeThrowedToCaller()
        {
            int toBeWaitedCount = 4;
            List<IEventNotification> notifications = new List<IEventNotification>();
            Func<IEventNotification, Task> func = @event =>
            {
                notifications.Add(@event);
                if (notifications.Count > toBeWaitedCount / 2)
                    throw new Exception();
                return Task.CompletedTask;
            };

            var subscription = Mock.Of<ISubscription>(s =>
                s.Execution == Mock.Of<IExecution>(e =>
                    e.Delegate == func &&
                    e.MaxTake == 100 &&
                    e.Interval == TimeSpan.FromMilliseconds(100)) &&
                s.Options == Mock.Of<IOptions>(o =>
                    o.QueueCapacity == 1000)
                );

            var handler = new SubscriptionHandler(subscription);
            handler.Start();

            Assert.True(Common.WaitUntil(() => notifications.Count == toBeWaitedCount, TimeSpan.FromSeconds(10)));
        }

        [Fact]
        public void Start_ValidStatement_DistanceBetweenLastExecutionsShouldBeAsMuchAsDelay()
        {
            List<IEventNotification> notifications = new List<IEventNotification>();
            Func<IEventNotification, Task> func = @event =>
            {
                if (@event.Events.Count > 0)
                    notifications.Add(@event);
                return Task.CompletedTask;
            };

            var subscription = Mock.Of<ISubscription>(s =>
                s.Execution == Mock.Of<IExecution>(e =>
                    e.Delegate == func &&
                    e.MaxTake == 100 &&
                    e.Interval == TimeSpan.FromMilliseconds(100)) &&
                s.Options == Mock.Of<IOptions>(o =>
                    o.QueueCapacity == 10)
                );

            var handler = new SubscriptionHandler(subscription);
            var sts = StartWithPausedState(handler);

            for (int i = 0; i < subscription.Options.QueueCapacity; i++)
                handler.Add(Mock.Of<IEventEntry>());

            sts.Resume();

            //Wait until all enqueued items are dequeued and added into the notification list to assert.
            Assert.True(Common.WaitUntil(() => handler.Queue.Count == 0, TimeSpan.FromSeconds(10)));

            //Skip first item, its executiontime is default value
            for (int i = 1; i < notifications.Count - 1; i++)
            {
                var distance =
                    (notifications[i + 1].LastExecution - notifications[i].LastExecution).TotalMilliseconds;
                var atLeast = subscription.Execution.Interval.TotalMilliseconds * 0.9;

                Assert.True(distance >= atLeast, $"{distance} >= {atLeast}");
            }
        }


        [Fact]
        public void Start_AfterDisposing_NoMoreNotificationShouldBeTriggered()
        {
            IDisposable disposable = null;
            int toBeWaitedCount = 3;
            List<IEventNotification> notifications = new List<IEventNotification>();
            Func<IEventNotification, Task> func = @event =>
            {
                notifications.Add(@event);
                if (notifications.Count == toBeWaitedCount)
                    disposable.Dispose();
                    
                return Task.CompletedTask;
            };

            var subscription = Mock.Of<ISubscription>(s =>
                s.Execution == Mock.Of<IExecution>(e =>
                    e.Delegate == func &&
                    e.MaxTake == 100 &&
                    e.Interval == TimeSpan.FromMilliseconds(100)) &&
                s.Options == Mock.Of<IOptions>(o =>
                    o.QueueCapacity == 1000)
                );

            var handler = new SubscriptionHandler(subscription);
            handler.Start();
            disposable = handler;

            //Wait until all enqueued items are dequeued and added into the notification list to assert.
            Common.WaitUntil(() => notifications.Count == toBeWaitedCount, TimeSpan.FromSeconds(3));

            //Wait until specified timeout is up. Any notification shouldn't be added and statement should be false.
            Assert.False(Common.WaitUntil(() => notifications.Count > toBeWaitedCount, TimeSpan.FromSeconds(1)));
        }

        private SubscriptionTokenSource StartWithPausedState(SubscriptionHandler handler)
        {
            var sts = new SubscriptionTokenSource();
            sts.Pause();
            handler.Start(sts);
            return sts;
        }
    }
}
