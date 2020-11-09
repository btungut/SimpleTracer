using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTracer.UnitTests
{
    public static class Number
    {
        public static IEnumerable<object[]> Positive => Generate(1, int.MaxValue);
        public static IEnumerable<object[]> Negative => Generate(int.MinValue, -1);

        public static IEnumerable<object[]> Generate(int min, int max) => GenerateWithCount(min, max, 10);

        public static IEnumerable<object[]> GenerateWithCount(int min, int max, int count)
        {
            Random random = new Random();
            return Enumerable.Range(1, count).Select(_ => new object[] { random.Next(min, max) });
        }
    }
}
