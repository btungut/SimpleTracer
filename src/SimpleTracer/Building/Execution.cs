using System;
using System.Threading.Tasks;

namespace SimpleTracer
{
    public sealed class Execution : IExecution, IExecutionConfigurator
    {
        /// <summary>
        /// <br>30 seconds</br>
        /// </summary>
        public const int DefaultIntervalInSeconds = 30;
        /// <summary>
        /// <br>1000</br>
        /// </summary>
        public const int DefaultMaxTake = 1000;

        public Func<IEventNotification, Task> Delegate { get; private set; }
        public TimeSpan Interval { get; private set; }
        public int MaxTake { get; private set; }

        internal Execution()
        {
            Interval = TimeSpan.FromSeconds(DefaultIntervalInSeconds);
            MaxTake = DefaultMaxTake;
        }

        public void Validate()
        {
            if (Delegate == null)
                throw new ArgumentNullException(nameof(Delegate),
                    $"Delegate of '{nameof(Execution)}' which is being executed on every occurence couldn't be null. " +
                    $"Use '{nameof(IExecutionConfigurator.WithDelegate)}' method to bind this.");

            if (MaxTake < 1)
                throw new ArgumentOutOfRangeException(nameof(MaxTake),
                    $"Item count which is being taken on every execution should be greater than zero. "+
                    $"Use '{nameof(IExecutionConfigurator.WithMaxTake)}' method to bind this.");

            if(Interval.TotalMilliseconds < 1000)
                throw new ArgumentOutOfRangeException(nameof(Interval),
                    $"Interval could not be lower than one second" +
                    $"Use '{nameof(IExecutionConfigurator.WithInterval)}' method to bind this.");
        }

        IExecutionConfigurator IExecutionConfigurator.WithInterval(TimeSpan interval)
        {
            Interval = interval;

            return this;
        }

        IExecutionConfigurator IExecutionConfigurator.WithMaxTake(int maximumEventPerExecution)
        {
            MaxTake = maximumEventPerExecution;

            return this;
        }

        IExecutionConfigurator IExecutionConfigurator.WithDelegate(Func<IEventNotification, Task> action)
        {
            Delegate = action;

            return this;
        }
    }
}
