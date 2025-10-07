using ComputerInterface.Extensions;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;

namespace ComputerInterface.Views.GameSettings;

public class TurnSettingView : ComputerView {
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardKey.Up, EKeyboardKey.Down);

    private int _turnSpeed = 4;

    public TurnSettingView() {
        _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");
        _selectionHandler.MaxIdx = 2;
    }

    public override void OnShow(object[] args) {
        base.OnShow(args);
        _selectionHandler.CurrentSelectionIndex = (int)BaseGameInterface.GetTurnMode();
        _turnSpeed = BaseGameInterface.GetTurnValue();
        Redraw();
    }

    private void SetTurnSpeed(int value) {
        _turnSpeed = value;
        BaseGameInterface.SetTurnValue(value);
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Turn Tab").AppendLine();
        stringBuilder.AppendClr("1 - 9 to change turn speed", "ffffff50").AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        stringBuilder.AppendLine("Turn Mode: ");
        stringBuilder.Append(_selectionHandler.GetIndicatedText(0, "Snap")).AppendLine()
            .Append(_selectionHandler.GetIndicatedText(1, "Smooth")).AppendLine()
            .Append(_selectionHandler.GetIndicatedText(2, "None")).AppendLines(2);

        stringBuilder.Append("Speed: ").Append(_turnSpeed);

        Text = stringBuilder.ToString();
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        switch (key) {
            case EKeyboardKey.Back:
                ShowView<GameSettingsView>();
                break;
            default:
                if (_selectionHandler.HandleKeypress(key)) {
                    BaseGameInterface.SetTurnMode((ETurnMode)_selectionHandler.CurrentSelectionIndex);
                    Redraw();
                    return;
                }
                if (key.TryParseNumber(out var num)) {
                    SetTurnSpeed(num);
                    Redraw();
                }
                break;
        }
    }
}