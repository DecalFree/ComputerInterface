using System.Text;
using ComputerInterface.Behaviors;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Models;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings;

internal class SessionView : ComputerView {
    private readonly UITextInputHandler _textInputHandler = new();

    private GameObject _callbacksObject;

    private string _joinedSession;
    private string _statusLabel;

    private bool _useTemporaryState;
    private NetSystemState _temporaryState;

    public void Redraw(bool useTemporaryState = false, NetSystemState temporaryState = NetSystemState.Initialization) {
        _useTemporaryState = useTemporaryState;
        _temporaryState = temporaryState;
        UpdateViewScreen();
    }

    public override void OnViewShown(object[] arguments) {
        _callbacksObject = new GameObject("RoomCallbacks");
        Object.DontDestroyOnLoad(_callbacksObject);

        // I genuinely doubt Fusion will be used in the future, so I'll just keep it like this for now. -DecalFree
        if (NetworkSystem.Instance.GetComponent<NetworkSystemPUN>()) {
            SV_PunCallbacks callbacksComponent = _callbacksObject.AddComponent<SV_PunCallbacks>();
            callbacksComponent.SessionView = this;
        }
    }

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginCenter().Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.Append("Room Tab").AppendLine();

        bool showState = true;

        if (GameInterfaceService.Computer.roomFull) {
            stringBuilder.AppendClr("Room Full", "ffffff50").AppendLine();
            showState = false;
        }

        if (GameInterfaceService.Computer.roomNotAllowed) {
            stringBuilder.AppendClr("Room Prohibited", "ffffff50").AppendLine();
            showState = false;
        }

        if (NetworkSystem.Instance.WrongVersion) {
            stringBuilder.AppendClr("Servers Prohibited", "ffffff50").AppendLine();
            showState = false;
        }

        if (showState) {
            NetSystemState netState = _useTemporaryState ? _temporaryState : NetworkSystem.Instance.netState;
            string text = netState switch {
                NetSystemState.Initialization => "Initialization",
                NetSystemState.PingRecon => "Reconnecting",
                NetSystemState.Idle => "Connected - Enter to Join",
                NetSystemState.Connecting => "Joining Room",
                NetSystemState.InGame => $"In Room {GameInterfaceService.GetSessionName()}",
                NetSystemState.Disconnecting => "Leaving Room",
                _ => throw new System.ArgumentOutOfRangeException()
            };

            _statusLabel = text != "None" ? text : _statusLabel;
            text = text == "None" ? _statusLabel : text;

            stringBuilder.AppendClr(text, "ffffff50").AppendLine();
        }

        stringBuilder.Repeat("=", ScreenWidth).EndAlign().AppendLines(2);

        stringBuilder.BeginColor("ffffff50").Append("> ").EndColor().Append(_textInputHandler.Text).AppendClr("_", "ffffff50");

        stringBuilder.AppendLines(5).AppendClr("* Press Enter to join or create a custom room.", "ffffff50").AppendLine();
        stringBuilder.AppendClr("* Press Option 1 to disconnect from the current room.", "ffffff50").AppendLine();

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Back:
                Object.Destroy(_callbacksObject);
                ShowView<GameSettingsView>();
                break;
            case EKeyboardButton.Enter:
                _joinedSession = _textInputHandler.Text.ToUpper();
                GameInterfaceService.Computer.roomFull = false;
                GameInterfaceService.Computer.roomNotAllowed = false;
                GameInterfaceService.JoinSession(_joinedSession);
                UpdateViewScreen();
                break;
            case EKeyboardButton.Option1:
                if (NetworkSystem.Instance.InRoom)
                    GameInterfaceService.ReturnToSinglePlayer();
                break;
            default:
                if (_textInputHandler.HandleButtonPress(pressedButton)) {
                    if (_textInputHandler.Text.Length > Constants.MaxSessionNameLength)
                        _textInputHandler.Text = _textInputHandler.Text[..Constants.MaxSessionNameLength];

                    UpdateViewScreen();
                }
                break;
        }
    }
}