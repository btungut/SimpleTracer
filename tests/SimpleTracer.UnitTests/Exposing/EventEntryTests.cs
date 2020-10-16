using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xunit;

namespace SimpleTracer.UnitTests.Exposing
{
    public class EventEntryTests
    {
        [Fact]
        public void Ctor_NullPayload_PropertiesShouldBeEmpty()
        {
            EventEntry sut = new EventEntry(
                default, default, default, default, default, new List<string> { "name1", "name2" }, null);

            Assert.True(sut.Properties.Count == 0);
        }

        [Fact]
        public void Ctor_NullPayloadNames_PropertiesShouldBeEmpty()
        {
            EventEntry sut = new EventEntry(
                default, default, default, default, default, null, new List<object> { "value1", "value2" });

            Assert.True(sut.Properties.Count == 0);
        }

        [Fact]
        public void Ctor_SomeKeysWithoutValues_PropertiesShouldContainMatchedPairs()
        {
            var payloadNames = new List<string> { "key1", "key2", "key3" }; //3 keys
            var payloads = new List<object> { "value1", "value2" }; //2 values

            EventEntry sut = new EventEntry(
                default, default, default, default, default, payloadNames, payloads);

            Assert.True(sut.Properties.Count == Math.Min(payloadNames.Count, payloads.Count));
        }
    }
}
