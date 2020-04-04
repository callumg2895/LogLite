﻿using LogLite.Core;
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

		private readonly object _logQueueLock;
		private readonly object _writeLock;

		public FileLoggerSink()
		{
			_rootDirectory = Path.GetPathRoot(Environment.SystemDirectory);
			_logFileDirectory = Path.Combine(_rootDirectory, "/Logs");

			_logQueue = new List<string>();
			_runQueue = new RunQueue();

			_logQueueLock = new object();
		}

		public void Write(string statement)
		{
			int queueLength = 0;

			lock (_logQueueLock)
			{
				_logQueue.Add(statement);
				queueLength = _logQueue.Count;
			}

			// Flush to disk asynchronously so we don't disrupt the main thread
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

				string logFilePath = Path.Combine(_logFileDirectory, "logFile.log");

				StringBuilder stringBuilder = new StringBuilder();
				FileInfo file = new FileInfo(logFilePath);
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

				if (!Directory.Exists(_logFileDirectory))
				{
					Directory.CreateDirectory(_logFileDirectory);
				}

				using FileStream stream = file.Exists
					? file.Open(FileMode.Open)
					: file.Create();

				stream.Write(Encoding.UTF8.GetBytes(stringBuilder.ToString()));			
			});
		}

		public void Dispose()
		{
			Flush();
			_runQueue.Dispose();
		}
	}
}
