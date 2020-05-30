using System;
using System.Collections.Generic;
using System.Threading;

namespace LogLite.Core.Util
{
	internal class RunQueue : IDisposable
	{
		private const int WaitTimeoutMilliseconds = 5000;

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

			/*
			 * We've just added a new item to the queue, but if it was previously empty the thread that dequeues actions
			 * will be stuck waiting indefinitely. We should notify this thread that it's time to wake up and do work.
			 */

			NotifyQueue();
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();

			// We need to stop the thread, but if the run queue is currently waiting it needs to be notified.

			NotifyQueue();
			_thread.Join();
		}

		private void NotifyQueue()
		{
			lock (_lock)
			{
				Monitor.Pulse(_lock);
			}
		}

		private void ProcessActions()
		{
			// Always process actions at least once.

			do
			{
				ProcessNextAction();
			}
			while (!_cancellationTokenSource.Token.IsCancellationRequested || _actions.Count > 0);
		}

		private void ProcessNextAction()
		{
			if (_actions.Count > 0)
			{
				_actions.Peek()?.Invoke();
				_actions.Dequeue();

				return;
			}

			/*
			 * At this point, the run queue is currently empty. We now aquire the lock, release it, and wait to be 
			 * notified about the next time we aquire the lock. This allows us to wait without specifying a timeout 
			 * interval.
			 */

			lock (_lock)
			{
				if (_cancellationTokenSource.IsCancellationRequested)
				{
					return;
				}

				Monitor.Wait(_lock, WaitTimeoutMilliseconds);
			}
		}
	}
}
