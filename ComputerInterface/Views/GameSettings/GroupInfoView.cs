using System.Text;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;

namespace ComputerInterface.Views.GameSettings;

internal class GroupInfoView : ComputerView {
    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginColor("ffffff50").Append("== ").EndColor();
        stringBuilder.Append("Group Info").BeginColor("ffffff50").Append(" ==").EndColor().AppendLines(2);

        stringBuilder.AppendLine("1. Create/Join a Private room").AppendLine();
        stringBuilder.AppendLine("2. Select a map in the Group tab").AppendLine();
        stringBuilder.AppendLine("3. Gather everyone near the computer").AppendLine();
        stringBuilder.AppendLine("4. Make sure everyone is on the same GameMode").AppendLine();
        stringBuilder.AppendLine("5. Press the Enter key").AppendLine();

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Back:
                ShowView<GroupView>();
                break;
        }
    }
}