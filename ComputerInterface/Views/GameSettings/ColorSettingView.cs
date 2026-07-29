using System.Text;
using ComputerInterface.Behaviors;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings;

internal class ColorSettingView : ComputerView {
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardButton.Up, EKeyboardButton.Down) {
        MaxIndex = 2
    };
    private readonly UISelectionHandler _columnSelectionHandler = new(EKeyboardButton.Left, EKeyboardButton.Right) {
        MaxIndex = 2
    };

    private Color _peerColor;
    private Color _savedPeerColor;

    private string _redString = "255";
    private string _greenString = "255";
    private string _blueString = "255";

    private void DrawValue(StringBuilder stringBuilder, string value, int lineNum) {
        for (int i = 0; i < 3; i++) {
            if (_columnSelectionHandler.CurrentSelectionIndex == i && lineNum == _selectionHandler.CurrentSelectionIndex) {
                stringBuilder.BeginColor(PrimaryColor).Append(value[i]).EndColor();
                continue;
            }

            stringBuilder.Append(value[i]);
        }
    }

    private void SetValOnString(ref string str, int column, char chr) {
        char[] ch = str.ToCharArray();
        ch[column] = chr;
        str = new string(ch);
    }

    public override void OnViewShown(object[] arguments) {
        _peerColor = GameInterfaceService.PeerColor;

        _redString = Mathf.RoundToInt(_peerColor.r * 255).ToString().PadLeft(3, '0');
        _greenString = Mathf.RoundToInt(_peerColor.g * 255).ToString().PadLeft(3, '0');
        _blueString = Mathf.RoundToInt(_peerColor.b * 255).ToString().PadLeft(3, '0');

        _savedPeerColor = _peerColor;
    }

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginCenter().BeginColor(_peerColor).Repeat("=", ScreenWidth).EndColor().AppendLine();
        stringBuilder.Append("Color Tab").AppendLine();
        stringBuilder.AppendClr("Values are from 0 - 255", "ffffff50").AppendLine();
        stringBuilder.BeginColor(_peerColor).Repeat("=", ScreenWidth).EndColor().EndAlign().AppendLines(2);

        stringBuilder.AppendClr(" R: ", "ffffff50");
        DrawValue(stringBuilder, _redString, 0);
        stringBuilder.AppendClr($"<size=40>  Current: {Mathf.RoundToInt(_savedPeerColor.r * 255).ToString().PadLeft(3, '0')}</size>", "ffffff50").AppendLine();

        stringBuilder.AppendClr(" G: ", "ffffff50");
        DrawValue(stringBuilder, _greenString, 1);
        stringBuilder.AppendClr($"<size=40>  Current: {Mathf.RoundToInt(_savedPeerColor.g * 255).ToString().PadLeft(3, '0')}</size>", "ffffff50").AppendLine();

        stringBuilder.AppendClr(" B: ", "ffffff50");
        DrawValue(stringBuilder, _blueString, 2);
        stringBuilder.AppendClr($"<size=40>  Current: {Mathf.RoundToInt(_savedPeerColor.b * 255).ToString().PadLeft(3, '0')}</size>", "ffffff50").AppendLine();

        stringBuilder.AppendLines(3).AppendClr("* Press Enter to update your color.", "ffffff50").AppendLine();

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Enter:
                GameInterfaceService.PeerColor = _peerColor;
                _savedPeerColor = _peerColor;
                UpdateViewScreen();
                break;
            case EKeyboardButton.Back:
                ShowView<GameSettingsView>();
                break;
            default:
                if (pressedButton.IsNumberKey()) {
                    int line = _selectionHandler.CurrentSelectionIndex;
                    int column = _columnSelectionHandler.CurrentSelectionIndex;
                    char numChar = pressedButton.ToString()[3..][0];

                    switch (line) {
                        case 0:
                            SetValOnString(ref _redString, column, numChar);
                            break;
                        case 1:
                            SetValOnString(ref _greenString, column, numChar);
                            break;
                        case 2:
                            SetValOnString(ref _blueString, column, numChar);
                            break;
                    }

                    int r = Mathf.Clamp(int.Parse(_redString), 0, 255);
                    int g = Mathf.Clamp(int.Parse(_greenString), 0, 255);
                    int b = Mathf.Clamp(int.Parse(_blueString), 0, 255);

                    _redString = r.ToString().PadLeft(3, '0');
                    _greenString = g.ToString().PadLeft(3, '0');
                    _blueString = b.ToString().PadLeft(3, '0');

                    _peerColor = new Color(r / 255f, g / 255f, b / 255f);
                    _columnSelectionHandler.MoveSelectionDown();
                    UpdateViewScreen();
                    break;
                }
                if (_selectionHandler.HandleButtonPress(pressedButton) || _columnSelectionHandler.HandleButtonPress(pressedButton))
                    UpdateViewScreen();
                break;
        }
    }
}