using System.Text;
using ComputerInterface.Behaviors;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;

namespace ComputerInterface.Views.GameSettings;

internal class VoiceSettingView : ComputerView {
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardButton.Up, EKeyboardButton.Down) {
        MaxIndex = 1
    };

    public VoiceSettingView() => _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");

    public override void OnViewShown(object[] arguments) => _selectionHandler.CurrentSelectionIndex = GameInterfaceService.VoiceChatOn ? 0 : 1;

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Voice Tab").AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        stringBuilder.AppendLine("Voice Type: ");
        stringBuilder.Append(_selectionHandler.GetIndicatedText(0, "Human Voices")).AppendLine();
        stringBuilder.Append(_selectionHandler.GetIndicatedText(1, "Monke Voices")).AppendLine();

        stringBuilder.AppendLines(4).AppendClr("* Choose which type of voice you would like to both hear and speak.", "ffffff50").AppendLine();

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Back:
                ShowView<GameSettingsView>();
                break;
            default:
                if (_selectionHandler.HandleButtonPress(pressedButton)) {
                    GameInterfaceService.VoiceChatOn = _selectionHandler.CurrentSelectionIndex == 0;
                    UpdateViewScreen();
                }
                break;
        }
    }
}