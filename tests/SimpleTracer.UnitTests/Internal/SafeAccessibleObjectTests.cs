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
        public async Task SafeAccessAction_MultiThreadCalls_SubsequentCallsShouldBeWaited()
        {
            List<string> list = new List<string>();
            var safeList = new LockBasedThreadSafety<List<string>>(list);

            int delay = 5000;
            string toBeAdded = "ParallelTask";
            var task = Task.Factory.StartNew(() =>
            {
                safeList.SafeAccess(o =>
                {
                    o.Add(toBeAdded);
                    Task.Delay((int)(delay * 1.1)).ConfigureAwait(false).GetAwaiter().GetResult();
                });
            });

            await Task.Delay(100); //Ensure that the task below is being invoked.

            var watch = Stopwatch.StartNew();
            safeList.SafeAccess(o => o.Add("MainThread"));
            watch.Stop();

            Assert.True(list.Count == 2);
            Assert.True(watch.ElapsedMilliseconds >= delay);
        }

        [Fact]
        public async Task SafeAccessFunc_MultiThreadCalls_SubsequentCallsShouldBeWaited()
        {
            List<string> list = new List<string>();
            var safeList = new LockBasedThreadSafety<List<string>>(list);

            int delay = 5000;
            string toBeAdded = "ParallelTask";
            var task = Task.Factory.StartNew(() =>
            {
                safeList.SafeAccess(o =>
                {
                    o.Add(toBeAdded);
                    Task.Delay((int)(delay * 1.1)).ConfigureAwait(false).GetAwaiter().GetResult();
                    return true;
                });
            });

            await Task.Delay(100); //Ensure that the task below is being invoked.

            var watch = Stopwatch.StartNew();
            safeList.SafeAccess(o => o.Add("MainThread"));
            watch.Stop();

            Assert.True(list.Count == 2);
            Assert.True(watch.ElapsedMilliseconds >= delay);
        }
    }
}
