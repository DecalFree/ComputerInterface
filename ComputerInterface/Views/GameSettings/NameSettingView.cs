using System.Text;
using ComputerInterface.Behaviors;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;

namespace ComputerInterface.Views.GameSettings;

internal class NameSettingView : ComputerView {
    private readonly UITextInputHandler _textInputHandler = new();

    private bool ShowFailureMessage => _failureMessage != null && _textInputHandler.Text != GameInterfaceService.Computer.currentName;
    private string _failureMessage;

    public override void OnViewShown(object[] arguments) => _textInputHandler.Text = GameInterfaceService.GetPeerName();

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Name Tab").AppendLine();

        bool showState = true;

        if (_textInputHandler.Text == GameInterfaceService.Computer.savedName) {
            stringBuilder.AppendClr("Name Synchronized", "ffffff50").EndAlign().AppendLine();
            showState = false;
        }

        if (showState) {
            switch (ShowFailureMessage) {
                case true:
                    stringBuilder.AppendClr(_failureMessage, "ffffff50").AppendLine();
                    break;
                case false:
                    if (_textInputHandler.Text != GameInterfaceService.Computer.currentName)
                        stringBuilder.AppendClr("Ready - Enter to Update", "ffffff50").AppendLine();
                    break;
            }
        }

        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        stringBuilder.BeginColor("ffffff50").Append("> ").EndColor().Append(_textInputHandler.Text).AppendClr("_", "ffffff50");
        stringBuilder.AppendLines(2).AppendLine($"Nametags: {(GameInterfaceService.Nametags ? "Enabled" : "Disabled")}");

        stringBuilder.AppendLines(2).AppendClr("* Press Enter to change your name.", "ffffff50").AppendLine();
        stringBuilder.AppendClr("* Press Option 1 to toggle nametags.", "ffffff50").AppendLine();

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Enter:
                if (GameInterfaceService.Nametags) {
                    (bool isSuccessful, string failureMessage) setPeerName = GameInterfaceService.SetPeerName(_textInputHandler.Text);
                    _failureMessage = setPeerName.failureMessage;
                    UpdateViewScreen();
                }
                break;
            case EKeyboardButton.Option1:
                GameInterfaceService.Nametags = !GameInterfaceService.Nametags;
                UpdateViewScreen();
                break;
            case EKeyboardButton.Back:
                _textInputHandler.Text = GameInterfaceService.GetPeerName();
                ShowView<GameSettingsView>();
                break;
            default:
                if (GameInterfaceService.Nametags && _textInputHandler.HandleButtonPress(pressedButton)) {
                    if (_textInputHandler.Text.Length > Constants.MaxPeerNameLength)
                        _textInputHandler.Text = _textInputHandler.Text[..Constants.MaxPeerNameLength];

                    UpdateViewScreen();
                }
                break;
        }
    }
}