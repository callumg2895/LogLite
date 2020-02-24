﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LogLite.Core
{
	public interface ILoggerSink
	{
		public void Write(string statement);

		public void Flush();
	}
}
