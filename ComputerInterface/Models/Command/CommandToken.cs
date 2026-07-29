using ComputerInterface.Behaviors;

namespace ComputerInterface.Models.Command;

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