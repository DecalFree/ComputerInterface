using System;

namespace ComputerInterface.Exceptions;

public class CommandAddException(string commandName, string message) : Exception($"Error adding command {commandName}\n{message}");