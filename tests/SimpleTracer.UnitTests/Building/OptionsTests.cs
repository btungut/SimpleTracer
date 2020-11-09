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
    public class OptionsTests
    {
        [Fact]
        public void GetMaxEventEntryCount_NewInstance_ReturnsDefaultValue()
        {
            var options = new Options();

            Assert.Equal(Options.DefaultMaxEventEntryCount, options.QueueCapacity);
        }

        [Theory]
        [MemberData(nameof(Number.Positive), MemberType = typeof(Number))]
        public void GetMaxEventEntryCount_WithValue_ReturnsGivenValue(int expected)
        {
            var options = new Options();
            var configurator = (IOptionsConfigurator)options;

            configurator.WithQueueCapacity(expected);

            Assert.Equal(expected, options.QueueCapacity);
        }

        [Theory]
        [InlineData(0)]
        [MemberData(nameof(Number.Negative), MemberType = typeof(Number))]
        public void Validate_LowerThanOneEntryCount_ThrowsException(int entryCount)
        {
            var options = new Options();
            var configurator = (IOptionsConfigurator)options;

            configurator.WithQueueCapacity(entryCount);

            Assert.Throws<ArgumentOutOfRangeException>(nameof(options.QueueCapacity), options.Validate);
        }

        [Theory]
        [MemberData(nameof(Number.Positive), MemberType = typeof(Number))]
        public void Validate_WithValidParameters_ThrowsNoException(int entryCount)
        {
            var options = new Options();
            var configurator = (IOptionsConfigurator)options;

            configurator.WithQueueCapacity(entryCount);
            
            options.Validate();
        }
    }

}
