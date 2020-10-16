using SimpleTracer.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace SimpleTracer.UnitTests.Internal
{
    public class MergedLookupTests
    {
        [Fact]
        public void Index_NotExistedKey_ShouldReturnEmpty()
        {
            var lookup1 = new List<KeyValuePair<string, int>>().ToLookup(x => x.Key, x => x.Value);
            var lookup2 = new List<KeyValuePair<string, int>>().ToLookup(x => x.Key, x => x.Value);

            var mergedLookup = new MergedLookup<string, int>(new ILookup<string, int>[] { lookup1, lookup2 });

            Assert.True(mergedLookup["some"].Count() == 0);

        }

        [Fact]
        public void Index_ExistedKeyInBothLookup_ShouldReturnMerged()
        {
            var lookup1 = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("from1to5", 1),
                new KeyValuePair<string, int>("from1to5", 2),
            }.ToLookup(x => x.Key, x => x.Value);

            var lookup2 = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("from1to5", 3),
                new KeyValuePair<string, int>("from1to5", 4),
                new KeyValuePair<string, int>("from1to5", 5),
            }.ToLookup(x => x.Key, x => x.Value);

            var mergedLookup = new MergedLookup<string, int>(new ILookup<string, int>[] { lookup1, lookup2 });

            Assert.True(mergedLookup["from1to5"].Count() == 5);
            Assert.True(mergedLookup["from1to5"].SequenceEqual(Enumerable.Range(1, 5)));
        }

        [Fact]
        public void Index_ExistedKeyWithSameValuesInBothLookup_ShouldReturnUnique()
        {
            var lookup1 = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("from1to5", 1),
                new KeyValuePair<string, int>("from1to5", 2),
                new KeyValuePair<string, int>("from1to5", 3),
                new KeyValuePair<string, int>("from1to5", 4),
                new KeyValuePair<string, int>("from1to5", 5),
            }.ToLookup(x => x.Key, x => x.Value);

            var lookup2 = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("from1to5", 1),
                new KeyValuePair<string, int>("from1to5", 2),
            }.ToLookup(x => x.Key, x => x.Value);

            var mergedLookup = new MergedLookup<string, int>(new ILookup<string, int>[] { lookup1, lookup2 });

            Assert.True(mergedLookup["from1to5"].Count() == 5);
            Assert.True(mergedLookup["from1to5"].SequenceEqual(Enumerable.Range(1, 5)));
        }
    }
}
