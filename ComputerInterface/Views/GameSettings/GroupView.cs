using ComputerInterface.Extensions;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;

namespace ComputerInterface.Views.GameSettings;

internal class GroupView : ComputerView {
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardKey.Up, EKeyboardKey.Down);

    public GroupView() =>
        _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}> ></color> ", "", "   ", "");

    public override void OnShow(object[] args) {
        base.OnShow(args);

        _selectionHandler.MaxIdx = BaseGameInterface.GetGroupJoinMaps().Length - 1;
        _selectionHandler.CurrentSelectionIndex = 0;
        Redraw();
    }

    private void Join() {
        BaseGameInterface.JoinGroupMap(_selectionHandler.CurrentSelectionIndex);
        ShowView<RoomView>();
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();

        DrawHeader(stringBuilder);
        DrawOptions(stringBuilder);

        SetText(stringBuilder);
    }

    private void DrawHeader(StringBuilder stringBuilder) {
        stringBuilder.BeginCenter().BeginColor("ffffff50").Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Press enter to join").AppendLine();
        stringBuilder.Append("Option 1 for more info").AppendLine();
        stringBuilder.Repeat("=", ScreenWidth).EndColor().EndAlign().AppendLines(2);
    }

    private void DrawOptions(StringBuilder stringBuilder) {
        stringBuilder.AppendLine("Available maps: ");
        var maps = BaseGameInterface.GetGroupJoinMaps();
        for (var i = 0; i < maps.Length; i++) {
            var formattedName = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(maps[i]);
            stringBuilder.Append(_selectionHandler.GetIndicatedText(i, formattedName)).AppendLine();
        }
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        switch (key) {
            case EKeyboardKey.Enter:
                Join();
                break;
            case EKeyboardKey.Option1:
                ShowView<GroupInfoView>();
                break;
            case EKeyboardKey.Back:
                ShowView<GameSettingsView>();
                break;
            default:
                if (_selectionHandler.HandleKeypress(key))
                    Redraw();
                break;
        }
    }
}