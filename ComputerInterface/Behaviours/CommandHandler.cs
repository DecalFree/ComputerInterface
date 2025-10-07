using BepInEx.Configuration;
using ComputerInterface.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using ComputerInterface.Interfaces;
using ComputerInterface.Tools;

namespace ComputerInterface.Behaviours;

public class CommandHandler {
    public static CommandHandler Singleton { get; private set; }
        
    private readonly Dictionary<string, Command> _commands = new();

    public CommandHandler() {
        if (Singleton != null && Singleton != this)
            return;
        Singleton = this;

        List<ICommandRegistrar> commandRegistrars = [];
        var modAssemblies = Chainloader.PluginInfos.Values.Select(pluginInfo => pluginInfo.Instance.GetType().Assembly).Distinct();
        var modCommandRegistrarTypes = modAssemblies.SelectMany(assembly => assembly.GetTypes()).Where(type => typeof(ICommandRegistrar).IsAssignableFrom(type) && !type.IsInterface);
        var modCommandRegistrars = modCommandRegistrarTypes.Select(type => (ICommandRegistrar)Activator.CreateInstance(type)).Where(registrar =>
            commandRegistrars.All(existingRegistrar => existingRegistrar.GetType() != registrar.GetType()));
        commandRegistrars.AddRange(modCommandRegistrars);
        Logging.Info($"Found {commandRegistrars.Count} command registrars");
        foreach (var commandRegistrar in commandRegistrars)
            commandRegistrar.Initialize();
    }

    public CommandToken AddCommand(Command command) {
        if (_commands.ContainsKey(command.Name))
            throw new CommandAddException(command.Name, "Command already exists");

        if (command.ArgumentTypes != null) {
            foreach (var argumentType in command.ArgumentTypes) {
                if (argumentType == null)
                    continue;

                if (!TomlTypeConverter.CanConvert(argumentType))
                    throw new CommandAddException(command.Name, $"Type {argumentType.Name} has no converter");
            }
        }

        _commands.Add(command.Name, command);
        return new CommandToken(this, command.Name, true);
    }

    internal void UnregisterCommand(string name) {
        _commands.Remove(name);
        Logging.Error($"Unregistered command: {name}");
    }

    public bool Execute(string commandString, out string messageString) {
        commandString = commandString.ToLower();

        messageString = "";

        var commandStrings = commandString.Split(' ');
        if (!_commands.TryGetValue(commandStrings[0], out var command)) {
            messageString = "Command not found!";
            return false;
        }

        // Check if the number of arguments is correct
        var argumentCount = commandStrings.Length - 1;
        if (argumentCount != command.ArgumentCount) {
            messageString = $"Incorrect number of arguments!\nGot {argumentCount}\nShould be {command.ArgumentCount}";
            return false;
        }

        // If there are no arguments passed the desired argument count is zero
        // Execute the command
        if (argumentCount == 0)
            messageString = command.Callback?.Invoke(null);

        // If there are arguments present move them into a new array
        var arguments = new object[argumentCount];
        for (var i = 1; i < argumentCount + 1; i++) {
            if (command.ArgumentTypes[i - 1] == null) {
                arguments[i - 1] = commandStrings[i];
                continue;
            }

            try {
                arguments[i - 1] = TomlTypeConverter.ConvertToValue(commandStrings[i], command.ArgumentTypes[i - 1]);
            }
            catch {
                messageString = "Incorrect arguments!\nArguments aren't in the correct format.";
                return false;
            }

        }

        messageString = command.Callback?.Invoke(arguments);

        return true;
    }

    public IList<Command> GetAllCommands() =>
        _commands.Values.ToList();
}

public class Command(string name, Type[] argumentTypes, Func<object[], string> callback) {
    public readonly string Name = name;
    public readonly Type[] ArgumentTypes = argumentTypes;
    public readonly Func<object[], string> Callback = callback;

    public int ArgumentCount => ArgumentTypes?.Length ?? 0;
}

public class CommandToken {
    private readonly CommandHandler _commandHandler;
    private readonly string _name;
    private readonly bool _success;

    private bool _unregistered;

    internal CommandToken(CommandHandler commandHandler, string name, bool success) {
        _commandHandler = commandHandler;
        _name = name;
        _success = success;
    }

    public void UnregisterCommand() {
        if (!_success || _unregistered)
            return;

        _unregistered = true;
        _commandHandler.UnregisterCommand(_name);
    }
}