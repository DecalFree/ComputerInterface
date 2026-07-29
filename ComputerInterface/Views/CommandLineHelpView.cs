using System;
using System.Collections.Generic;
using System.Text;
using ComputerInterface.Behaviors;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;
using ComputerInterface.Models.Command;

namespace ComputerInterface.Views;

internal class CommandLineHelpView : ComputerView {
    private readonly CommandHandler _commandHandler = CommandHandler.Singleton;
    private readonly UITextPageHandler _pageHandler = new(EKeyboardButton.Left, EKeyboardButton.Right) {
        EntriesPerPage = 8
    };

    public override void OnViewShown(object[] arguments) {
        IList<Command> commands = _commandHandler.GetAllCommands();
        string[] lines = new string[commands.Count];

        for (int i = 0; i < lines.Length; i++) {
            Command command = commands[i];

            lines[i] = "- ";

            if (command == null)
                continue;

            lines[i] += command.Name;

            if (command.ArgumentTypes == null)
                continue;

            foreach (Type argumentType in command.ArgumentTypes) {
                if (argumentType == null) {
                    lines[i] += " <string>";
                    continue;
                }

                lines[i] += " <" + argumentType.Name + ">";
            }
        }
        _pageHandler.SetLines(lines);
    }

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginColor("ffffff50").Append("== ").EndColor();
        stringBuilder.Append("Command Line Info").BeginColor("ffffff50").Append(" ==").EndColor().AppendLine();
        stringBuilder.Append("<size=40>Navigate using the Left/Right arrow keys</size>").AppendLines(2);

        string[] lines = _pageHandler.GetLinesForCurrentPage();
        foreach (string line in lines) {
            stringBuilder.Append(line);
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine();
        _pageHandler.AppendFooter(stringBuilder);
        stringBuilder.AppendLine();

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Back:
                ReturnToPreviousView();
                break;
            default:
                if (_pageHandler.HandleButtonPress(pressedButton))
                    UpdateViewScreen();
                break;
        }
    }
}