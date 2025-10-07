using ComputerInterface.Extensions;
using System.Text;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;

namespace ComputerInterface.Views.GameSettings;

internal class GroupInfoView : ComputerView {
    public override void OnShow(object[] args) {
        base.OnShow(args);
        SetText(Redraw);
    }

    private void Redraw(StringBuilder stringBuilder) {
        stringBuilder.BeginColor("ffffff50").Append("== ").EndColor();
        stringBuilder.Append("Group Info").BeginColor("ffffff50").Append(" ==").EndColor().AppendLines(2);
        stringBuilder.AppendLine("1. Create/Join a Private room").AppendLine();
        stringBuilder.AppendLine("2. Select a map in the Group tab").AppendLine();
        stringBuilder.AppendLine("3. Gather everyone near the computer").AppendLine();
        stringBuilder.AppendLine("4. Make sure everyone is on the same gamemode").AppendLine();
        stringBuilder.AppendLine("5. Press the Enter key").AppendLine();
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        switch (key) {
            case EKeyboardKey.Back:
                ShowView<GroupView>();
                break;
        }
    }
}