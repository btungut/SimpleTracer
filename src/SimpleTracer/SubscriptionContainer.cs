using SimpleTracer.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SimpleTracer
{
    public class SubscriptionContainer : ISubscriptionContainer
    {
        private Dictionary<string, ISubscriptionHandler> _handlers;
        private bool _isDisposed = false;

        internal SubscriptionContainer(List<ISubscriptionHandler> handlers)
        {
            _handlers = handlers.ToDictionary(k => k.Subscription.Id, k => k);
        }

        public void Start()
        {
            Foreach(h => h.Start());
        }

        public void Pause()
        {
            Foreach(h => h.SubscriptionTokenSource.Pause());
        }

        public void Resume()
        {
            Foreach(h => h.SubscriptionTokenSource.Resume());
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Foreach(h => h.Dispose());
                _handlers.Clear();
                _handlers = null;

                GC.SuppressFinalize(this);
                _isDisposed = true;
            }
        }

        private void Foreach(Action<ISubscriptionHandler> action)
        {
            foreach (ISubscriptionHandler handler in _handlers.Values)
                action(handler);
        }


        #region IDictionary exposing/implementation

        public IEnumerable<string> Keys => _handlers.Keys;
        public IEnumerable<ISubscriptionHandler> Values => _handlers.Values;
        public int Count => _handlers.Count;
        public ISubscriptionHandler this[string key] => _handlers[key];
        public bool ContainsKey(string key) => _handlers.ContainsKey(key);
        public bool TryGetValue(string key, out ISubscriptionHandler value) => _handlers.TryGetValue(key, out value);
        public IEnumerator<KeyValuePair<string, ISubscriptionHandler>> GetEnumerator() => _handlers.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _handlers.GetEnumerator();
        
        #endregion
    }
}
