using ComputerInterface.Extensions;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;

namespace ComputerInterface.Views.GameSettings;

public class AutomodView : ComputerView {
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardKey.Up, EKeyboardKey.Down);

    public AutomodView() {
        _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");
        _selectionHandler.MaxIdx = 2;
    }

    public override void OnShow(object[] args) {
        base.OnShow(args);

        _selectionHandler.CurrentSelectionIndex = (int)BaseGameInterface.GetAutomodMode();
        Redraw();
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Automod Tab").AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        stringBuilder.AppendLine("Automod Mode: ");
        stringBuilder.Append(_selectionHandler.GetIndicatedText(0, "Off")).AppendLine()
            .Append(_selectionHandler.GetIndicatedText(1, "Moderate")).AppendLine()
            .Append(_selectionHandler.GetIndicatedText(2, "Aggressive")).AppendLines(2);

        Text = stringBuilder.ToString();
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        switch (key) {
            case EKeyboardKey.Back:
                ShowView<GameSettingsView>();
                break;
            default:
                if (_selectionHandler.HandleKeypress(key)) {
                    BaseGameInterface.SetAutomodMode(_selectionHandler.CurrentSelectionIndex);
                    Redraw();
                }
                break;
        }
    }
}