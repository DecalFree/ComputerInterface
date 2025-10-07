using ComputerInterface.Extensions;
using GorillaNetworking;
using System.Text;
using System.Threading.Tasks;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;

namespace ComputerInterface.Views.GameSettings;

public class RedemptionView : ComputerView {
    private readonly UITextInputHandler _textInputHandler = new();

    public override void OnShow(object[] args) {
        base.OnShow(args);

        _textInputHandler.Text = string.Empty;
        Redraw();
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();

        stringBuilder.Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.BeginCenter().Append("Redeem Tab").AppendLine();

        var showState = _textInputHandler.Text != string.Empty;

        if (showState) {
            switch (BaseGameInterface.GetRedemptionStatus()) {
                case GorillaComputer.RedemptionResult.Invalid:
                    stringBuilder.AppendClr("Invalid Code", "ffffff50").EndAlign().AppendLine();
                    break;
                case GorillaComputer.RedemptionResult.Checking:
                    stringBuilder.AppendClr("Validating Code", "ffffff50").EndAlign().AppendLine();
                    break;
                case GorillaComputer.RedemptionResult.AlreadyUsed:
                    stringBuilder.AppendClr("Code Already Claimed", "ffffff50").EndAlign().AppendLine();
                    break;
                case GorillaComputer.RedemptionResult.Success:
                    stringBuilder.AppendClr("Successfully Claimed Code", "ffffff50").EndAlign().AppendLine();
                    break;
                case GorillaComputer.RedemptionResult.Empty:
                    showState = false;
                    break;
            }
        }

        stringBuilder.Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.AppendLine().EndAlign();

        stringBuilder.BeginColor("ffffff50").Append("> ").EndColor().Append(_textInputHandler.Text).AppendClr("_", "ffffff50");
        stringBuilder.AppendLines(2).BeginColor("ffffff50").Append("* ").EndColor().Append("Press Enter to redeem code.");

        Text = stringBuilder.ToString();
    }

    public override async void OnKeyPressed(EKeyboardKey key) {
        if (_textInputHandler.HandleKey(key)) {
            if (_textInputHandler.Text.Length > BaseGameInterface.MaxCodeLength)
                _textInputHandler.Text = _textInputHandler.Text[..BaseGameInterface.MaxCodeLength];

            Redraw();
            return;
        }

        switch (key) {
            case EKeyboardKey.Enter:
                if (_textInputHandler.Text != "") {
                    if (_textInputHandler.Text.Length < 8) {
                        BaseGameInterface.SetRedemptionStatus(GorillaComputer.RedemptionResult.Invalid);
                        return;
                    }
                    CodeRedemption.Instance.HandleCodeRedemption(_textInputHandler.Text);
                    BaseGameInterface.SetRedemptionStatus(GorillaComputer.RedemptionResult.Checking);
                }
                else if (BaseGameInterface.GetRedemptionStatus() != GorillaComputer.RedemptionResult.Success) {
                    BaseGameInterface.SetRedemptionStatus(GorillaComputer.RedemptionResult.Empty);
                }
                Redraw();
                await Task.Delay(600); // Wait 0.6 seconds for the computer to fully register the code inputted and show the correct state.
                Redraw();
                break;
            case EKeyboardKey.Back:
                _textInputHandler.Text = string.Empty;
                BaseGameInterface.SetRedemptionStatus(GorillaComputer.RedemptionResult.Empty);
                ShowView<GameSettingsView>();
                break;
        }
    }
}