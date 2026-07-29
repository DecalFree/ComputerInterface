using System.Text;
using ComputerInterface.Behaviors;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;

namespace ComputerInterface.Views.GameSettings;

internal class TurnSettingView : ComputerView {
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardButton.Up, EKeyboardButton.Down) {
        MaxIndex = 2
    };

    private int _turnFactor = 4;

    public TurnSettingView() => _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");

    public override void OnViewShown(object[] arguments) {
        _selectionHandler.CurrentSelectionIndex = (int)GameInterfaceService.TurnType;
        _turnFactor = GameInterfaceService.TurnFactor;
    }

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Turn Tab").AppendLine();
        stringBuilder.AppendClr("1 - 9 to change turn factor", "ffffff50").AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        stringBuilder.AppendLine("Turn Type: ");
        stringBuilder.AppendLine(_selectionHandler.GetIndicatedText(0, "Snap"));
        stringBuilder.AppendLine(_selectionHandler.GetIndicatedText(1, "Smooth"));
        stringBuilder.AppendLine(_selectionHandler.GetIndicatedText(2, "None"));

        stringBuilder.AppendLines(1).Append("Turn Factor: ").Append(_turnFactor);

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Back:
                ShowView<GameSettingsView>();
                break;
            default:
                if (_selectionHandler.HandleButtonPress(pressedButton)) {
                    GameInterfaceService.TurnType = (ETurnType)_selectionHandler.CurrentSelectionIndex;
                    UpdateViewScreen();
                    return;
                }
                if (pressedButton.TryParseNumber(out int num)) {
                    _turnFactor = num;
                    GameInterfaceService.TurnFactor = num;
                    UpdateViewScreen();
                }
                break;
        }
    }
}