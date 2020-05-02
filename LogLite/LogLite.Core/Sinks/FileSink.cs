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

		private string? _logFileDirectory;
		private FileInfo? _logFile;

		public FileSink()
		{
			_rootDirectory = Path.GetPathRoot(Environment.SystemDirectory)!;

			ConfigureDirectoryName("/logs");
			ConfigureFileName("logFile");
		}

		#region Configuration

		public FileSink ConfigureDirectoryName(string directoryName)
		{
			_logFileDirectory = Path.Combine(_rootDirectory, directoryName);

			if (!Directory.Exists(_logFileDirectory))
			{
				Directory.CreateDirectory(_logFileDirectory);
			}

			return this;
		}

		public FileSink ConfigureFileName(string fileName)
		{
			if (_logFile != null && _logFile.Exists)
			{
				_logFile.Delete();
			}

			_logFile = new FileInfo(Path.Combine(_logFileDirectory!, $"{fileName}.log"));

			if (_logFile.Exists)
			{
				_logFile.Delete();
			}

			using FileStream fileStream = _logFile.Create();

			return this;
		}

		#endregion
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
