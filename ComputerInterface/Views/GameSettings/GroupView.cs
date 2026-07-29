using System.Text;
using ComputerInterface.Behaviors;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;

namespace ComputerInterface.Views.GameSettings;

internal class GroupView : ComputerView {
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardButton.Up, EKeyboardButton.Down) {
        MaxIndex = GameInterfaceService.AllowedMapsToJoin.Length - 1,
        CurrentSelectionIndex = 0
    };

    public GroupView() => _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Group Tab").AppendLine();
        stringBuilder.AppendClr("Option 1 for more info", "ffffff50").EndColor().AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        stringBuilder.AppendLine("Available maps: ");
        string[] maps = GameInterfaceService.AllowedMapsToJoin;
        for (int i = 0; i < maps.Length; i++) {
            string formattedName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(maps[i]);
            stringBuilder.Append(_selectionHandler.GetIndicatedText(i, formattedName)).AppendLine();
        }

        stringBuilder.AppendLines(1).AppendClr("* Press Enter to group join.", "ffffff50").AppendLine();

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Enter:
                GameInterfaceService.JoinGroupMap(_selectionHandler.CurrentSelectionIndex);
                ShowView<SessionView>();
                break;
            case EKeyboardButton.Option1:
                ShowView<GroupInfoView>();
                break;
            case EKeyboardButton.Back:
                ShowView<GameSettingsView>();
                break;
            default:
                if (_selectionHandler.HandleButtonPress(pressedButton))
                    UpdateViewScreen();
                break;
        }
    }
}