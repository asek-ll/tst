using System;

namespace TsT.Components
{
    public enum MessageType
    {
        REGULAR,
        OK,
        ERROR,
    }

    public class Logger
    {
        public event EventHandler<LogEventArgs> OnLogging;

        public void Log(string message, MessageType type = MessageType.REGULAR)
        {
            OnLogging?.Invoke(this, new LogEventArgs(message, type));
        }

        public void Error(string message)
        {
            OnLogging?.Invoke(this, new LogEventArgs(message, MessageType.ERROR));
        }

        public void Success(string message)
        {
            OnLogging?.Invoke(this, new LogEventArgs(message, MessageType.OK));
        }
    }
}