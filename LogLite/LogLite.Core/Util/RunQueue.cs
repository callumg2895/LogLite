using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LogLite.Core.Util
{
	internal class RunQueue : IDisposable
	{
		private const int IdleTimeoutMilliseconds = 100;

		private readonly Queue<Action> _actions;
		private readonly Thread _thread;
		private readonly CancellationTokenSource _cancellationTokenSource;

		public RunQueue()
		{
			ThreadStart threadStart = new ThreadStart(ProcessActions);

			_actions = new Queue<Action>();
			_thread = new Thread(threadStart);
			_cancellationTokenSource = new CancellationTokenSource();

			_thread.Start();
		}

		public void Enqueue(Action action)
		{
			_actions.Enqueue(action);
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_thread.Join();
		}

		private void ProcessActions()
		{
			while (!_cancellationTokenSource.Token.IsCancellationRequested || _actions.Count > 0)
			{
				if (_actions.Count == 0)
				{
					Thread.Sleep(IdleTimeoutMilliseconds);
					continue;
				}

				Action action = _actions.Peek();

				action.Invoke();

				_actions.Dequeue();
			}
		}
	}
}
