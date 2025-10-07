using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using System;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;

namespace ComputerInterface.Views;

public class CommandLineEntry : IComputerModEntry {
    public string EntryName => "Command Line";
    public Type EntryViewType => typeof(CommandLineView);
}

public class CommandLineView : ComputerView {
    private readonly CommandHandler _commandHandler = CommandHandler.Singleton;
    private readonly UITextInputHandler _textInputHandler = new();

    private string _notification = "";
    private string _previousCommand = "";

    public override void OnShow(object[] args) {
        base.OnShow(args);
        Redraw();
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();
        DrawHeader(stringBuilder);
        DrawCurrentCommand(stringBuilder);

        Text = stringBuilder.ToString();
    }

    private void DrawHeader(StringBuilder stringBuilder) {
        stringBuilder.BeginColor("ffffff50").Append("== ").EndColor();
        stringBuilder.Append("Command Line").BeginColor("ffffff50").Append(" ==").EndColor().AppendLine();
        stringBuilder.Append("<size=40>Press Option 1 to roomView command list</size>").AppendLines(2);
    }

    private void DrawCurrentCommand(StringBuilder stringBuilder) {
        if (!string.IsNullOrEmpty(_notification))
            stringBuilder.Append("  <color=#ffffff60>").Append(_notification.Replace("\n", "\n  ")).Append("</color>").AppendLine();

        stringBuilder.AppendClr(">", "ffffff60").Append(_textInputHandler.Text).AppendClr("_", "ffffff60").AppendLine();
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        if (_textInputHandler.HandleKey(key)) {
            Redraw();
            return;
        }

        switch (key) {
            case EKeyboardKey.Enter:
                RunCommand();
                break;
            case EKeyboardKey.Back:
                ReturnToMainMenu();
                break;
            case EKeyboardKey.Option1:
                ShowView<CommandLineHelpView>();
                break;
            case EKeyboardKey.Up:
                if (_previousCommand == "") return;
                _textInputHandler.Text = _previousCommand;
                _previousCommand = "";
                Redraw();
                break;
            default:
                _previousCommand = "";
                break;
        }
    }

    private void RunCommand() {
        _notification = "";
        var success = _commandHandler.Execute(_textInputHandler.Text, out var messageString);

        _notification = messageString;

        _previousCommand = "";
        if (success) {
            _previousCommand = _textInputHandler.Text;
            _textInputHandler.Text = "";
        }

        Redraw();
    }
}