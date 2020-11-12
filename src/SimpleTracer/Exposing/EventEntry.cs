using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;

namespace SimpleTracer
{
    /// <inheritdoc cref="IEventEntry"/>
    [Serializable]
    public class EventEntry : EventArgs, IEventEntry
    {
        internal EventEntry(int id, string name, Registration registration, DateTime createdOn, DateTime listenedOn, IList<string> payloadNames, IList<object> payloads)
        {
            Id = id;
            Name = name;
            Registration = registration;
#if !NETSTANDARD2_0
            CreatedOn = createdOn;
#endif
            ListenedOn = listenedOn;
            PopulateProperties(payloadNames, payloads);
        }

        internal EventEntry(EventWrittenEventArgs args, Registration registration, DateTime listenedOn)
        {
            Id = args.EventId;
            Name = args.EventName;
            Registration = registration;
#if !NETSTANDARD2_0
            CreatedOn = args.TimeStamp;
#endif
            ListenedOn = listenedOn;
            PopulateProperties(args.PayloadNames, args.Payload);
            Keywords = args.Keywords;
            Level = args.Level;
            Message = args.Message;
            Version = args.Version;
        }

        internal void PopulateProperties(IList<string> payloadNames, IList<object> payload)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>();

            if (payload != null && payloadNames != null)
            {
                int count = Math.Min(payload.Count, payloadNames.Count);
                for (int i = 0; i < count; i++)
                {
                    properties.Add(payloadNames[i].ToString(), payload[i]);
                }
            }

            Properties = new ReadOnlyDictionary<string, object>(properties);
        }

        public int Id { get; private set; }
        public string Name { get; private set; }
#if !NETSTANDARD2_0
        public DateTime CreatedOn { get; private set; }
#endif
        public DateTime ListenedOn { get; private set; }
        public IReadOnlyDictionary<string, object> Properties { get; private set; }
        public Registration Registration { get; private set; }
        public EventKeywords Keywords { get; private set; }
        public EventLevel Level { get; private set; }
        public string Message { get; private set; }
        public byte Version { get; private set; }
    }
}
