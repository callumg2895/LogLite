using LogLite.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LogLite.Sinks.File
{
	public class FileLoggerSink : ILoggerSink
	{
		private readonly string _rootDirectory;
		private readonly string _logFileDirectory;

		private readonly List<string> _logQueue;

		private readonly object _logQueueLock;
		private readonly object _writeLock;

		public FileLoggerSink()
		{
			_rootDirectory = Path.GetPathRoot(Environment.SystemDirectory);
			_logFileDirectory = Path.Combine(_rootDirectory, "/Logs");

			_logQueue = new List<string>();

			_logQueueLock = new object();
			_writeLock = new object();
		}

		public void Write(string statement)
		{
			lock (_logQueueLock)
			{
				_logQueue.Add(statement);
			}

			// Flush to disk asynchronously so we don't disrupt the main thread
			Task.Run(Flush);
		}
		public void Flush()
		{
			StringBuilder stringBuilder = new StringBuilder();
			List<string> statements;
			string logFilePath = Path.Combine(_logFileDirectory, "logFile.log");

			lock (_logQueueLock)
			{
				statements = new List<string>(_logQueue);

				_logQueue.Clear();
			}

			foreach (string item in statements)
			{
				stringBuilder.AppendLine(item);
			}

			lock (_writeLock)
			{
				if (!Directory.Exists(_logFileDirectory))
				{
					Directory.CreateDirectory(_logFileDirectory);
				}

				if (!System.IO.File.Exists(logFilePath))
				{
					System.IO.File.Create(logFilePath);
				}

				using FileStream stream = System.IO.File.Open(logFilePath, FileMode.Open);

				stream.Write(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
			}
		}

	}
}
