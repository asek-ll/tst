using System;

namespace TsT.Components
{
    public class LogEventArgs : EventArgs
    {
        public readonly string Message;

        public LogEventArgs(string message)
        {
            Message = message;
        }
    }
}