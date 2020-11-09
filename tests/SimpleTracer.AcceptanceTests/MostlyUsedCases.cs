using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SimpleTracer.AcceptanceTests
{
    public class MostlyUsedCases : BaseAcceptanceTest
    {
        [Fact]
        public void SubscribeAllEventSources()
        {
            #region Build ISubscriptionContainer
            var builder = SubscriptionContainerBuilder
                .New()
                .WithSubscription(EventLevel.Informational, s => s
                    .WithExecution(e => e
                        .WithDelegate(OnEventNotification)
                        .WithInterval(TimeSpan.FromSeconds(1)))
                    .WithOptions());

            var container = builder.Build();
            container.Start();
            #endregion


            CallHttpClientMethods();
            CallAllocationMethods();
            GC.Collect();


            Assert.True(WaitUntil(() => ListenedEvents.Any(e => e.Registration.Source == "Microsoft-System-Net-Http"), TimeSpan.FromSeconds(3)));

            Assert.True(WaitUntil(() => ListenedEvents.Any(e => e.Name == "GCTriggered"), TimeSpan.FromSeconds(3)));
        }

        private void CallAllocationMethods()
        {
            new Task(async () =>
            {
                while (true)
                {
                    using (var stream = new MemoryStream())
                    using (var writer = new StreamWriter(stream))
                    {
                        for (int i = 0; i < 90000; i++)
                        {
                            writer.Write(i);
                        }

                        writer.Flush();

                        var bytes = stream.ToArray();
                        PinnedList.Add(bytes);
                        PinnedList.Add(Encoding.UTF8.GetString(bytes));
                    }

                    await Task.Delay(300);
                }
            }, TaskCreationOptions.LongRunning);
        }

        private void CallHttpClientMethods()
        {
            new Task(async () =>
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(10);

                while (true)
                {
                    var response = await client.GetAsync("https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf");

                    await Task.Delay(300);
                    var stream = response.Content.ReadAsStreamAsync();

                    await Task.Delay(300);
                    var @string = response.Content.ReadAsStringAsync();
                }

            }, TaskCreationOptions.LongRunning).Start();
        }
    }
}
