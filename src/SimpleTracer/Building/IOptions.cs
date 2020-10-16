namespace SimpleTracer
{
    /// <summary>
    /// Provides various configuration values which has predefined default values but also be able to get configured explicitly by using <see cref="IOptionsConfigurator"/> methods.
    /// <br>Default of <see cref="QueueCapacity"/> : <inheritdoc cref="Options.DefaultMaxEventEntryCount"/></br>
    /// </summary>
    public interface IOptions : IValidatable
    {
        /// <summary>
        /// The capacity of internal queue which contains subscribed/listened events.
        /// <br>If this value is reached, no more event would be enqueued to prevent memory leak.
        /// It should be tuned correctly according to speed of consuming and producing.</br>
        /// </summary>
        int QueueCapacity { get; }
    }

    public interface IOptionsConfigurator
    {
        /// <summary>
        /// Sets the QueueCapacity with specified value.
        /// <br><inheritdoc cref="IOptions.QueueCapacity"/></br>
        /// </summary>
        /// <param name="value">The value which is being used to limit internal queue.</param>
        /// <returns>Returns the same builder object</returns>
        IOptionsConfigurator WithQueueCapacity(int value);
    }
}