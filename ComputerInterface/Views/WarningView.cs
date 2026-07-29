using System.Text;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;

namespace ComputerInterface.Views;

internal class WarningView : ComputerView {
    private interface IWarning {
        string WarningMessage { get; }
    }

    private static IWarning _currentWarning;

    public override void OnViewShown(object[] arguments) => _currentWarning = arguments[0] as IWarning;

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginColor("ffffff50").Append("== ").EndColor();
        stringBuilder.Append("Warning").BeginColor("ffffff50").Append(" ==").EndColor().AppendLines(2);

        stringBuilder.AppendLine(_currentWarning.WarningMessage);

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Back:
                ReturnToMainMenu();
                break;
        }
    }

    public class GeneralWarning(string message) : IWarning {
        public string WarningMessage => message;
    }

    public class OutdatedWarning : IWarning {
        public string WarningMessage => "You aren't on the latest version of Gorilla Tag, please update your game to continue playing with others.";
    }

    public class NoInternetWarning : IWarning {
        public string WarningMessage => "You aren't connected to an internet connection, please connect to a valid connection to continue playing with others.";
    }
}