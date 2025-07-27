using ComputerInterface.Extensions;
using ComputerInterface.ViewLib;
using System.Text;

namespace ComputerInterface.Views.GameSettings
{
    internal class MicSettingsView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;

        public MicSettingsView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");
            _selectionHandler.MaxIdx = 2;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            _selectionHandler.CurrentSelectionIndex = (int)BaseGameInterface.GetPttMode();
            Redraw();
        }

        public void Redraw()
        {
            StringBuilder str = new();

            str.BeginCenter().Repeat("=", SCREEN_WIDTH).AppendLine();
            str.Append("Mic Tab").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).EndAlign().AppendLines(2);

            str.AppendLine(_selectionHandler.GetIndicatedText(0, "All Chat"));
            str.AppendLine(_selectionHandler.GetIndicatedText(1, "Push to Talk"));
            str.AppendLine(_selectionHandler.GetIndicatedText(2, "Push to Mute"));

            str.AppendLine().BeginColor("ffffff50").Append("* ").EndColor().Append("\"Push to Talk\" and \"Push to Mute\" work with any face button.");

            SetText(str);
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
                BaseGameInterface.SetPttMode((BaseGameInterface.EPTTMode)_selectionHandler.CurrentSelectionIndex);
                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Back:
                    ShowView<GameSettingsView>();
                    break;
            }
        }
    }
}