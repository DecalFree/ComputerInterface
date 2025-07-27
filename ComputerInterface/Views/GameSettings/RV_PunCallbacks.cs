using System;
using Photon.Pun;

namespace ComputerInterface.Views.GameSettings
{
    internal class RV_PunCallbacks : MonoBehaviourPunCallbacks
    {
        public RoomView roomView;

        public void Start()
        {
            NetworkSystem.Instance.OnMultiplayerStarted += (Action) OnJoinedRoom;
            NetworkSystem.Instance.OnReturnedToSinglePlayer += (Action) OnLeftRoom;
        }

        public void OnDestroy()
        {
            NetworkSystem.Instance.OnMultiplayerStarted -= (Action) OnJoinedRoom;
            NetworkSystem.Instance.OnReturnedToSinglePlayer -= (Action) OnLeftRoom;
        }

        public override void OnJoinedRoom() => roomView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.InGame);
        public override void OnCreatedRoom() => roomView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.InGame);
        public override void OnLeftRoom() => roomView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.Idle);
        public override void OnJoinRoomFailed(short returnCode, string message) => roomView.Redraw();
        public override void OnJoinRandomFailed(short returnCode, string message) => roomView.Redraw();
        public override void OnCreateRoomFailed(short returnCode, string message) => roomView.Redraw();
        public override void OnConnected() => roomView.Redraw();
        public override void OnConnectedToMaster() => roomView.Redraw();
    }
}
