using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleTracer.AcceptanceTests
{
    public abstract class BaseAcceptanceTest
    {
        protected readonly List<IEventEntry> ListenedEvents;
        protected readonly static List<object> PinnedList = new List<object>();


        public BaseAcceptanceTest()
        {
            ListenedEvents = new List<IEventEntry>();
        }

        protected Task OnEventNotification(IEventNotification notification)
        {
            if (notification.Events.Count > 0)
                ListenedEvents.AddRange(notification.Events);

            return Task.CompletedTask;
        }

        protected void CallDummyClientMethods()
        {
            using (var client = new DummyClient())
            {
                var connection1 = client.CreateConnection("connection1", "10.20.30.40");
                var connection2 = client.CreateConnectionToBeThrownException("connection2", "10.20.30.40", TimeSpan.FromMilliseconds(300));
                var connection3 = client.CreateConnectionToBeTimedOut("connection3", "10.20.30.40", TimeSpan.FromMilliseconds(300));

                connection1.Open();
                connection2.Open();
                connection3.Open();
            }
        }

        protected bool WaitUntil(Func<bool> condition, TimeSpan timeout)
        {
            var started = DateTime.Now;
            while (!condition())
            {
                if (!System.Diagnostics.Debugger.IsAttached && (DateTime.Now - started) >= timeout)
                {
                    return false;
                }

                Thread.Sleep(100);
            }

            return true;
        }

        protected bool CompareListsOfWrittenAndListenedEvents(List<WrittenDummyEvent> writtenEvents, List<IEventEntry> listenedEvents)
        {
            if (writtenEvents.Count != listenedEvents.Count)
                return false;

            var writtenEventsAsOrdered = writtenEvents.OrderBy(o => o.CreatedOn).ToList();
            var listenedEventsAsOrdered = listenedEvents.OrderBy(o => o.CreatedOn).ToList();

            for (int i = 0; i < writtenEventsAsOrdered.Count; i++)
            {
                var written = writtenEventsAsOrdered[i];
                var listened = listenedEventsAsOrdered[i];

                if (written.EventId != listened.Id)
                    return false;

                if (!listened.Properties.All(l => written.Properties[l.Key].Equals(l.Value)))
                    return false;
            }

            return true;
        }
    }

    public class DummyClient : IDisposable
    {
        private readonly List<DummyConnection> _connections = new List<DummyConnection>();

        public DummyClient()
        {
            DummyEventSource.Instance.ApplicationStarted();
        }

        public DummyConnection CreateConnection(string someIdentifier, string hostname)
        {
            DummyConnection connection = new DummyConnection(someIdentifier, hostname, null);
            _connections.Add(connection);

            return connection;
        }

        public DummyConnection CreateConnectionToBeTimedOut(string someIdentifier, string hostname, TimeSpan throwTimeoutAt)
        {
            DummyConnection connection = new DummyConnection(someIdentifier, hostname, async () =>
            {
                await Task.Delay(throwTimeoutAt);
                DummyEventSource.Instance.ConnectionTimedOut(someIdentifier, hostname);
            });
            _connections.Add(connection);

            return connection;
        }

        public DummyConnection CreateConnectionToBeThrownException(string someIdentifier, string hostname, TimeSpan throwExceptionAt)
        {
            DummyConnection connection = new DummyConnection(someIdentifier, hostname, async () =>
            {
                await Task.Delay(throwExceptionAt);
                DummyEventSource.Instance.UnhandledException("An unhandled exception is occured in connection.");
            });
            _connections.Add(connection);

            return connection;
        }

        public void Dispose()
        {
            _connections.ForEach(c => c.Dispose());
            DummyEventSource.Instance.ApplicationTerminated();
        }

        public class DummyConnection : IDisposable
        {

            internal readonly string _someIdentifier;
            internal readonly string _hostname;
            private readonly Func<Task> _openFunc;

            public DummyConnection(string someIdentifier, string hostname, Func<Task> openFunc)
            {
                _someIdentifier = someIdentifier;
                _hostname = hostname;
                _openFunc = openFunc;
            }

            public void Open()
            {
                DummyEventSource.Instance.ConnectionOpened(_someIdentifier, _hostname);

                if (_openFunc != null)
                {
                    Task.Run(async () => await _openFunc().ConfigureAwait(false));
                }
            }

            public void Dispose()
            {
                DummyEventSource.Instance.ConnectionClosed(_someIdentifier, _hostname);
            }
        }
    }


    [EventSource(Name = EventSourceName)]
    public class DummyEventSource : EventSource
    {
        public const string EventSourceName = "Dummy-Communication-Library";
        public static DummyEventSource Instance = new DummyEventSource();

        public List<WrittenDummyEvent> WrittenEvents = new List<WrittenDummyEvent>();

        public class Keywords
        {
            public const EventKeywords Application = (EventKeywords)1;
            public const EventKeywords Connection = (EventKeywords)2;
        }

        [Event(1, Keywords = Keywords.Application, Level = EventLevel.Informational)]
        public void ApplicationStarted()
        {
            WrittenEvents.Add(new WrittenDummyEvent(1));
            WriteEvent(1);
        }

        [Event(2, Keywords = Keywords.Application, Level = EventLevel.Informational)]
        public void ApplicationTerminated()
        {
            WrittenEvents.Add(new WrittenDummyEvent(2));
            WriteEvent(2);
        }

        [Event(3, Keywords = Keywords.Application, Level = EventLevel.Error)]
        public void UnhandledException(string message)
        {
            WrittenEvents.Add(new WrittenDummyEvent(3, (nameof(message), message)));
            WriteEvent(3, message);
        }

        [Event(4, Keywords = Keywords.Connection, Level = EventLevel.Informational)]
        public void ConnectionOpened(string uniqueId, string hostname)
        {
            WrittenEvents.Add(new WrittenDummyEvent(4, (nameof(uniqueId), uniqueId), (nameof(hostname), hostname)));
            WriteEvent(4, uniqueId, hostname);
        }

        [Event(5, Keywords = Keywords.Connection, Level = EventLevel.Informational)]
        public void ConnectionClosed(string uniqueId, string hostname)
        {
            WrittenEvents.Add(new WrittenDummyEvent(5, (nameof(uniqueId), uniqueId), (nameof(hostname), hostname)));
            WriteEvent(5, uniqueId, hostname);
        }

        [Event(6, Keywords = Keywords.Connection, Level = EventLevel.Error)]
        public void ConnectionTimedOut(string uniqueId, string hostname)
        {
            WrittenEvents.Add(new WrittenDummyEvent(6, (nameof(uniqueId), uniqueId), (nameof(hostname), hostname)));
            WriteEvent(6, uniqueId, hostname);
        }
    }

    public class WrittenDummyEvent
    {
        public WrittenDummyEvent(int eventId, params (string Key, object Value)[] parameters)
        {
            EventId = eventId;
            if (parameters != null)
                Properties = parameters.ToDictionary(p => p.Key, p => p.Value);
            else
                Properties = new Dictionary<string, object>();

            CreatedOn = DateTime.UtcNow;
        }

        public DateTime CreatedOn { get; private set; }
        public int EventId { get; private set; }
        public IReadOnlyDictionary<string, object> Properties { get; private set; }


    }
}
