using LogLite.Core;
using LogLite.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogLite.Sinks.File
{
	public class FileLoggerSink : ILoggerSink
	{
		private const int FlushDelayMilliseconds = 10;

		private readonly string _rootDirectory;
		private readonly string _logFileDirectory;

		private readonly List<string> _logQueue;
		private readonly RunQueue _runQueue;
		private readonly FileInfo _logFile;

		private readonly object _logQueueLock;

		public FileLoggerSink()
		{
			_rootDirectory = Path.GetPathRoot(Environment.SystemDirectory);
			_logFileDirectory = Path.Combine(_rootDirectory, "/Logs");

			_logQueue = new List<string>();
			_runQueue = new RunQueue();

			_logQueueLock = new object();

			if (!Directory.Exists(_logFileDirectory))
			{
				Directory.CreateDirectory(_logFileDirectory);
			}

			_logFile = new FileInfo(Path.Combine(_logFileDirectory, "logFile.log"));

			if (!_logFile.Exists)
			{
				_logFile.Create();
			}
		}

		public void Write(string statement)
		{
			int queueLength = 0;

			lock (_logQueueLock)
			{
				_logQueue.Add(statement);
				queueLength = _logQueue.Count;
			}

			if (queueLength == 1)
			{
				Flush();
			}	
		}
		public void Flush()
		{
			_runQueue.Enqueue(() =>
			{
				Thread.Sleep(FlushDelayMilliseconds);

				StringBuilder stringBuilder = new StringBuilder();

				do
				{
					List<string> statements;

					lock (_logQueueLock)
					{
						statements = new List<string>(_logQueue);

						_logQueue.Clear();
					}

					foreach (string item in statements)
					{
						stringBuilder.AppendLine(item);
					}
				}
				while (_logQueue.Count > 0);

				FileStream fileStream = _logFile.Open(FileMode.Open);

				fileStream.Write(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
				fileStream.Dispose();

			});
		}

		public void Dispose()
		{
			_runQueue.Dispose();
		}
	}
}
