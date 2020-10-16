using System;
using System.Threading;

namespace SimpleTracer.Internal
{
    internal class LockBasedThreadSafety<TObject>
    {
        private TObject _obj;

        public LockBasedThreadSafety(TObject obj)
        {
            _obj = obj;
        }

        public void SafeAccess(Action<TObject> action)
        {
            lock (_obj)
            {
                action(_obj);
            }
        }

        public TReturn SafeAccess<TReturn>(Func<TObject, TReturn> action)
        {
            lock (_obj)
            {
                return action(_obj);
            }
        }
    }

    internal class ReadWriteLockThreadSafety<TObject>
    {
        private TObject _obj;
        private ReaderWriterLockSlim _slim;

        public ReadWriteLockThreadSafety(TObject obj)
        {
            _obj = obj;
            _slim = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        public void Write(Action<TObject> action)
        {
            _slim.EnterWriteLock();
            try
            {
                action(_obj);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                _slim.ExitWriteLock();
            }
        }

        public TReturn Read<TReturn>(Func<TObject, TReturn> action)
        {
            _slim.EnterReadLock();
            try
            {
                return action(_obj);
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                _slim.ExitReadLock();
            }
        }
    }

}
