using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ComputerInterface.Views.GameSettings
{
    internal class RV_FusionCallbacks : MonoBehaviour, INetworkRunnerCallbacks
    {
        public RoomView roomView;

        public void Start()
        {
            if (NetworkSystem.Instance.TryGetComponent(out NetworkSystemFusion fusion))
            {
                fusion.runner.AddCallbacks(this);
            }

            NetworkSystem.Instance.OnMultiplayerStarted += (Action) OnJoinedRoom;
            NetworkSystem.Instance.OnReturnedToSinglePlayer += (Action) OnLeftRoom;
        }

        public void OnDestroy()
        {
            if (NetworkSystem.Instance.TryGetComponent(out NetworkSystemFusion fusion))
            {
                fusion.runner.RemoveCallbacks(this);
            }

            NetworkSystem.Instance.OnMultiplayerStarted -= (Action) OnJoinedRoom;
            NetworkSystem.Instance.OnReturnedToSinglePlayer -= (Action) OnLeftRoom;
        }

        public void OnJoinedRoom() => roomView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.InGame);
        public void OnLeftRoom() => roomView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.Idle);

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {

        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {

        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {

        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {

        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            roomView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.Idle);
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
            roomView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.Idle);
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
            roomView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.Idle);
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {

        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {

        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {

        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {

        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {

        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            roomView.Redraw(useTemporaryState: true, temporaryState: NetSystemState.Idle);
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {

        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {

        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {

        }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            throw new NotImplementedException();
        }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
        {
            throw new NotImplementedException();
        }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
        {
            throw new NotImplementedException();
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
        {
            throw new NotImplementedException();
        }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
        {
            throw new NotImplementedException();
        }
    }
}
