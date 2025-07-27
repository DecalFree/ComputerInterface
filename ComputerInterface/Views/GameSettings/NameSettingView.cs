using ComputerInterface.Extensions;
using ComputerInterface.ViewLib;
using System.Text;

namespace ComputerInterface.Views.GameSettings
{
    public class NameSettingView : ComputerView
    {
        private readonly UITextInputHandler _textInputHandler;
        private BaseGameInterface.WordCheckResult _wordCheckResult;

        public NameSettingView()
        {
            _textInputHandler = new UITextInputHandler();
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            _textInputHandler.Text = BaseGameInterface.GetName();

            Redraw();
        }

        private void Redraw()
        {
            BaseGameInterface.CheckForComputer(out var computer);

            StringBuilder str = new();

            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.BeginCenter().Append("Name Tab").AppendLine();

            bool showState = true;

            if (_textInputHandler.Text == computer.savedName)
            {
                str.AppendClr("Name Synchronized", "ffffff50").EndAlign().AppendLine();
                showState = false;
            }

            if (showState)
            {
                switch (_wordCheckResult)
                {
                    case BaseGameInterface.WordCheckResult.Allowed:
                        str.AppendClr("Ready - Enter to Update", "ffffff50").EndAlign().AppendLine();
                        break;
                    case BaseGameInterface.WordCheckResult.Blank:
                        str.AppendClr("Error - Name is Blank", "ffffff50").EndAlign().AppendLine();
                        break;
                    case BaseGameInterface.WordCheckResult.Empty:
                        str.AppendClr("Error - Name is Empty", "ffffff50").EndAlign().AppendLine();
                        break;
                    case BaseGameInterface.WordCheckResult.TooLong:
                        str.AppendClr("Error - Name Exceeds Character Limit", "ffffff50").EndAlign().AppendLine();
                        break;
                    case BaseGameInterface.WordCheckResult.NotAllowed:
                        str.AppendClr("Error - Name Inappropriate", "ffffff50").EndAlign().AppendLine();
                        break;
                }
            }

            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.AppendLine();

            str.BeginColor("ffffff50").Append("> ").EndColor().Append(_textInputHandler.Text).AppendClr("_", "ffffff50");
            str.AppendLines(2).AppendLine($"Nametags: {(BaseGameInterface.GetNametagsEnabled() ? "Enabled" : "Disabled")}");
            
            str.AppendLines(2).BeginColor("ffffff50").Append("* ").EndColor().Append("Press Enter to change your name.");
            str.AppendLine().BeginColor("ffffff50").Append("* ").EndColor().Append("Press Option 1 to toggle nametags.");

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (BaseGameInterface.GetNametagsEnabled() && _textInputHandler.HandleKey(key))
            {
                if (_textInputHandler.Text.Length > BaseGameInterface.MAX_NAME_LENGTH)
                {
                    _textInputHandler.Text = _textInputHandler.Text[..BaseGameInterface.MAX_NAME_LENGTH];
                }

                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Enter:
                    if (BaseGameInterface.GetNametagsEnabled())
                    {
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
}