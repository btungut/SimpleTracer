using System;
using System.Threading.Tasks;

namespace SimpleTracer
{
    /// <summary>
    /// Provides the configurations about execution and invocation like which method is being invoked, how and how often the invocation should be performed.
    /// <br>Default of <see cref="IExecution.Interval"/> : <inheritdoc cref="Execution.DefaultIntervalInSeconds"/><br></br>
    /// Default of <see cref="IExecution.MaxTake"/> : <inheritdoc cref="Execution.DefaultMaxTake"/></br>
    /// </summary>
    public interface IExecution : IValidatable
    {
        /// <summary>
        /// The delegate that is being invoked and serves <see cref="IEventNotification"/> for every time period specified by <see cref="Interval"/><br></br>
        /// By default, invocation is performed by a <see cref="Task"/> without <b>capturing the curent context</b> to use any of the available threads and to prevent dedication of any specific thread.
        /// </summary>
        Func<IEventNotification, Task> Delegate { get; }

        /// <summary>
        /// Indicates the invocation frequency of <see cref="Delegate"/><br></br>
        /// It should be tuned correctly according the performance, latency, avg took time of <see cref="Delegate"/>
        /// </summary>
        TimeSpan Interval { get; }

        /// <summary>
        /// Indicates that the <see cref="IEventNotification.Events"/> count could be at most per <see cref="Delegate"/> invocation.
        /// </summary>
        int MaxTake { get; }
    }

    public interface IExecutionConfigurator
    {
        /// <summary>
        /// Sets the <see cref="IExecution.Interval"/> with specified value.<para></para>
        /// <inheritdoc cref="IExecution.Interval"/>
        /// </summary>
        /// <param name="interval"></param>
        /// <returns>Returns the same builder object</returns>
        IExecutionConfigurator WithInterval(TimeSpan interval);

        /// <summary>
        /// Sets the <see cref="IExecution.MaxTake"/> with specified value.<para></para>
        /// <inheritdoc cref="IExecution.MaxTake"/>
        /// </summary>
        /// <param name="maximumEventPerExecution"></param>
        /// <returns>Returns the same builder object</returns>
        IExecutionConfigurator WithMaxTake(int maximumEventPerExecution);

        /// <summary>
        /// Sets the <see cref="IExecution.Delegate"/> with specified value.<para></para>
        /// <inheritdoc cref="IExecution.Delegate"/>
        /// </summary>
        /// <param name="action">The delegate which serves IEventNotification and needs to return Task (or using async)</param>
        /// <returns>Returns the same builder object</returns>
        IExecutionConfigurator WithDelegate(Func<IEventNotification, Task> action);
    }
}