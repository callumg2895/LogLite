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
		private const int FlushIntervalMilliseconds = 100;

		private readonly string _rootDirectory;
		private readonly string _logFileDirectory;

		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly Queue<string> _logQueue;
		private readonly Thread _thread;
		private readonly FileInfo _logFile;

		private readonly object _lock;

		public FileLoggerSink()
		{
			ThreadStart threadStart = new ThreadStart(Flush);

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
			_logQueue.Enqueue(statement);

			lock (_lock)
			{
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

			// We need to stop the thread, but if the run queue is currently waiting it needs to be notified.

			_thread.Join();
		}

		private void Flush()
		{
			do
			{
				if (FlushIteration())
				{
					continue;
				}

				lock (_lock)
				{
					Monitor.Wait(_lock, FlushIntervalMilliseconds);
				}			
			}
			while (!_cancellationTokenSource.Token.IsCancellationRequested || _logQueue.Count > 0);
		}

		private bool FlushIteration()
		{
			Thread.Sleep(FlushIntervalMilliseconds);

			StringBuilder stringBuilder = new StringBuilder();
			int writeCount = 0;

			while (_logQueue.Count > 0)
			{
				stringBuilder.AppendLine(_logQueue.Dequeue());
				writeCount++;
			}

			using (FileStream fileStream = _logFile.Open(FileMode.Open))
			{
				fileStream.Write(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
			};

			return writeCount > 0;
		}
	}
}
