using LogLite.Core.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace LogLite.Core.Sinks
{
	public class FileSink : Sink
	{
		private readonly string _rootDirectory;
		private readonly string _logFileDirectory;

		private readonly FileInfo _logFile;

		public FileSink()
		{
			_rootDirectory = Path.GetPathRoot(Environment.SystemDirectory)!;
			_logFileDirectory = Path.Combine(_rootDirectory, "/Logs");

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
		}

		public override void Write(string statement)
		{
			lock (_lock)
			{
				_logQueue.Enqueue(statement);

				if (_logQueue.Count == 1)
				{
					_runQueue.Enqueue(Flush);
				}
			}
		}

		public override void Dispose()
		{
			_runQueue.Enqueue(Flush);
			_runQueue.Dispose();
		}

		protected override void Flush()
		{
			Thread.Sleep(FlushTimeoutMilliseconds);

			using FileStream fileStream = _logFile.Open(FileMode.Append);
			using StreamWriter streamWriter = new StreamWriter(fileStream);

			while (true)
			{
				string? statement;

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
	}
}
