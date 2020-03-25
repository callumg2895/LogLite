using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogLite.Core.Util
{
	internal class RunQueue : IDisposable
	{
		private readonly Queue<Action> _actions;
		private readonly Thread _thread;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly object _lock;

		public RunQueue()
		{
			ThreadStart threadStart = new ThreadStart(ProcessActions);

			_actions = new Queue<Action>();
			_thread = new Thread(threadStart);
			_cancellationTokenSource = new CancellationTokenSource();
			_lock = new object();

			_thread.Start();
		}

		public void Enqueue(Action action)
		{
			_actions.Enqueue(action);

			lock (_lock)
			{
				/*
				 * We've just added a new item to the queue, but if it was previously empty the thread that dequeues actions
				 * will be stuck waiting indefinitely. We should notify this thread that it's time to wake up and do work.
				 */

				Monitor.Pulse(_lock);
			}
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();

			lock (_lock)
			{
				/*
				 * We need to stop the thread, but if the run queue is currently waiting it needs to be notified.
				 */

				Monitor.Pulse(_lock);
			}

			_thread.Join();
		}

		private void ProcessActions()
		{
			while (!_cancellationTokenSource.Token.IsCancellationRequested || _actions.Count > 0)
			{
				if (_actions.Count == 0)
				{
					/*
					 * At this point, the run queue is currently empty. We now aquire the lock, release it, and wait to be 
					 * notified about the next time we aquire the lock. This allows us to wait without specifying a timeout 
					 * interval.
					 */

					lock (_lock)
					{
						Monitor.Wait(_lock);
					}

					continue;
				}

				Action action = _actions.Peek();

				action.Invoke();

				_actions.Dequeue();
			}
		}
	}
}
