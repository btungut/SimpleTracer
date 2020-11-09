using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SimpleTracer.AcceptanceTests
{
    public class CustomEventSourceCases : BaseAcceptanceTest
    {
        [Fact]
        public void ListenAll()
        {
            #region Build ISubscriptionContainer
            var builder = SubscriptionContainerBuilder
                .New()
                .WithSubscription(subscription => subscription
                    .WithExecution(execution => execution
                        .WithDelegate(OnEventNotification)
                        .WithInterval(TimeSpan.FromSeconds(1)))
                    .WithOptions()
                    .WithEvents(new SourceDefinition(DummyEventSource.EventSourceName, EventLevel.Verbose, EventKeywords.All)));

            var container = builder.Build();
            container.Start();
            #endregion

            //Use custom event source and write events
            CallDummyClientMethods();

            //Assert
            var writtenEvents = DummyEventSource.Instance.WrittenEvents;
            var listenedEvents = base.ListenedEvents;

            Assert.True(WaitUntil(() => writtenEvents.Count == 10, TimeSpan.FromSeconds(10)));
            Assert.True(WaitUntil(() => listenedEvents.Count == 10, TimeSpan.FromSeconds(10)));

            Assert.True(CompareListsOfWrittenAndListenedEvents(writtenEvents, listenedEvents));
        }
    }

}
