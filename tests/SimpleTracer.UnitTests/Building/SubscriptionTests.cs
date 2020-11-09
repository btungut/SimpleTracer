using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleTracer.UnitTests.Building
{
    public class SubscriptionTests
    {
        [Fact]
        public void Validate_WithZeroEvent_ThrowsException()
        {
            var subscription = new Subscription();
            var configurator = (ISubscriptionConfigurator)subscription;

            Assert.Throws<ArgumentNullException>(nameof(Subscription.Events), subscription.Validate);
        }

        [Fact]
        public void GetExecution_CreatedByWithExecutionMethod_ObjectsShouldBeSame()
        {
            var subscription = new Subscription();
            var configurator = (ISubscriptionConfigurator)subscription;

            IExecutionConfigurator execution = null;
            configurator.WithExecution(e => { execution = e; });

            Assert.Same(execution, subscription.Execution);
        }

        [Fact]
        public void GetOptions_CreatedByWithOptionsMethod_ObjectsShouldBeSame()
        {
            var subscription = new Subscription();
            var configurator = (ISubscriptionConfigurator)subscription;

            IOptionsConfigurator options = null;
            configurator.WithOptions(e => { options = e; });

            Assert.Same(options, subscription.Options);
        }

        [Fact]
        public void GetId_AfterWithIdMethod_ValuesShouldBeEqual()
        {
            var subscription = new Subscription();
            var configurator = (ISubscriptionConfigurator)subscription;

            string id = DateTime.Now.ToString();
            configurator.WithId(id);

            Assert.Equal(id, subscription.Id);
        }


        [Fact]
        public void Validate_WithNullExecution_ThrowsException()
        {
            var subscription = new Subscription();
            var configurator = (ISubscriptionConfigurator)subscription;

            configurator.WithEvents(Mock.Of<ISourceDefinition>());

            Assert.Throws<ArgumentNullException>(nameof(Subscription.Execution), subscription.Validate);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Validate_WithNullAndEmptyId_ThrowsException(string id)
        {
            var subscription = new Subscription();
            var configurator = (ISubscriptionConfigurator)subscription;

            configurator.WithId(id);

            Assert.Throws<ArgumentNullException>(nameof(Subscription.Id), subscription.Validate);
        }

        [Fact]
        public void Validate_WithNullOptions_ThrowsException()
        {
            var subscription = new Subscription();
            var configurator = (ISubscriptionConfigurator)subscription;

            configurator.WithEvents(Mock.Of<ISourceDefinition>());
            configurator.WithExecution(Mock.Of<IExecution>());

            Assert.Throws<ArgumentNullException>(nameof(Subscription.Options), subscription.Validate);
        }

        [Fact]
        public void Validate_WithValidParameters_ThrowsNoException()
        {
            var execution = new Mock<IExecution>();
            execution.Setup(e => e.Validate()).Verifiable();

            var options = new Mock<IOptions>();
            options.Setup(e => e.Validate()).Verifiable();

            var subscription = new Subscription();
            var configurator = (ISubscriptionConfigurator)subscription;

            configurator.WithEvents(Mock.Of<ISourceDefinition>());
            configurator.WithExecution(execution.Object);
            configurator.WithOptions(options.Object);

            subscription.Validate();
        }
    }

}
