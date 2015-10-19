using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncTest
{
    public class CustomSynchronizationContext : SynchronizationContext
    {
        private BlockingCollection<Action> jobs { get; set; }

        private CustomSynchronizationContext()
        {
            jobs = new BlockingCollection<Action>();
        }

        /// <summary>
        /// Instantiate a new 'CustomSynchronizationContext'
        /// and Replace current thread's SyncContext with that.
        /// </summary>
        /// <returns>The instance of CustomSynchronizationContext which Instantiated</returns>
        public static CustomSynchronizationContext SetCurrent()
        {
            var syncContext = new CustomSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(syncContext);

            return syncContext;
        }

        /// <summary>
        /// Dispatch a single job to the target thread and wait until the job ends.
        /// This method can be called in background thread.
        /// </summary>
        /// <param name="callback">The callback which you wants to dispatch</param>
        public override void Send(SendOrPostCallback callback, object state)
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            jobs.Add(() =>
            {
                callback.Invoke(state);

                ev.Set();
            });

            ev.WaitOne();
        }

        /// <summary>
        /// Dispatch a single job to the target thread and return immediately.
        /// This method can be called in background thread.
        /// </summary>
        /// <param name="callback">The callback which you wants to dispatch</param>
        public override void Post(SendOrPostCallback callback, object state)
        {
            jobs.Add(() =>
            {
                callback.Invoke(state);
            });
        }

        /// <summary>
        /// Execute a single job which dispatched.
        /// This method blocks the caller thread until the job dispatched.
        /// This method must be called in owner thread.
        /// </summary>
        public void DispatchOne()
        {
            var action = jobs.Take();
            action.Invoke();
        }
        /// <summary>
        /// Execute a single job if the job queue is not empty state.
        /// This method must be called in owner thread.
        /// </summary>
        public bool TryDispatchOne()
        {
            Action action = null;

            if (jobs.TryTake(out action) == false)
                return false;

            action.Invoke();
            return true;
        }
        /// <summary>
        /// Execute all jobs in the job queue.
        /// If the job queue is empty, return immediatly.
        /// This method must be called in owner thread.
        /// </summary>
        public void DispatchAll()
        {
            while (TryDispatchOne())
                ;
        }
    }
}
