using System;

namespace SimpleTracer
{
    public sealed class Options : IOptions, IOptionsConfigurator
    {
        /// <summary>
        /// <br>1000</br>
        /// </summary>
        public const int DefaultMaxEventEntryCount = 1000;

        public int QueueCapacity { get; private set; }

        internal Options()
        {
            QueueCapacity = DefaultMaxEventEntryCount;
        }

        
        IOptionsConfigurator IOptionsConfigurator.WithQueueCapacity(int value)
        {
            QueueCapacity = value;

            return this;
        }

        public void Validate()
        {
            if (QueueCapacity < 1)
                throw new ArgumentOutOfRangeException(nameof(QueueCapacity),
                     $"Value can't be less than 1. Use '{nameof(IOptionsConfigurator.WithQueueCapacity)}' method to bind this in correct range.");
        }
    }
}
