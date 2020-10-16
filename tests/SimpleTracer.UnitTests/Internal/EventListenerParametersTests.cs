using SimpleTracer.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleTracer.UnitTests.Internal
{
    public class EventListenerParametersTests
    {
        [Fact]
        public void Registration_DummyEvent_AllPropertiesShouldBeSame()
        {
            var definition = Mock.Of<ISourceDefinition>(d => 
                d.EventSource == "Source" && 
                d.EventKeywords == (EventKeywords)10 &&
                d.MinimumEventLevel == EventLevel.Informational
            );

            var handlers = new IInternalSubscriptionHandler[]
            {
                Mock.Of<IInternalSubscriptionHandler>(h =>
                    h.Subscription == Mock.Of<ISubscription>(s =>
                        s.Id == Guid.NewGuid().ToString() &&
                        s.Events == new List<ISourceDefinition>
                        {
                            definition
                        }.AsReadOnly()))
            };

            var sut = new EventListenerParameters(handlers).Registrations.Value;

            Assert.True(sut.Count == 1);
            Assert.True(sut[definition.EventSource].Source == definition.EventSource);
            Assert.True(sut[definition.EventSource].Keywords == definition.EventKeywords);
            Assert.True(sut[definition.EventSource].Level == definition.MinimumEventLevel);
        }

        [Fact]
        public void Registration_TwoDefinitionWithSameSourceDifferentKeywords_KeywordsShouldBeMerged()
        {
            var definition1 = Mock.Of<ISourceDefinition>(d =>
                d.EventSource == "Source" &&
                d.EventKeywords == (EventKeywords)10 &&
                d.MinimumEventLevel == EventLevel.Informational
            );

            var definition2 = Mock.Of<ISourceDefinition>(d =>
                d.EventSource == "Source" &&
                d.EventKeywords == (EventKeywords)20 &&
                d.MinimumEventLevel == EventLevel.Informational
            );

            var handlers = new IInternalSubscriptionHandler[]
            {
                Mock.Of<IInternalSubscriptionHandler>(h =>
                    h.Subscription == Mock.Of<ISubscription>(s =>
                        s.Id == Guid.NewGuid().ToString() &&
                        s.Events == new List<ISourceDefinition>
                        {
                            definition1,
                            definition2
                        }.AsReadOnly()))
            };

            var sut = new EventListenerParameters(handlers).Registrations.Value;

            Assert.True(sut.Count == 1);
            Assert.True(sut[definition1.EventSource].Keywords == (definition1.EventKeywords | definition2.EventKeywords));
        }

        [Fact]
        public void Registration_TwoDefinitionWithSameSourceDifferentLevel_LevelShouldBeHighestOne()
        {
            var definition1 = Mock.Of<ISourceDefinition>(d =>
                d.EventSource == "Source" &&
                d.EventKeywords == (EventKeywords)10 &&
                d.MinimumEventLevel == EventLevel.Informational
            );

            var definition2 = Mock.Of<ISourceDefinition>(d =>
                d.EventSource == "Source" &&
                d.EventKeywords == (EventKeywords)20 &&
                d.MinimumEventLevel == EventLevel.Verbose
            );

            var handlers = new IInternalSubscriptionHandler[]
            {
                Mock.Of<IInternalSubscriptionHandler>(h =>
                    h.Subscription == Mock.Of<ISubscription>(s =>
                        s.Id == Guid.NewGuid().ToString() &&
                        s.Events == new List<ISourceDefinition>
                        {
                            definition1,
                            definition2
                        }.AsReadOnly()))
            };

            var sut = new EventListenerParameters(handlers).Registrations.Value;
            var expected = (EventLevel)Math.Max((int)definition1.MinimumEventLevel, (int)definition2.MinimumEventLevel);
            Assert.True(sut.Count == 1);
            Assert.True(sut[definition1.EventSource].Level == expected);
        }

        [Fact]
        public void Lookup_WithNotExistedSourceDefinition_ShouldServeNoHandler()
        {
            var source1_EventAll = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1");
            var source1_Event10 = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1" && d.EventId == 10);

            var handlers = new IInternalSubscriptionHandler[]
            {
                Mock.Of<IInternalSubscriptionHandler>(h =>
                    h.Subscription == Mock.Of<ISubscription>(s =>
                        s.Id == Guid.NewGuid().ToString() &&
                        s.Events == new List<ISourceDefinition>
                        {
                            source1_EventAll,
                            source1_Event10
                        }.AsReadOnly()))
            };

            var notExistedSource = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source2");
            var sut = new EventListenerParameters(handlers).Lookup.Value;
            var actual = sut[new SourceDefinitionLookup(notExistedSource, true)];

            Assert.True(actual.Count() == 0);
        }

        [Fact]
        public void Lookup_WithEventDefinitionExistedNameNotExistedEventId_ShouldServeNoHandler()
        {
            var source1_Event10 = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1" && d.EventId == 10);
            var source1_Event20 = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1" && d.EventId == 20);

            var handlers = new IInternalSubscriptionHandler[]
            {
                Mock.Of<IInternalSubscriptionHandler>(h =>
                    h.Subscription == Mock.Of<ISubscription>(s =>
                        s.Id == Guid.NewGuid().ToString() &&
                        s.Events == new List<ISourceDefinition>
                        {
                            source1_Event10,
                            source1_Event20
                        }.AsReadOnly()))
            };

            var notExisted = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1" && d.EventId == 30);
            var sut = new EventListenerParameters(handlers).Lookup.Value;
            var actual = sut[new SourceDefinitionLookup(notExisted, true)];

            Assert.True(actual.Count() == 0);
        }

        [Fact]
        public void Lookup_SourceDefinition_ShouldServeHandler()
        {
            var source1_EventAll = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1");
            var source1_Event10 = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1" && d.EventId == 10);
            var source1_Event20 = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1" && d.EventId == 20);
            var source1_EventAll_SubscriptionId = Guid.NewGuid().ToString();

            var handlers = new IInternalSubscriptionHandler[]
            {
                Mock.Of<IInternalSubscriptionHandler>(h =>
                    h.Subscription == Mock.Of<ISubscription>(s =>
                        s.Id == source1_EventAll_SubscriptionId &&
                        s.Events == new List<ISourceDefinition>
                        {
                            source1_EventAll
                        }.AsReadOnly())),

                Mock.Of<IInternalSubscriptionHandler>(h =>
                    h.Subscription == Mock.Of<ISubscription>(s =>
                        s.Id == Guid.NewGuid().ToString() &&
                        s.Events == new List<ISourceDefinition>
                        {
                            source1_Event10,
                            source1_Event20
                        }.AsReadOnly()))
            };

            var source1_Event30 = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1" && d.EventId == 30);
            var sut = new EventListenerParameters(handlers).Lookup.Value;
            var actual = sut[new SourceDefinitionLookup(source1_Event30, true)];

            Assert.True(actual.Count() == 1);
            Assert.True(actual.First().Subscription.Id == source1_EventAll_SubscriptionId);
        }

        [Fact]
        public void LookupWithExistedEventId_BothOfSourceAndEventDefinition_ShouldServeBothHandlers()
        {
            var source1_EventAll = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1");
            var source1_Event10 = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1" && d.EventId == 10);
            var source1_EventAll_SubscriptionId = Guid.NewGuid().ToString();
            var source1_Event10_SubscriptionId = Guid.NewGuid().ToString();

            var handlers = new IInternalSubscriptionHandler[]
            {
                Mock.Of<IInternalSubscriptionHandler>(h =>
                    h.Subscription == Mock.Of<ISubscription>(s =>
                        s.Id == source1_EventAll_SubscriptionId &&
                        s.Events == new List<ISourceDefinition>
                        {
                            source1_Event10,
                            source1_EventAll
                        }.AsReadOnly())),

                Mock.Of<IInternalSubscriptionHandler>(h =>
                    h.Subscription == Mock.Of<ISubscription>(s =>
                        s.Id == source1_Event10_SubscriptionId &&
                        s.Events == new List<ISourceDefinition>
                        {
                            source1_Event10
                        }.AsReadOnly()))
            };

            var sut = new EventListenerParameters(handlers).Lookup.Value;
            var actual = sut[new SourceDefinitionLookup(source1_Event10, true)];

            Assert.True(actual.Count() == 2);
            Assert.True(actual.All(a => new string[] { source1_EventAll_SubscriptionId, source1_Event10_SubscriptionId }.Contains(a.Subscription.Id)));
        }

        [Fact]
        public void LookupWithNotExistedEventId_BothOfSourceAndEventDefinition_ShouldServeBothHandlers()
        {
            var source1_EventAll = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1");
            var source1_Event10 = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1" && d.EventId == 10);
            var source1_Event20 = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1" && d.EventId == 20);
            var source1_EventAll_SubscriptionId = Guid.NewGuid().ToString();
            var source1_Event10_SubscriptionId = Guid.NewGuid().ToString();

            var handlers = new IInternalSubscriptionHandler[]
            {
                Mock.Of<IInternalSubscriptionHandler>(h =>
                    h.Subscription == Mock.Of<ISubscription>(s =>
                        s.Id == source1_EventAll_SubscriptionId &&
                        s.Events == new List<ISourceDefinition>
                        {
                            source1_Event10,
                            source1_EventAll
                        }.AsReadOnly())),

                Mock.Of<IInternalSubscriptionHandler>(h =>
                    h.Subscription == Mock.Of<ISubscription>(s =>
                        s.Id == source1_Event10_SubscriptionId &&
                        s.Events == new List<ISourceDefinition>
                        {
                            source1_Event10
                        }.AsReadOnly()))
            };

            var sut = new EventListenerParameters(handlers).Lookup.Value;
            var actual = sut[new SourceDefinitionLookup(source1_Event20, true)];

            Assert.True(actual.Count() == 1);
            Assert.True(actual.First().Subscription.Id == source1_EventAll_SubscriptionId);
        }

        [Fact]
        public void Lookup_SameEventDefinitionAccrossMultipleHandlers_ShouldServeAllHandlers()
        {
            var sourceDefinition = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source" && d.EventId == 10);

            var handlers = Enumerable.Range(1, 10).Select((_) =>
             {
                 return Mock.Of<IInternalSubscriptionHandler>(h =>
                     h.Subscription == Mock.Of<ISubscription>(s =>
                         s.Id == Guid.NewGuid().ToString() &&
                         s.Events == new List<ISourceDefinition>
                         {
                            sourceDefinition
                         }.AsReadOnly()));
             }).ToList(); //It should be enumerated/executed

            var sut = new EventListenerParameters(handlers).Lookup.Value;
            var actual = sut[new SourceDefinitionLookup(sourceDefinition, true)];

            Assert.True(sut.Count == 1);
            Assert.True(actual.Count() == handlers.Count());
            Assert.True(actual.All(a => handlers.Count(h => h.Subscription.Id == a.Subscription.Id) == 1));
        }

        [Fact]
        public void Lookup_SpecificDefinitionAcrossMultipleHandlers_ShouldServeAllHandlersThatIncludesSpecificDefinition()
        {
            //Create random number in range of 3-20 then create definitions as much as the randomized number
            //Like : Source:1, Source:2, Source:3 ..... Source:15
            Func<List<ISourceDefinition>> sourceDefinitionFunc = () => Enumerable
                 .Range(1, new Random().Next(3, 20))
                 .Select(i => (ISourceDefinition)Mock.Of<ISourceDefinition>(d => d.EventSource == $"Source:{i}" && d.EventId == 10))
                 .ToList();

            //Create 10 handlers and fill it with randomized definitions. Every handler can have various definitions.
            var handlers = Enumerable.Range(1, 10).Select((_) =>
            {
                return Mock.Of<IInternalSubscriptionHandler>(h =>
                    h.Subscription == Mock.Of<ISubscription>(s =>
                        s.Id == Guid.NewGuid().ToString() &&
                        s.Events == sourceDefinitionFunc().AsReadOnly()));
            }).ToList(); //It should be enumerated/executed

            //Order all EventSource names to pick up a definition that appears neither less nor more.
            //Items on the top are the ones that appears less.
            var eventsAsOrdered = handlers
                .SelectMany(h => h.Subscription.Events)
                .OrderBy(h => h.EventSource);
            var definition = eventsAsOrdered.ElementAt((int)(eventsAsOrdered.Count() * 0.2));
            var definitionAppearingCount = eventsAsOrdered.Count(h => h.EventSource == definition.EventSource);


            var sut = new EventListenerParameters(handlers).Lookup.Value;
            var actual = sut[new SourceDefinitionLookup(definition, true)];

            //Lookup item count should equal total count of unique source name.
            Assert.True(sut.Count == eventsAsOrdered.Select(s => s.EventSource).Distinct().Count());

            //Handler count of the looked up item should equal the appearing count of the definition
            Assert.True(actual.Count() == definitionAppearingCount);

            //Generated handlers which includes picked up definition should be existed in looked up item.
            var handlersThatIncludesPickedUpDefinition =
                handlers.Where(h => h.Subscription.Events.Any(e => e.EventSource == definition.EventSource));

            Assert.True(handlersThatIncludesPickedUpDefinition.All(h => actual.Contains(h)));
        }




        [Fact]
        public void Lookup_VariousSourceDefinitionsWithUniqueSourceName_RegistrationsShouldBeUnique()
        {
            var source1WithoutSpecificEvent = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1");
            var source1WithSpecificEvent10 = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1" && d.EventId == 10);
            var source1WithSpecificEvent20 = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source1" && d.EventId == 20);
            var source2WithSpecificEvent30 = Mock.Of<ISourceDefinition>(d => d.EventSource == "Source2" && d.EventId == 30);

            IInternalSubscriptionHandler[] handlers = new IInternalSubscriptionHandler[]
            {
                Mock.Of<IInternalSubscriptionHandler>(h =>
                    h.Subscription == Mock.Of<ISubscription>(s =>
                        s.Id == Guid.NewGuid().ToString() &&
                        s.Events == new List<ISourceDefinition>
                        {
                            source1WithoutSpecificEvent,
                            source1WithSpecificEvent10

                        }.AsReadOnly())),

                Mock.Of<IInternalSubscriptionHandler>(h =>
                    h.Subscription == Mock.Of<ISubscription>(s =>
                        s.Id == Guid.NewGuid().ToString() &&
                        s.Events == new List<ISourceDefinition>
                        {
                            source1WithSpecificEvent10
                            //source1WithSpecificEvent20,
                            //source2WithSpecificEvent30

                        }.AsReadOnly()))
            };

            var sut = new EventListenerParameters(handlers).Lookup.Value;

            var def10 = new SourceDefinitionLookup(source1WithSpecificEvent10.EventSource, source1WithSpecificEvent10.EventId.Value, true);
            var def20 = new SourceDefinitionLookup(source1WithSpecificEvent20.EventSource, source1WithSpecificEvent20.EventId.Value, true);

            var def10r = sut[def10].ToList();
            var def20r = sut[def20].ToList();


            Assert.True(sut.Count == handlers.Count());

        }

    }


}
