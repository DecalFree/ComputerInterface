using ComputerInterface.Extensions;
using ComputerInterface.ViewLib;
using GorillaNetworking;
using System.Text;
using System.Threading.Tasks;

namespace ComputerInterface.Views.GameSettings
{
    public class RedemptionView : ComputerView
    {
        private readonly UITextInputHandler _textInputHandler;

        public RedemptionView()
        {
            _textInputHandler = new UITextInputHandler();
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            _textInputHandler.Text = string.Empty;
            Redraw();
        }

        private void Redraw()
        {
            StringBuilder str = new();

            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.BeginCenter().Append("Redeem Tab").AppendLine();

            bool showState = true;

            if (_textInputHandler.Text == string.Empty)
            {
                showState = false;
            }

            if (showState)
            {
                switch (BaseGameInterface.GetRedemptionStatus())
                {
                    case GorillaComputer.RedemptionResult.Invalid:
                        str.AppendClr("Invalid Code", "ffffff50").EndAlign().AppendLine();
                        break;
                    case GorillaComputer.RedemptionResult.Checking:
                        str.AppendClr("Validating Code", "ffffff50").EndAlign().AppendLine();
                        break;
                    case GorillaComputer.RedemptionResult.AlreadyUsed:
                        str.AppendClr("Code Already Claimed", "ffffff50").EndAlign().AppendLine();
                        break;
                    case GorillaComputer.RedemptionResult.Success:
                        str.AppendClr("Successfully Claimed Code", "ffffff50").EndAlign().AppendLine();
                        break;
                    case GorillaComputer.RedemptionResult.Empty:
                        showState = false;
                        break;
                }
            }

            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.AppendLine().EndAlign();

            str.BeginColor("ffffff50").Append("> ").EndColor().Append(_textInputHandler.Text).AppendClr("_", "ffffff50");
            str.AppendLines(2).BeginColor("ffffff50").Append("* ").EndColor().Append("Press Enter to redeem code.");

            Text = str.ToString();
        }

        public override async void OnKeyPressed(EKeyboardKey key)
        {
            if (_textInputHandler.HandleKey(key))
            {
                if (_textInputHandler.Text.Length > BaseGameInterface.MAX_CODE_LENGTH)
                {
                    _textInputHandler.Text = _textInputHandler.Text[..BaseGameInterface.MAX_CODE_LENGTH];
                }

                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Enter:
                    if (_textInputHandler.Text != "")
                    {
                        if (_textInputHandler.Text.Length < 8)
                        {
                            BaseGameInterface.SetRedemptionStatus(GorillaComputer.RedemptionResult.Invalid);
                            return;
                        }
                        CodeRedemption.Instance.HandleCodeRedemption(_textInputHandler.Text);
                        BaseGameInterface.SetRedemptionStatus(GorillaComputer.RedemptionResult.Checking);
                    }
                    else if (BaseGameInterface.GetRedemptionStatus() != GorillaComputer.RedemptionResult.Success)
                    {
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
}
