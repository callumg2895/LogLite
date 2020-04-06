using System;

namespace LogLite.Core.Interface
{
	public interface ILoggerSink : IDisposable
	{

		public void Write(string statement);

	}
}
