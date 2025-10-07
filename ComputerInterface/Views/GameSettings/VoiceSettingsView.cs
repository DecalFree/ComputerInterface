using ComputerInterface.Extensions;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;

namespace ComputerInterface.Views.GameSettings;

internal class VoiceSettingsView : ComputerView {
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardKey.Up, EKeyboardKey.Down);

    public VoiceSettingsView() {
        _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");
        _selectionHandler.MaxIdx = 1;
    }

    public override void OnShow(object[] args) {
        base.OnShow(args);
        _selectionHandler.CurrentSelectionIndex = BaseGameInterface.GetVoiceMode() ? 0 : 1;
        Redraw();
    }


    private void Redraw() {
        var stringBuilder = new StringBuilder();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Voice Tab").AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        stringBuilder.AppendLine(_selectionHandler.GetIndicatedText(0, "Human Voices"));
        stringBuilder.AppendLine(_selectionHandler.GetIndicatedText(1, "Monke Voices"));

        stringBuilder.AppendLine().BeginColor("ffffff50").Append("* ").EndColor().Append("Choose which type of voice you would like to both hear and speak.");

        SetText(stringBuilder);
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        if (_selectionHandler.HandleKeypress(key)) {
            BaseGameInterface.SetVoiceMode(_selectionHandler.CurrentSelectionIndex == 0);
            Redraw();
            return;
        }

        switch (key) {
            case EKeyboardKey.Back:
                ShowView<GameSettingsView>();
                break;
        }
    }
}