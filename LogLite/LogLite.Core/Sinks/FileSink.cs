using LogLite.Core.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace LogLite.Core.Sinks
{
	public class FileSink : Sink
	{
		private DirectoryInfo? _logFileDirectory;
		private FileInfo? _logFile;

		/*
		 * All operations performed on the file must be locked. Some of these operations will
		 * delete the file. If this were to occur during a flush, the file stream would have 
		 * nowhere to write to, which would probably result in a horrible exception.
		 */

		private object _fileLock = new object();

		public FileSink()
			: this(null)
		{

		}

		public FileSink(LogLevel? filter)
			: base(filter)
		{
			string rootDirectory = Path.GetPathRoot(Environment.SystemDirectory)!;
			string logFileDirectory = Path.Combine(rootDirectory, "/logs");

			ConfigureDirectoryName(logFileDirectory);
			ConfigureFileName("logFile");
		}

		#region Configuration

		public FileSink ConfigureDirectoryName(string directoryName)
		{
			lock (_fileLock)
			{
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}

				_logFileDirectory = new DirectoryInfo(directoryName);
			}

			return this;
		}

		public FileSink ConfigureFileName(string fileName)
		{
			lock (_fileLock)
			{
				if (_logFile != null && _logFile.Exists)
				{
					_logFile.Delete();
				}

				_logFile = new FileInfo(Path.Combine(_logFileDirectory!.FullName, $"{fileName}.log"));

				if (_logFile.Exists)
				{
					_logFile.Delete();
				}

				using FileStream fileStream = _logFile.Create();
			}

			return this;
		}

		#endregion
		protected override void Flush()
		{
			lock (_fileLock)
			{
				Thread.Sleep(FlushTimeoutMilliseconds);

				using FileStream fileStream = _logFile!.Open(FileMode.Append);
				using StreamWriter streamWriter = new StreamWriter(fileStream);

				while (true)
				{
					LogStatement? statement;

					lock (_lock)
					{
						if (!_logQueue.TryDequeue(out statement))
						{
							break;
						}
					}

					streamWriter.WriteLine(statement.ToString());
				}
			}
		}
	}
}
