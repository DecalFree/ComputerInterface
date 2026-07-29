using System.Text;
using ComputerInterface.Behaviors;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;

namespace ComputerInterface.Views.GameSettings;

internal class AutoMuteView : ComputerView {
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardButton.Up, EKeyboardButton.Down) {
        MaxIndex = 2
    };

    public AutoMuteView() => _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");

    public override void OnViewShown(object[] arguments) => _selectionHandler.CurrentSelectionIndex = (int)GameInterfaceService.AutoMuteType;

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("AutoMute Tab").AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        stringBuilder.AppendLine("AutoMute Type: ");
        stringBuilder.AppendLine(_selectionHandler.GetIndicatedText(0, "Off"));
        stringBuilder.AppendLine(_selectionHandler.GetIndicatedText(1, "Moderate"));
        stringBuilder.AppendLine(_selectionHandler.GetIndicatedText(2, "Aggressive"));

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Back:
                ShowView<GameSettingsView>();
                break;
            default:
                if (_selectionHandler.HandleButtonPress(pressedButton)) {
                    GameInterfaceService.AutoMuteType = (EAutoMuteType)_selectionHandler.CurrentSelectionIndex;
                    UpdateViewScreen();
                }
                break;
        }
    }
}