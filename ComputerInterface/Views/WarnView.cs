using ComputerInterface.Extensions;
using System.Text;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;

namespace ComputerInterface.Views;

internal class WarnView : ComputerView {
    private static IWarning _currentWarn;

    public override void OnShow(object[] args) {
        base.OnShow(args);

        _currentWarn = (IWarning)args[0]; // No way I'm actually using these arguments
        Redraw();
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();
        stringBuilder.BeginColor("ffffff50").Append("== ").EndColor();
        stringBuilder.Append("Warning").BeginColor("ffffff50").Append(" ==").EndColor().AppendLines(2);

        stringBuilder.AppendLine(_currentWarn.WarningMessage);

        Text = stringBuilder.ToString();
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        if (key == EKeyboardKey.Back)
            ReturnToMainMenu();
    }

    private interface IWarning {
        string WarningMessage { get; }
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

    public class TemporaryBanWarning(string reason, int hoursRemaining) : IWarning {
        public string WarningMessage => $"You have been temporarily banned. You will not be able to play with others until the ban expires.\nReason: {reason}\nHours remaining: {hoursRemaining}";
    }

    public class PermanentBanWarning(string reason) : IWarning {
        public string WarningMessage => $"You have been permanently banned.\nReason: {reason}";
    }
}