using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SimpleTracer.UnitTests
{
    public static class Common
    {
        public static bool WaitUntil(Func<bool> condition, TimeSpan timeout)
        {
            var started = DateTime.Now;
            while (!condition())
            {
                if ((DateTime.Now - started) >= timeout)
                {
                    Console.WriteLine($"Timeout details : {DateTime.Now} - {started} >= {timeout}");
                    return false;
                }

                Thread.Sleep(100);
            }

            return true;
        }
    }
}
