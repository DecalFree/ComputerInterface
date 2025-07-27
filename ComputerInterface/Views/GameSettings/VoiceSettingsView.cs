using ComputerInterface.Extensions;
using ComputerInterface.ViewLib;
using System.Text;

namespace ComputerInterface.Views.GameSettings
{
    internal class VoiceSettingsView : ComputerView
    {
        private readonly UISelectionHandler _selectionHandler;

        public VoiceSettingsView()
        {
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down);
            _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");
            _selectionHandler.MaxIdx = 1;
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            _selectionHandler.CurrentSelectionIndex = BaseGameInterface.GetVoiceMode() ? 0 : 1;
            Redraw();
        }


        public void Redraw()
        {
            StringBuilder str = new();

            str.BeginCenter().Repeat("=", SCREEN_WIDTH).AppendLine();
            str.Append("Voice Tab").AppendLine();
            str.Repeat("=", SCREEN_WIDTH).EndAlign().AppendLines(2);

            str.AppendLine(_selectionHandler.GetIndicatedText(0, "Human Voices"));
            str.AppendLine(_selectionHandler.GetIndicatedText(1, "Monke Voices"));

            str.AppendLine().BeginColor("ffffff50").Append("* ").EndColor().Append("Choose which type of voice you would like to both hear and speak.");

            SetText(str);
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
                BaseGameInterface.SetVoiceMode(_selectionHandler.CurrentSelectionIndex == 0);
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