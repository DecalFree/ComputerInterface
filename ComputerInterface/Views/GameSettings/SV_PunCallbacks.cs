using System;
using Photon.Pun;

namespace ComputerInterface.Views.GameSettings;

internal class SV_PunCallbacks : MonoBehaviourPunCallbacks {
    public SessionView SessionView;

    public void Start() {
        NetworkSystem.Instance.OnMultiplayerStarted += (Action) OnJoinedRoom;
        NetworkSystem.Instance.OnReturnedToSinglePlayer += (Action) OnLeftRoom;
    }

    public void OnDestroy() {
        NetworkSystem.Instance.OnMultiplayerStarted -= (Action) OnJoinedRoom;
        NetworkSystem.Instance.OnReturnedToSinglePlayer -= (Action) OnLeftRoom;
    }

    public override void OnConnected() => SessionView.Redraw();

    public override void OnLeftRoom() => SessionView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.Idle);

    public override void OnCreateRoomFailed(short returnCode, string message) => SessionView.Redraw();

    public override void OnJoinRoomFailed(short returnCode, string message) => SessionView.Redraw();

    public override void OnCreatedRoom() => SessionView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.InGame);

    public override void OnJoinedRoom() => SessionView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.InGame);

    public override void OnJoinRandomFailed(short returnCode, string message) => SessionView.Redraw();

    public override void OnConnectedToMaster() => SessionView.Redraw();
}