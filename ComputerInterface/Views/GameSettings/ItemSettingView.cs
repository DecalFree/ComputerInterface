using System.Text;
using ComputerInterface.Behaviors;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings;

internal class ItemSettingView : ComputerView {
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardButton.Up, EKeyboardButton.Down) {
        MaxIndex = 1
    };

    private float _instrumentVolume = 0.1f;

    private void UpdateState() {
        _selectionHandler.CurrentSelectionIndex = GameInterfaceService.ItemParticles ? 0 : 1;
        _instrumentVolume = GameInterfaceService.InstrumentVolume;
    }

    public ItemSettingView() => _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Items Tab").AppendLine();
        stringBuilder.AppendClr("0 - 9 to set Instrument Volume", "ffffff50").AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        stringBuilder.Append("Instrument Volume: ").Append(Mathf.CeilToInt(_instrumentVolume * 50f));

        stringBuilder.AppendLines(3).Append("Item Particles:").AppendLine();
        stringBuilder.Append(_selectionHandler.GetIndicatedText(0, "Enabled")).AppendLine();
        stringBuilder.Append(_selectionHandler.GetIndicatedText(1, "Disabled")).AppendLine();

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Back:
                ShowView<GameSettingsView>();
                break;
            default:
                if (_selectionHandler.HandleButtonPress(pressedButton)) {
                    GameInterfaceService.ItemParticles = _selectionHandler.CurrentSelectionIndex == 0;
                    UpdateViewScreen();
                    return;
                }

                if (pressedButton.TryParseNumber(out int num)) {
                    GameInterfaceService.InstrumentVolume = num;
                    UpdateState();
                    UpdateViewScreen();
                }
                break;
        }
    }
}