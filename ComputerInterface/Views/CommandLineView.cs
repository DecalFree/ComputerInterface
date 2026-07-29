using System;
using System.Text;
using ComputerInterface.Behaviors;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using ComputerInterface.Models;

namespace ComputerInterface.Views;

internal class CommandLineEntry : IComputerViewEntry {
    public string EntryName => "Command Line";

    public Type EntryComputerView => typeof(CommandLineView);
}

internal class CommandLineView : ComputerView {
    private readonly CommandHandler _commandHandler = CommandHandler.Singleton;
    private readonly UITextInputHandler _textInputHandler = new();

    private string _notification = "";
    private string _previousCommand = "";

    private void RunCommand() {
        _notification = "";
        bool isSuccessful = _commandHandler.Execute(_textInputHandler.Text, out string messageString);

        _notification = messageString;

        _previousCommand = "";
        if (isSuccessful) {
            _previousCommand = _textInputHandler.Text;
            _textInputHandler.Text = "";
        }

        UpdateViewScreen();
    }

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginColor("ffffff50").Append("== ").EndColor();
        stringBuilder.Append("Command Line").BeginColor("ffffff50").Append(" ==").EndColor().AppendLine();
        stringBuilder.Append("<size=40>Press Option 1 to view command list</size>").AppendLines(2);

        if (!string.IsNullOrEmpty(_notification))
            stringBuilder.Append("  <color=#ffffff60>").Append(_notification.Replace("\n", "\n  ")).Append("</color>").AppendLine();

        stringBuilder.AppendClr(">", "ffffff60").Append(_textInputHandler.Text).AppendClr("_", "ffffff60").AppendLine();

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Enter:
                RunCommand();
                break;
            case EKeyboardButton.Back:
                ReturnToMainMenu();
                break;
            case EKeyboardButton.Option1:
                ShowView<CommandLineHelpView>();
                break;
            case EKeyboardButton.Up:
                if (_previousCommand == "")
                    return;

                _textInputHandler.Text = _previousCommand;
                _previousCommand = "";
                UpdateViewScreen();
                break;
            default:
                if (_textInputHandler.HandleButtonPress(pressedButton)) {
                    UpdateViewScreen();
                    return;
                }

                _previousCommand = "";
                break;
        }
    }
}