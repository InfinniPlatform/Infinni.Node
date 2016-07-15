using System;

namespace Infinni.Node.CommandHandlers
{
    [Serializable]
    public class CommandHandlerException : Exception
    {
        public CommandHandlerException(string message) : base(message)
        {
        }

        public CommandHandlerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public override string ToString()
        {
            return Message;
        }
    }
}