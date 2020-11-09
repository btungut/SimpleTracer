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
    public class ExecutionTests
    {
        [Fact]
        public void Validate_NewInstance_ThrowsException()
        {
            var execution = new Execution();

            Assert.Throws<ArgumentNullException>(nameof(Execution.Delegate), execution.Validate);
        }

        [Theory]
        [InlineData(0)]
        [MemberData(nameof(Number.Negative), MemberType = typeof(Number))]
        public void Validate_LowerThanOneTake_ThrowsException(int count)
        {
            var execution = new Execution();
            var configurator = (IExecutionConfigurator)execution;

            configurator.WithDelegate(_ => Task.CompletedTask);
            configurator.WithMaxTake(count);

            Assert.Throws<ArgumentOutOfRangeException>(nameof(Execution.MaxTake), execution.Validate);
        }

        [Theory]
        [MemberData(nameof(Number.Generate),new object[] { 0, 999 }, MemberType = typeof(Number))]
        public void Validate_LessThanOneSecondDelay_ThrowsException(int miliseconds)
        {
            var execution = new Execution();
            var configurator = (IExecutionConfigurator)execution;

            configurator.WithDelegate(_ => Task.CompletedTask);
            configurator.WithMaxTake(10);
            configurator.WithInterval(TimeSpan.FromMilliseconds(miliseconds));

            Assert.Throws<ArgumentOutOfRangeException>(nameof(Execution.Interval), execution.Validate);
        }

        [Theory]
        [MemberData(nameof(Number.Generate), new object[] { 1000, int.MaxValue }, MemberType = typeof(Number))]
        public void Validate_WithValidParameters_ThrowsNoException(int miliseconds)
        {
            var execution = new Execution();
            var configurator = (IExecutionConfigurator)execution;

            configurator.WithDelegate(_ => Task.CompletedTask);
            configurator.WithMaxTake(10);
            configurator.WithInterval(TimeSpan.FromMilliseconds(miliseconds));

            execution.Validate();
        }

        [Fact]
        public void GetDelegate_NewInstance_ReturnsNull()
        {
            var execution = new Execution();

            Assert.Null(execution.Delegate);
        }

        [Fact]
        public void GetInterval_NewInstance_ReturnsDefaultValue()
        {
            var execution = new Execution();

            Assert.Equal(execution.Interval, TimeSpan.FromSeconds(Execution.DefaultIntervalInSeconds));
        }

        [Fact]
        public void GetMaxItemPerOccurence_NewInstance_ReturnsDefaultValue()
        {
            var execution = new Execution();

            Assert.Equal(execution.MaxTake, Execution.DefaultMaxTake);
        }

        [Theory]
        [MemberData(nameof(Number.Positive), MemberType = typeof(Number))]
        public void GetInterval_DelayWithValue_ReturnsGivenValue(int miliseconds)
        {
            var execution = new Execution();
            var configurator = (IExecutionConfigurator)execution;

            configurator.WithInterval(TimeSpan.FromMilliseconds(miliseconds));

            Assert.Equal(execution.Interval, TimeSpan.FromMilliseconds(miliseconds));
        }

        [Theory]
        [MemberData(nameof(Number.Positive), MemberType = typeof(Number))]
        public void GetMaxItemPerOccurence_TakeWithValue_ReturnsGivenValue(int count)
        {
            var execution = new Execution();
            var configurator = (IExecutionConfigurator)execution;

            configurator.WithMaxTake(count);

            Assert.Equal(execution.MaxTake, count);
        }

        [Fact]
        public async Task InvokeDelegate_WithValidNotification_ReturnsSame()
        {
            var expected = Mock.Of<IEventNotification>(ctx1 =>
                ctx1.LastExecution == DateTime.Now &&
                ctx1.RemainingEventCount == 100 &&
                ctx1.Events == Enumerable.Repeat(Mock.Of<IEventEntry>(), 10).ToList().AsReadOnly()
            );

            var execution = new Execution();
            var configurator = (IExecutionConfigurator)execution;

            configurator.WithDelegate(notification =>
            {
                return Task.FromResult(notification);
            });

            var actual = await (Task<IEventNotification>)execution.Delegate(expected);
            Assert.Equal(expected.Events, actual.Events);
            Assert.Equal(expected.LastExecution, actual.LastExecution);
            Assert.Equal(expected.RemainingEventCount, actual.RemainingEventCount);
        }
    }


}
