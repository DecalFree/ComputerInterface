using System.Text;
using ComputerInterface.Behaviors;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;

namespace ComputerInterface.Views.GameSettings;

internal class MicrophoneSettingView : ComputerView {
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardButton.Up, EKeyboardButton.Down) {
        MaxIndex = 2
    };

    public MicrophoneSettingView() => _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");

    public override void OnViewShown(object[] arguments) => _selectionHandler.CurrentSelectionIndex = (int)GameInterfaceService.MicrophoneType;

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Microphone Tab").AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        stringBuilder.AppendLine("Microphone Type: ");
        stringBuilder.AppendLine(_selectionHandler.GetIndicatedText(0, "Open Microphone"));
        stringBuilder.AppendLine(_selectionHandler.GetIndicatedText(1, "Push to Talk"));
        stringBuilder.AppendLine(_selectionHandler.GetIndicatedText(2, "Push to Mute"));

        stringBuilder.AppendLines(3).AppendClr("* Push to Talk and Push to Mute work with any face button.", "ffffff50").AppendLine();

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Back:
                ShowView<GameSettingsView>();
                break;
            default:
                if (_selectionHandler.HandleButtonPress(pressedButton)) {
                    GameInterfaceService.MicrophoneType = (EMicrophoneType)_selectionHandler.CurrentSelectionIndex;
                    UpdateViewScreen();
                }
                break;
        }
    }
}