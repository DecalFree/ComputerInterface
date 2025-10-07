using ComputerInterface.Extensions;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;

namespace ComputerInterface.Views.GameSettings;

public class NameSettingView : ComputerView {
    private readonly UITextInputHandler _textInputHandler = new();
    private EWordCheckResult _wordCheckResult;

    public override void OnShow(object[] args) {
        base.OnShow(args);
        _textInputHandler.Text = BaseGameInterface.GetName();

        Redraw();
    }

    private void Redraw() {
        BaseGameInterface.CheckForComputer(out var computer);

        var stringBuilder = new StringBuilder();

        stringBuilder.Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.BeginCenter().Append("Name Tab").AppendLine();

        var showState = true;

        if (_textInputHandler.Text == computer.savedName) {
            stringBuilder.AppendClr("Name Synchronized", "ffffff50").EndAlign().AppendLine();
            showState = false;
        }

        if (showState) {
            switch (_wordCheckResult) {
                case EWordCheckResult.Allowed:
                    stringBuilder.AppendClr("Ready - Enter to Update", "ffffff50").EndAlign().AppendLine();
                    break;
                case EWordCheckResult.Blank:
                    stringBuilder.AppendClr("Error - Name is Blank", "ffffff50").EndAlign().AppendLine();
                    break;
                case EWordCheckResult.Empty:
                    stringBuilder.AppendClr("Error - Name is Empty", "ffffff50").EndAlign().AppendLine();
                    break;
                case EWordCheckResult.TooLong:
                    stringBuilder.AppendClr("Error - Name Exceeds Character Limit", "ffffff50").EndAlign().AppendLine();
                    break;
                case EWordCheckResult.NotAllowed:
                    stringBuilder.AppendClr("Error - Name Inappropriate", "ffffff50").EndAlign().AppendLine();
                    break;
            }
        }

        stringBuilder.Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.AppendLine();

        stringBuilder.BeginColor("ffffff50").Append("> ").EndColor().Append(_textInputHandler.Text).AppendClr("_", "ffffff50");
        stringBuilder.AppendLines(2).AppendLine($"Nametags: {(BaseGameInterface.GetNametagsEnabled() ? "Enabled" : "Disabled")}");
            
        stringBuilder.AppendLines(2).BeginColor("ffffff50").Append("* ").EndColor().Append("Press Enter to change your name.");
        stringBuilder.AppendLine().BeginColor("ffffff50").Append("* ").EndColor().Append("Press Option 1 to toggle nametags.");

        Text = stringBuilder.ToString();
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        if (BaseGameInterface.GetNametagsEnabled() && _textInputHandler.HandleKey(key)) {
            if (_textInputHandler.Text.Length > BaseGameInterface.MaxNameLength)
                _textInputHandler.Text = _textInputHandler.Text[..BaseGameInterface.MaxNameLength];

            Redraw();
            return;
        }

        switch (key) {
            case EKeyboardKey.Enter:
                if (BaseGameInterface.GetNametagsEnabled()) {
                    _wordCheckResult = BaseGameInterface.SetName(_textInputHandler.Text);
                    Redraw();
                }
                break;
            case EKeyboardKey.Option1:
                BaseGameInterface.SetNametagSetting(!BaseGameInterface.GetNametagsEnabled());
                Redraw();
                break;
            case EKeyboardKey.Back:
                _textInputHandler.Text = BaseGameInterface.GetName();
                ShowView<GameSettingsView>();
                break;
        }
    }
}