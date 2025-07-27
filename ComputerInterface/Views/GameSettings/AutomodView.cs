using ComputerInterface.Extensions;
using ComputerInterface.ViewLib;
using System.Text;

namespace ComputerInterface.Views.GameSettings
{
    public class AutomodView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;

        private AutomodView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");
            _selectionHandler.MaxIdx = 2;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            _selectionHandler.CurrentSelectionIndex = (int)BaseGameInterface.GetAutomodMode();
            Redraw();
        }

        private void Redraw()
        {
            StringBuilder str = new();

            str.BeginCenter().Repeat("=", SCREEN_WIDTH).AppendLine();
            str.Append("Automod Tab").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).EndAlign().AppendLines(2);

            str.AppendLine("Automod Mode: ");
            str.Append(_selectionHandler.GetIndicatedText(0, "Off")).AppendLine()
                .Append(_selectionHandler.GetIndicatedText(1, "Moderate")).AppendLine()
                .Append(_selectionHandler.GetIndicatedText(2, "Aggressive")).AppendLines(2);

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Back:
                    ShowView<GameSettingsView>();
                    break;
                default:
                    if (_selectionHandler.HandleKeypress(key))
                    {
                        BaseGameInterface.SetAutomodMode(_selectionHandler.CurrentSelectionIndex);
                        Redraw();
                        return;
                    }
                    break;
            }
        }
    }
}
