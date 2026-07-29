using System.Text;
using System.Threading.Tasks;
using ComputerInterface.Behaviors;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;
using GorillaNetworking;
using UnityEngine.PlayerLoop;

namespace ComputerInterface.Views.GameSettings;

internal class RedemptionView : ComputerView {
    private readonly UITextInputHandler _textInputHandler = new();

    public override void OnViewShown(object[] arguments) => _textInputHandler.Text = string.Empty;

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Redemption Tab").AppendLine();

        switch (GameInterfaceService.RedemptionStatus) {
            case GorillaComputer.RedemptionResult.Invalid:
                stringBuilder.AppendClr("Invalid Code", "ffffff50").AppendLine();
                break;
            case GorillaComputer.RedemptionResult.Checking:
                stringBuilder.AppendClr("Validating Code", "ffffff50").AppendLine();
                break;
            case GorillaComputer.RedemptionResult.AlreadyUsed:
                stringBuilder.AppendClr("Code Already Claimed", "ffffff50").AppendLine();
                break;
            case GorillaComputer.RedemptionResult.Success:
                stringBuilder.AppendClr("Successfully Claimed Code", "ffffff50").AppendLine();
                break;
            case GorillaComputer.RedemptionResult.Empty:
                stringBuilder.AppendClr("Ready - Enter to Redeem", "ffffff50").AppendLine();
                break;
        }

        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLine();

        stringBuilder.AppendLines(1).BeginColor("ffffff50").Append("> ").EndColor().Append(_textInputHandler.Text).AppendClr("_", "ffffff50");

        stringBuilder.AppendLines(6).AppendClr("* Press Enter to redeem code.", "ffffff50").AppendLine();

        return stringBuilder.ToString();
    }

    public override async void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Enter:
                if (_textInputHandler.Text != "") {
                    if (_textInputHandler.Text.Length < Constants.MaxRedemptionCodeLength) {
                        GameInterfaceService.RedemptionStatus = GorillaComputer.RedemptionResult.Invalid;
                        return;
                    }
                    CodeRedemption.Instance.HandleCodeRedemption(_textInputHandler.Text);
                    GameInterfaceService.RedemptionStatus = GorillaComputer.RedemptionResult.Checking;
                }
                else if (GameInterfaceService.RedemptionStatus != GorillaComputer.RedemptionResult.Success) {
                    GameInterfaceService.RedemptionStatus = GorillaComputer.RedemptionResult.Empty;
                }

                UpdateViewScreen();
                await Task.Delay(600); // Wait 0.6 seconds for the computer to fully register the code inputted and show the correct state. -DecalFree
                UpdateViewScreen();
                break;
            case EKeyboardButton.Back:
                _textInputHandler.Text = string.Empty;
                GameInterfaceService.RedemptionStatus = GorillaComputer.RedemptionResult.Empty;
                ShowView<GameSettingsView>();
                break;
            default:
                if (_textInputHandler.HandleButtonPress(pressedButton)) {
                    if (_textInputHandler.Text.Length > Constants.MaxRedemptionCodeLength)
                        _textInputHandler.Text = _textInputHandler.Text[..Constants.MaxRedemptionCodeLength];

                    UpdateViewScreen();
                }
                break;
        }
    }
}