using SimpleTracer.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SimpleTracer.UnitTests.Internal
{
    public class SafeAccessibleObjectTests
    {
        [Fact]
        public void SafeAccessAction_MultiThreadCalls_SubsequentCallsShouldBeWaited()
        {
            var list = new List<string>();
            var safeList = new LockBasedThreadSafety<List<string>>(list);

            bool isTaken = true;
            var task = Task.Factory.StartNew(() =>
            {
                safeList.SafeAccess(o =>
                {
                    o.Add("First");
                    while (isTaken)
                    {
                        Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                });
            });

            Assert.True(Common.WaitUntil(() => list.Count == 1, TimeSpan.FromSeconds(3)));

            isTaken = false;
            safeList.SafeAccess(o => o.Add("Second"));

            Assert.True(list.Count == 2);
        }

        [Fact]
        public void SafeAccessFunc_MultiThreadCalls_SubsequentCallsShouldBeWaited()
        {
            var list = new List<string>();
            var safeList = new LockBasedThreadSafety<List<string>>(list);

            bool isTaken = true;
            var task = Task.Factory.StartNew(() =>
            {
                safeList.SafeAccess(o =>
                {
                    o.Add("First");
                    while (isTaken)
                    {
                        Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();
                    }

                    return true;
                });
            });

            Assert.True(Common.WaitUntil(() => list.Count == 1, TimeSpan.FromSeconds(3)));

            isTaken = false;
            safeList.SafeAccess(o => o.Add("Second"));

            Assert.True(list.Count == 2);
        }
    }
}
