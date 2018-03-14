using System;

namespace TsT.Components
{
    public class LogEventArgs : EventArgs
    {
        public string Message { get; }

        public MessageType Type { get; }

        public LogEventArgs(string message, MessageType type)
        {
            Message = message;
            Type = type;
        }
    }
}