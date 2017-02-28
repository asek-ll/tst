using System;

namespace TsT.Components
{
    public class Logger
    {
        public event EventHandler<LogEventArgs> OnLogging;

        public void Log(string message)
        {
            if (OnLogging != null) OnLogging(this, new LogEventArgs(message));
        }
    }
}