using ComputerInterface.Extensions;
using GorillaNetworking;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings;

public class RoomView : ComputerView {
    private readonly UITextInputHandler _textInputHandler = new();
    private GameObject _callbacks;
    private string _joinedRoom, _statusLabel;

    public override void OnShow(object[] args) {
        base.OnShow(args);

        _callbacks = new GameObject("RoomCallbacks");
        Object.DontDestroyOnLoad(_callbacks);

        if (NetworkSystem.Instance.GetComponent<NetworkSystemPUN>()) {
            var callbacksComponent = _callbacks.AddComponent<RV_PunCallbacks>();
            callbacksComponent.roomView = this;
        }
        else if (NetworkSystem.Instance.GetComponent<NetworkSystemFusion>()) {
            var callbacksComponent = _callbacks.AddComponent<RV_FusionCallbacks>();
            callbacksComponent.roomView = this;
        }

        Redraw();
    }

    public void Redraw(bool useTemporaryState = false, NetSystemState temporaryState = NetSystemState.Initialization) {
        var stringBuilder = new StringBuilder();

        stringBuilder.Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.BeginCenter().Append("Room Tab").AppendLine();

        var showState = true;

        if (GorillaComputer.instance.roomFull) {
            stringBuilder.AppendClr("Room Full", "ffffff50").EndAlign().AppendLine();
            showState = false;
        }

        if (GorillaComputer.instance.roomNotAllowed) {
            stringBuilder.AppendClr("Room Prohibited", "ffffff50").EndAlign().AppendLine();
            showState = false;
        }

        if (NetworkSystem.Instance.WrongVersion) {
            stringBuilder.AppendClr("Servers Prohibited", "ffffff50").EndAlign().AppendLine();
            showState = false;
        }

        if (showState) {
            var netState = useTemporaryState ? temporaryState : GetConnectionState();
            var text = netState switch {
                NetSystemState.Initialization => "Initialization",
                NetSystemState.PingRecon => "Reconnecting",
                NetSystemState.Idle => "Connected - Enter to Join",
                NetSystemState.Connecting => "Joining Room",
                NetSystemState.InGame => $"In Room {BaseGameInterface.GetRoomCode()}",
                NetSystemState.Disconnecting => "Leaving Room",
                _ => throw new System.ArgumentOutOfRangeException()
            };

            _statusLabel = text != "None" ? text : _statusLabel;
            text = text == "None" ? _statusLabel : text;

            stringBuilder.AppendClr(text, "ffffff50").EndAlign().AppendLine();
        }

        stringBuilder.Repeat("=", ScreenWidth).AppendLine();
        stringBuilder.AppendLine();
        stringBuilder.BeginColor("ffffff50").Append("> ").EndColor().Append(_textInputHandler.Text)
            .AppendClr("_", "ffffff50");
        stringBuilder.AppendLines(2).BeginColor("ffffff50").Append("* ").EndColor()
            .AppendLine("Press Enter to join or create a custom room.");
        stringBuilder.AppendLine().BeginColor("ffffff50").Append("* ").EndColor()
            .AppendLine("Press Option 1 to disconnect from the current room.");

        Text = stringBuilder.ToString();
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        switch (key) {
            case EKeyboardKey.Back:
                Object.Destroy(_callbacks);
                ShowView<GameSettingsView>();
                break;
            case EKeyboardKey.Enter:
                _joinedRoom = _textInputHandler.Text.ToUpper();
                GorillaComputer.instance.roomFull = false;
                GorillaComputer.instance.roomNotAllowed = false;
                BaseGameInterface.JoinRoom(_joinedRoom);
                Redraw();
                break;
            case EKeyboardKey.Option1:
                if (NetworkSystem.Instance.InRoom)
                    BaseGameInterface.Disconnect();
                break;
            default:
                if (_textInputHandler.HandleKey(key)) {
                    if (_textInputHandler.Text.Length > BaseGameInterface.MaxRoomLength)
                        _textInputHandler.Text = _textInputHandler.Text[..BaseGameInterface.MaxRoomLength];

                    Redraw();
                }

                break;
        }
    }

    private NetSystemState GetConnectionState() {
        var networkSystem = NetworkSystem.Instance;
        return networkSystem.netState;
    }
}