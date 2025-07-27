using System;

namespace ComputerInterface.Exceptions
{
    public class CommandAddException : Exception
    {
        public CommandAddException(string commandName, string message) : base($"Error adding command {commandName}\n{message}")
        {
        }
    }
}
