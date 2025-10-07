using ComputerInterface.Extensions;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;

namespace ComputerInterface.Views;

public class CommandLineHelpView : ComputerView {
    private readonly CommandHandler _commandHandler = CommandHandler.Singleton;
    private readonly UITextPageHandler _pageHandler = new(EKeyboardKey.Left, EKeyboardKey.Right) {
        EntriesPerPage = 8
    };

    public override void OnShow(object[] args) {
        base.OnShow(args);

        var commands = _commandHandler.GetAllCommands();
        var lines = new string[commands.Count];

        for (var i = 0; i < lines.Length; i++) {
            var command = commands[i];

            lines[i] = "- ";

            if (command == null)
                continue;

            lines[i] += command.Name;

            if (command.ArgumentTypes != null) {
                foreach (var argType in command.ArgumentTypes) {
                    if (argType == null) {
                        lines[i] += " <string>";
                        continue;
                    }

                    lines[i] += " <" + argType.Name + ">";
                }
            }
        }
        _pageHandler.SetLines(lines);

        Redraw();
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();

        DrawHeader(stringBuilder);
        DrawCommands(stringBuilder);

        SetText(stringBuilder);
    }

    private void DrawHeader(StringBuilder stringBuilder) {
        stringBuilder.BeginColor("ffffff50").Append("== ").EndColor();
        stringBuilder.Append("Command Line Info").BeginColor("ffffff50").Append(" ==").EndColor().AppendLine();
        stringBuilder.Append("<size=40>Nativate using the Left/Right arrow keys</size>").AppendLines(2);
    }

    private void DrawCommands(StringBuilder stringBuilder) {
        var lines = _pageHandler.GetLinesForCurrentPage();
        foreach (var line in lines) {
            stringBuilder.Append(line);
            stringBuilder.AppendLine();
        }

        stringBuilder.AppendLine();
        _pageHandler.AppendFooter(stringBuilder);
        stringBuilder.AppendLine();
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        if (_pageHandler.HandleKeyPress(key)) {
            Redraw();
            return;
        }

        switch (key) {
            case EKeyboardKey.Back:
                ReturnView();
                break;
        }
    }
}