using ComputerInterface.Extensions;
using ComputerInterface.ViewLib;
using System.Text;

namespace ComputerInterface.Views
{
    internal class WarnView : ComputerView
    {
        internal static IWarning _currentWarn;

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            _currentWarn = (IWarning)args[0]; // No way I'm actually using these arguments
            Redraw();
        }

        public void Redraw()
        {
            StringBuilder str = new();
            str.BeginColor("ffffff50").Append("== ").EndColor();
            str.Append("Warning").BeginColor("ffffff50").Append(" ==").EndColor().AppendLines(2);

            str.AppendLine(_currentWarn.WarningMessage);

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (key == EKeyboardKey.Back)
            {
                ReturnToMainMenu();
            }
        }

        internal interface IWarning
        {
            string WarningMessage { get; }
        }

        public class GeneralWarning : IWarning
        {
            private readonly string message;

            public GeneralWarning(string message)
            {
                this.message = message;
            }

            public string WarningMessage => message;
        }

        public class OutdatedWarning : IWarning
        {
            public string WarningMessage => "You aren't on the latest version of Gorilla Tag, please update your game to continue playing with others.";
        }

        public class NoInternetWarning : IWarning
        {
            public string WarningMessage => "You aren't connected to an internet connection, please connect to a valid connection to continue playing with others.";
        }

        public class TemporaryBanWarning : IWarning
        {
            private readonly string reason;
            private readonly int hoursRemaining;

            public TemporaryBanWarning(string reason, int hoursRemaining)
            {
                this.reason = reason;
                this.hoursRemaining = hoursRemaining;
            }

            public string WarningMessage => $"You have been temporarily banned. You will not be able to play with others until the ban expires.\nReason: {reason}\nHours remaining: {hoursRemaining}";
        }

        public class PermanentBanWarning : IWarning
        {
            private readonly string reason;

            public PermanentBanWarning(string reason)
            {
                this.reason = reason;
            }

            public string WarningMessage => $"You have been permanently banned.\nReason: {reason}";
        }
    }
}
