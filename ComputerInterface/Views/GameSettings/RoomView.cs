using ComputerInterface.Extensions;
using ComputerInterface.ViewLib;
using GorillaNetworking;
using System.Text;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings
{
    public class RoomView : ComputerView
    {
        private readonly UITextInputHandler _textInputHandler;
        private GameObject callbacks;
        private string _joinedRoom, _statusLabel;

        public RoomView()
        {
            _textInputHandler = new UITextInputHandler();
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);

            callbacks = new GameObject("RoomCallbacks");
            Object.DontDestroyOnLoad(callbacks);

            if (NetworkSystem.Instance.GetComponent<NetworkSystemPUN>())
            {
                RV_PunCallbacks calllbacksComponent = callbacks.AddComponent<RV_PunCallbacks>();
                calllbacksComponent.roomView = this;
            }
            else if (NetworkSystem.Instance.GetComponent<NetworkSystemFusion>())
            {
                RV_FusionCallbacks calllbacksComponent = callbacks.AddComponent<RV_FusionCallbacks>();
                calllbacksComponent.roomView = this;
            }

            Redraw();
        }

        public void Redraw(bool useTemporaryState = false, NetSystemState temporaryState = NetSystemState.Initialization)
        {
            StringBuilder str = new();

            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.BeginCenter().Append("Room Tab").AppendLine();

            bool showState = true;

            if (GorillaComputer.instance.roomFull)
            {
                str.AppendClr("Room Full", "ffffff50").EndAlign().AppendLine();
                showState = false;
            }

            if (GorillaComputer.instance.roomNotAllowed)
            {
                str.AppendClr("Room Prohibited", "ffffff50").EndAlign().AppendLine();
                showState = false;
            }

            if (NetworkSystem.Instance.WrongVersion)
            {
                str.AppendClr("Servers Prohibited", "ffffff50").EndAlign().AppendLine();
                showState = false;
            }

            if (showState)
            {
                NetSystemState netState = useTemporaryState ? temporaryState : GetConnectionState();
                string text = netState switch
                {
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

                str.AppendClr(text, "ffffff50").EndAlign().AppendLine();
            }

            str.Repeat("=", SCREEN_WIDTH).AppendLine();
            str.AppendLine();
            str.BeginColor("ffffff50").Append("> ").EndColor().Append(_textInputHandler.Text).AppendClr("_", "ffffff50");
            str.AppendLines(2).BeginColor("ffffff50").Append("* ").EndColor().AppendLine("Press Enter to join or create a custom room.");
            str.AppendLine().BeginColor("ffffff50").Append("* ").EndColor().AppendLine("Press Option 1 to disconnect from the current room.");

            Text = str.ToString();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            switch (key)
            {
                case EKeyboardKey.Back:
                    UnityEngine.Object.Destroy(callbacks);
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
                    {
                        BaseGameInterface.Disconnect();
                    }
                    break;
                default:
                    if (_textInputHandler.HandleKey(key))
                    {
                        if (_textInputHandler.Text.Length > BaseGameInterface.MAX_ROOM_LENGTH)
                        {
                            _textInputHandler.Text = _textInputHandler.Text[..BaseGameInterface.MAX_ROOM_LENGTH];
                        }

                        Redraw();
                        return;
                    }
                    break;
            }
        }

        private NetSystemState GetConnectionState()
        {
            NetworkSystem networkSystem = NetworkSystem.Instance;
            return networkSystem.netState;
        }
    }
}
