using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleTracer
{
    /// <summary>
    /// Allows a <see cref="ISubscriptionHandler"/> to get it stopped, resumed and paused
    /// </summary>
    public sealed class SubscriptionTokenSource : IDisposable
    {

        private CancellationTokenSource _cts;
        private TimeSpan _pauseDelay;
        private Status _status;
        private bool _isDisposed;

        internal SubscriptionTokenSource()
        {
            _cts = new CancellationTokenSource();
            _pauseDelay = default;
            _status = Status.NotPaused;
        }

        public bool IsCancelled => _cts.IsCancellationRequested;
        public bool IsPaused => _status != Status.NotPaused;
        /// <summary>
        /// Exposes CancellationToken of Task which is owned by Subscription.
        /// </summary>
        public CancellationToken CancellationToken => _cts.Token;

        internal async Task PauseIfRequested()
        {
            EnsureItIsInValidState();

            if (_status == Status.PausedWithDelay)
            {
                await Task.Delay(_pauseDelay).ConfigureAwait(false);
                Resume();
            }
            else if(_status == Status.PausedWithoutDelay)
            {
                int iterationCount = 0;
                const int BasePauseDelayInMilliseconds = 100;

                while (IsPaused)
                {
                    iterationCount++;
                    await Task.Delay(BasePauseDelayInMilliseconds * iterationCount).ConfigureAwait(false);
                }

                Resume();
            }
        }

        private void EnsureItIsInValidState()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SubscriptionTokenSource));
        }

        /// <summary>
        /// Sends Cancellation request which could not be irreversible.
        /// </summary>
        public void Cancel()
        {
            EnsureItIsInValidState();
            _cts.Cancel();
        }

        /// <summary>
        /// Sends Pause request. <see cref="Resume"/> method could be called to get it unpaused again.
        /// </summary>
        public void Pause()
        {
            EnsureItIsInValidState();
            _status = Status.PausedWithoutDelay;
            _pauseDelay = default;
        }

        /// <summary>
        /// Sends Pause request with specific delay. After <paramref name="delay"/> is up, Subscription will be continued to work.
        /// </summary>
        /// <param name="delay"></param>
        public void Pause(TimeSpan delay)
        {
            EnsureItIsInValidState();
            _status = Status.PausedWithDelay;
            _pauseDelay = delay;
        }

        /// <summary>
        /// Sends Resume request for paused Subscription.<br></br>
        /// If Subscription was not paused, no action will be taken.
        /// </summary>
        public void Resume()
        {
            EnsureItIsInValidState();
            _status = Status.NotPaused;
            _pauseDelay = default;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _cts.Cancel();
                _cts.Dispose();
                _isDisposed = true;

                GC.SuppressFinalize(this);
            }
        }

        enum Status
        {
            NotPaused,
            PausedWithDelay,
            PausedWithoutDelay
        }
    }


}
