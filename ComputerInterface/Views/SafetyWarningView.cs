using System.Text;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;

namespace ComputerInterface.Views;

internal class SafetyWarningView : ComputerView {
    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginColor("ffffff50").Append("== ").EndColor();
        stringBuilder.Append("Safety Warning").BeginColor("ffffff50").Append(" ==").EndColor().AppendLines(2);

        stringBuilder.Append("There have been recent CI fixes that have things like Console in them.").AppendLines(2);
        stringBuilder.AppendLine("The only safe place to get CI is at github.com/DecalFree/ComputerInterface, or through trusted Mod Managers recommended by the GTMG.");

        stringBuilder.AppendLines(2).AppendClr("* Press Enter to acknowledge that you understand these safety concerns.", "ffffff50").AppendLine();

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Enter:
                Plugin.CIConfig.AcknowledgedSafetyWarning.Value = true;
                ReturnToMainMenu();
                break;
        }
    }
}