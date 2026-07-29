using System.Text;
using ComputerInterface.Behaviors;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings;

internal class TroopView : ComputerView {
    private readonly UITextInputHandler _textInputHandler = new();

    private bool ShowFailureMessage => _failureMessage != null;
    private string _failureMessage;

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Troop Tab").AppendLine();

        if (!GameInterfaceService.IsInTroop) {
            switch (ShowFailureMessage) {
                case true:
                    stringBuilder.AppendClr(_failureMessage, "ffffff50").AppendLine();
                    break;
                case false:
                    if (_textInputHandler.Text != GameInterfaceService.Computer.currentName)
                        stringBuilder.AppendClr("Ready - Enter to Join or Create Troop", "ffffff50").AppendLine();
                    break;
            }
        }

        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        if (GameInterfaceService.IsValidTroopName(GameInterfaceService.Computer.troopName)) {
            stringBuilder.AppendLine($"Current Troop: {(GameInterfaceService.TroopQueueActive ? GameInterfaceService.TroopName : GameInterfaceService.Queue)}");
            stringBuilder.AppendLine($"Players In Troop: {Mathf.Max(1, GameInterfaceService.TroopPopulation)}");

            stringBuilder.AppendLines(4).AppendClr($"* {(GameInterfaceService.TroopQueueActive ? "Press Option 2 for default queue." : "Press Option 1 for troop queue.")}", "ffffff50").AppendLine();
            stringBuilder.AppendClr("* Press Option 3 to leave your troop.", "ffffff50").AppendLine();
        }
        else {
            stringBuilder.BeginColor("ffffff50").Append("> ").EndColor().Append(_textInputHandler.Text).AppendClr("_", "ffffff50");

            stringBuilder.AppendLines(6).AppendClr("* Press Enter to join or create a troop.", "ffffff50").AppendLine();
        }

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Option1:
                GameInterfaceService.JoinTroopQueue();
                UpdateViewScreen();
                break;
            case EKeyboardButton.Option2:
                GameInterfaceService.Queue = "DEFAULT";
                UpdateViewScreen();
                break;
            case EKeyboardButton.Option3:
                GameInterfaceService.LeaveTroop();
                UpdateViewScreen();
                break;
            case EKeyboardButton.Enter:
                (bool isSuccessful, string failureMessage) joinTroop = GameInterfaceService.JoinTroop(_textInputHandler.Text);
                _failureMessage = joinTroop.failureMessage;
                UpdateViewScreen();
                break;
            case EKeyboardButton.Back:
                ShowView<GameSettingsView>();
                break;
            default:
                if (!GameInterfaceService.IsInTroop && _textInputHandler.HandleButtonPress(pressedButton)) {
                    if (_textInputHandler.Text.Length > Constants.MaxTroopNameLength)
                        _textInputHandler.Text = _textInputHandler.Text[..Constants.MaxTroopNameLength];

                    UpdateViewScreen();
                }
                break;
        }
    }
}