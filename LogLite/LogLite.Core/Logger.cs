using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogLite.Core
{
    /// <summary>
    /// Thread safe singleton implementation. The Logger class is responsible for maintaining a queue of log statements ready to be logged, 
    /// and will flush these to the appropriate sinks.
    /// </summary>
	public sealed class Logger : IDisposable
    {
        private readonly string rootDirectory;
        private readonly string logFileDirectory;

        private readonly List<string> logQueue;

        private readonly object logQueueLock;

        private Task currentTask = null;
        
        private Logger()
        {
            rootDirectory = Path.GetPathRoot(Environment.SystemDirectory);
            logFileDirectory = Path.Combine(rootDirectory, "/Logs");

            logQueue = new List<string>();

            logQueueLock = new object();
        }   

        public static Logger Instance => Nested.instance;

        private class Nested
        {
            static Nested()
            {

            }

            internal static readonly Logger instance = new Logger();
        }

        public void Log(string statement)
        {
            lock (logQueueLock)
            {
                logQueue.Add(statement);
            }

            // Flush to disk asynchronously so we don't disrupt the main thread
            currentTask = Task.Run(Flush);
        }

        public void Flush()
        {
            List<string> statements;
            string logFilePath = Path.Combine(logFileDirectory, "logFile.log");

            lock (logQueueLock)
            {
                statements = new List<string>(logQueue);

                logQueue.Clear();
            }

            if (!Directory.Exists(logFileDirectory))
            {
                Directory.CreateDirectory(logFileDirectory);
            }

            if (!File.Exists(logFilePath))
            {
                File.Create(logFilePath);
            }     

            using FileStream stream = File.Open(logFilePath, FileMode.Open);

            foreach (string item in statements)
            {
                stream.Write(Encoding.UTF8.GetBytes(item));
            }
        }

        public void Dispose()
        {
            if (currentTask != null)
            {
                currentTask.Wait();
            }

            Flush();
        }
    }
}
