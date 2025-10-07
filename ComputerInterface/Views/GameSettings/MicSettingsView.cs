using ComputerInterface.Extensions;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;

namespace ComputerInterface.Views.GameSettings;

internal class MicSettingsView : ComputerView {
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardKey.Up, EKeyboardKey.Down);

    public MicSettingsView() {
        _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");
        _selectionHandler.MaxIdx = 2;
    }

    public override void OnShow(object[] args) {
        base.OnShow(args);
        _selectionHandler.CurrentSelectionIndex = (int)BaseGameInterface.GetPttMode();
        Redraw();
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Mic Tab").AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        stringBuilder.AppendLine(_selectionHandler.GetIndicatedText(0, "All Chat"));
        stringBuilder.AppendLine(_selectionHandler.GetIndicatedText(1, "Push to Talk"));
        stringBuilder.AppendLine(_selectionHandler.GetIndicatedText(2, "Push to Mute"));

        stringBuilder.AppendLine().BeginColor("ffffff50").Append("* ").EndColor().Append("\"Push to Talk\" and \"Push to Mute\" work with any face button.");

        SetText(stringBuilder);
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        if (_selectionHandler.HandleKeypress(key)) {
            BaseGameInterface.SetPttMode((EPTTMode)_selectionHandler.CurrentSelectionIndex);
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