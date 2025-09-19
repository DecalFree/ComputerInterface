using ComputerInterface.Extensions;
using ComputerInterface.ViewLib;
using System.Text;

namespace ComputerInterface.Views
{
    public class CommandLineHelpView : ComputerView
    {
        private readonly CommandHandler _commandHandler;
        private readonly UITextPageHandler _pageHandler;

        public CommandLineHelpView()
        {
            _commandHandler = Plugin.CommandHandler;
            _pageHandler = new UITextPageHandler(EKeyboardKey.Left, EKeyboardKey.Right);
            _pageHandler.EntriesPerPage = 8;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            System.Collections.Generic.IList<Command> commands = _commandHandler.GetAllCommands();
            string[] lines = new string[commands.Count];

            for (int i = 0; i < lines.Length; i++)
            {
                Command command = commands[i];

                lines[i] = "- ";

                if (command == null) continue;

                lines[i] += command.Name;

                if (command.ArgumentTypes != null)
                {
                    foreach (System.Type argType in command.ArgumentTypes)
                    {
                        if (argType == null)
                        {
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

        public void Redraw()
        {
            StringBuilder str = new();

            DrawHeader(str);
            DrawCommands(str);

            SetText(str);
        }

        public void DrawHeader(StringBuilder str)
        {
            str.BeginColor("ffffff50").Append("== ").EndColor();
            str.Append("Command Line Info").BeginColor("ffffff50").Append(" ==").EndColor().AppendLine();
            str.Append("<size=40>Nativate using the Left/Right arrow keys</size>").AppendLines(2);
        }

        public void DrawCommands(StringBuilder str)
        {
            string[] lines = _pageHandler.GetLinesForCurrentPage();
            for (int i = 0; i < lines.Length; i++)
            {
                str.Append(lines[i]);
                str.AppendLine();
            }

            str.AppendLine();
            _pageHandler.AppendFooter(str);
            str.AppendLine();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_pageHandler.HandleKeyPress(key))
            {
                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Back:
                    ReturnView();
                    break;
            }
        }
    }
}