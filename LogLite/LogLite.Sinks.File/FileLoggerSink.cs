using LogLite.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace LogLite.Sinks.File
{
	public class FileLoggerSink : ILoggerSink
	{
		private const int FlushTimeoutMilliseconds = 1000;

		private readonly string _rootDirectory;
		private readonly string _logFileDirectory;

		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly Queue<string> _logQueue;
		private readonly Thread _thread;
		private readonly FileInfo _logFile;

		private readonly object _lock;

		public FileLoggerSink()
		{
			ThreadStart threadStart = new ThreadStart(ProcessQueue);

			_rootDirectory = Path.GetPathRoot(Environment.SystemDirectory);
			_logFileDirectory = Path.Combine(_rootDirectory, "/Logs");

			_cancellationTokenSource = new CancellationTokenSource();
			_logQueue = new Queue<string>();
			_thread = new Thread(threadStart);

			_lock = new object();

			if (!Directory.Exists(_logFileDirectory))
			{
				Directory.CreateDirectory(_logFileDirectory);
			}

			_logFile = new FileInfo(Path.Combine(_logFileDirectory, "logFile.log"));

			if (_logFile.Exists)
			{
				_logFile.Delete();
			}

			using FileStream fileStream = _logFile.Create();

			_thread.Start();
		}

		public void Write(string statement)
		{
			lock (_lock)
			{
				_logQueue.Enqueue(statement);

				Monitor.Pulse(_lock);
			}
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();

			lock (_lock)
			{
				Monitor.Pulse(_lock);
			}

			_thread.Join();
		}

		private void ProcessQueue()
		{
			do
			{
				FlushQueue();
				AwaitNotification();
			}
			while (!_cancellationTokenSource.Token.IsCancellationRequested || _logQueue.Count > 0);
		}

		private void FlushQueue()
		{
			using FileStream fileStream = _logFile.Open(FileMode.Append);
			using StreamWriter streamWriter = new StreamWriter(fileStream);

			while (true)
			{
				string statement;

				lock (_lock)
				{
					if (!_logQueue.TryDequeue(out statement))
					{
						break;
					}
				}

				streamWriter.WriteLine(statement);
			}
		}

		private void AwaitNotification()
		{
			lock (_lock)
			{
				if (_logQueue.Count == 0)
				{
					Monitor.Wait(_lock, FlushTimeoutMilliseconds);
				}
			}
		}
	}
}
