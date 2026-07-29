using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using ComputerInterface.Exceptions;
using ComputerInterface.Interfaces;
using ComputerInterface.Models.Command;
using ComputerInterface.Tools;
using UnityEngine;

namespace ComputerInterface.Behaviors;

public class CommandHandler : MonoBehaviour {
    public static CommandHandler Singleton { get; private set; }
    private readonly bool _initialized;

    private readonly Dictionary<string, Command> _commands = new();

    public CommandHandler() {
        if (_initialized || Singleton != null && Singleton != this) {
            Logging.Info("Failed to start initializing Computer Interface's CommandHandler");
            return;
        }
        Singleton = this;
        _initialized = true;

        List<ICommandRegistrar> commandRegistrars = [];
        IEnumerable<Assembly> assemblies = Chainloader.PluginInfos.Values.Select(pluginInfo => pluginInfo.Instance.GetType().Assembly).Distinct();
        IEnumerable<ICommandRegistrar> foundCommandRegistrars = assemblies.SelectMany(assembly => assembly.GetTypes())
            .Where(foundCommandRegistrar => typeof(ICommandRegistrar).IsAssignableFrom(foundCommandRegistrar) && !foundCommandRegistrar.IsInterface)
            .Select(commandRegistrarType => (ICommandRegistrar)Activator.CreateInstance(commandRegistrarType)).Where(commandRegistrar =>
                commandRegistrars.All(existingEntry => existingEntry.GetType() != commandRegistrar.GetType()));
        commandRegistrars.AddRange(foundCommandRegistrars);
        Logging.Info($"Found {commandRegistrars.Count} command registrars");

        foreach (ICommandRegistrar commandRegistrar in commandRegistrars)
            commandRegistrar.Initialize();

        Logging.Info("Successfully ended initializing Computer Interface's CommandHandler");
    }

    public CommandToken AddCommand(Command command) {
        if (_commands.ContainsKey(command.Name))
            throw new CommandAddException(command.Name, "Command already exists");

        if (command.ArgumentTypes != null) {
            foreach (Type argumentType in command.ArgumentTypes) {
                if (argumentType == null)
                    continue;

                if (!TomlTypeConverter.CanConvert(argumentType))
                    throw new CommandAddException(command.Name, $"Type {argumentType.Name} has no converter");
            }
        }

        _commands.Add(command.Name, command);
        return new CommandToken(this, command.Name, true);
    }

    internal void UnregisterCommand(string commandName) {
        _commands.Remove(commandName);
        Logging.Error($"Unregistered command: {commandName}");
    }

    public bool Execute(string commandString, out string messageString) {
        commandString = commandString.ToLower();

        messageString = "";

        string[] commandStrings = commandString.Split(' ');
        if (!_commands.TryGetValue(commandStrings[0], out Command command)) {
            messageString = "Command not found!";
            return false;
        }

        // Check if the number of arguments is correct
        int argumentCount = commandStrings.Length - 1;
        if (argumentCount != command.ArgumentCount) {
            messageString = $"Incorrect number of arguments!\nGot {argumentCount}\nShould be {command.ArgumentCount}";
            return false;
        }

        // If there are no arguments passed the desired argument count is zero
        // Execute the command
        if (argumentCount == 0)
            messageString = command.Callback?.Invoke(null);

        // If there are arguments present move them into a new array
        object[] arguments = new object[argumentCount];
        for (int i = 1; i < argumentCount + 1; i++) {
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

    public IList<Command> GetAllCommands() => [.. _commands.Values];
}