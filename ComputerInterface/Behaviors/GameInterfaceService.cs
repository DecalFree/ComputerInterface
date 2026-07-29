using System;
using System.Reflection;
using System.Threading.Tasks;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using GorillaNetworking;
using GorillaTagScripts;
using GorillaTagScripts.VirtualStumpCustomMaps;
using Photon.Pun;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace ComputerInterface.Behaviors;

public static class GameInterfaceService {
    public static GorillaComputer Computer => GorillaComputer.instance;

    public static GorillaFriendCollider FriendJoinCollider => Computer.friendJoinCollider;

    public static bool IsPeerInParty => FriendshipGroupDetection.Instance.IsInParty;

    public static bool IsPartyWithinCollider => FriendshipGroupDetection.Instance.IsPartyWithinCollider(FriendJoinCollider);

    public static bool IsPeerInVirtualStump => Computer.IsPlayerInVirtualStump();

    private static bool IsNameAllowed(string name) => Computer.CheckAutoBanListForName(name);

    public static void InitializeNoobMaterial(Color peerColor) {
        if (NetworkSystem.Instance.InRoom)
            GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, peerColor.r, peerColor.g, peerColor.b);
    }

    #region Session Services

    public static async void ReturnToSinglePlayer() {
        if (!NetworkSystem.Instance.InRoom)
            return;

        if (IsPeerInParty) {
            FriendshipGroupDetection.Instance.LeaveParty();
            await Task.Delay(1000);
        }

        await NetworkSystem.Instance.ReturnToSinglePlayer();
    }

    public static (bool isSuccessful, string failureMessage) JoinSession(string sessionName) {
        if ((!IsPeerInVirtualStump && sessionName == "") || (IsPeerInVirtualStump && sessionName.Length == 1))
            return (false, "Input is Empty");

        if (sessionName.Length > Constants.MaxSessionNameLength)
            return (false, "Input Exceeds Character Limit");

        if (!IsNameAllowed(sessionName))
            return (false, "Input is Inappropriate");

        if (IsPeerInParty && !IsPartyWithinCollider)
            FriendshipGroupDetection.Instance.LeaveParty();

        if (IsPeerInVirtualStump)
            CustomMapManager.UnloadMap(false);

        PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(sessionName, IsPeerInParty ? JoinType.JoinWithParty : JoinType.Solo);

        return (true, null);
    }

    public static string GetSessionName() => NetworkSystem.Instance.InRoom ? NetworkSystem.Instance.RoomName : null;

    #endregion

    #region Peer Name Services

    public static string GetPeerName() => NetworkSystem.Instance.GetMyNickName();

    public static (bool isSuccessful, string failureMessage) SetPeerName(string newPeerName) {
        if (newPeerName == "" || newPeerName.IsNullOrWhiteSpace())
            return (false, "Input is Empty");

        if (newPeerName.Length > Constants.MaxPeerNameLength)
            return (false, "Input Exceeds Character Limit");

        if (!IsNameAllowed(newPeerName))
            return (false, "Input is Inappropriate");

        NetworkSystem.Instance.SetMyNickName(newPeerName);
        CustomMapsTerminal.RequestDriverNickNameRefresh();

        Computer.SetLocalNameTagText(newPeerName);

        Computer.savedName = newPeerName;
        Computer.currentName = newPeerName;

        PlayerPrefs.SetString("playerName", newPeerName);
        PlayerPrefs.Save();

        InitializeNoobMaterial(PeerColor);

        return (true, null);
    }

    public static bool Nametags {
        get => Computer.NametagsEnabled;
        set => Computer.InvokeMethod("UpdateNametagSetting", value, true);
    }

    #endregion

    #region Peer Color Services

    public static Color PeerColor {
        get {
            float r = Mathf.Clamp01(PlayerPrefs.GetFloat("redValue"));
            float g = Mathf.Clamp01(PlayerPrefs.GetFloat("greenValue"));
            float b = Mathf.Clamp01(PlayerPrefs.GetFloat("blueValue"));

            return new Color(r, g, b);
        }
        set {
            float r = value.r;
            float g = value.g;
            float b = value.b;

            PlayerPrefs.SetFloat("redValue", Mathf.Clamp01(r));
            PlayerPrefs.SetFloat("greenValue", Mathf.Clamp01(g));
            PlayerPrefs.SetFloat("blueValue", Mathf.Clamp01(b));

            GorillaTagger.Instance.UpdateColor(r, g, b);
            PlayerPrefs.Save();

            InitializeNoobMaterial(value);
        }
    }

    #endregion

    // TODO
    #region Language Services



    #endregion

    #region Turn Services

    public static ETurnType TurnType {
        get => PlayerPrefs.GetString("stickTurning", Application.platform == RuntimePlatform.Android ? "NONE" : "SNAP") switch {
            "SNAP" => ETurnType.Snap,
            "SMOOTH" => ETurnType.Smooth,
            "NONE" => ETurnType.None,
            _ => throw new ArgumentOutOfRangeException()
        };
        set => GorillaSnapTurn.UpdateAndSaveTurnType(value.ToString().ToUpper());
    }

    public static int TurnFactor {
        get => PlayerPrefs.GetInt("turnFactor", 4);
        set => GorillaSnapTurn.UpdateAndSaveTurnFactor(value);
    }

    #endregion

    #region Microphone Services

    public static EMicrophoneType MicrophoneType {
        get => Computer.pttType switch {
            "OPEN MIC" => EMicrophoneType.OpenMicrophone,
            "PUSH TO TALK" => EMicrophoneType.PushToTalk,
            "PUSH TO MUTE" => EMicrophoneType.PushToMute,
            _ => throw new ArgumentOutOfRangeException()
        };
        set {
            string microphoneType = value switch {
                EMicrophoneType.OpenMicrophone => "OPEN MIC",
                EMicrophoneType.PushToTalk => "PUSH TO TALK",
                EMicrophoneType.PushToMute => "PUSH  TO MUTE",
                _ => throw new ArgumentOutOfRangeException()
            };

            Computer.pttType = microphoneType;
            PlayerPrefs.SetString("pttType", microphoneType);
            PlayerPrefs.Save();
        }
    }

    #endregion

    #region Queue Services

    private static void JoinQueue(string queueName, bool isTroopQueue = false) {
        Computer.currentQueue = queueName;
        TroopQueueActive = isTroopQueue;
        TroopPopulation = -1;
        PlayerPrefs.SetString("currentQueue", Computer.currentQueue);
        PlayerPrefs.SetInt("troopQueueActive", Computer.troopQueueActive ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static string Queue {
        get => Computer.currentQueue;
        set => JoinQueue(value);
    }

    public static bool IsPeerAllowedInCompetitive => Computer.allowedInCompetitive;

    #endregion

    #region Troop Services

    public static bool IsInTroop => Computer.troopName != string.Empty;

    public static int TroopPopulation {
        get => Computer.GetCurrentTroopPopulation();
        set => Computer.SetField("currentTroopPopulation", value);
    }

    public static string TroopName {
        get => Computer.troopName;
        set {
            Computer.troopName = value;
            PlayerPrefs.SetString("troopName", value);
        }
    }

    public static bool TroopQueueActive {
        get => Computer.troopQueueActive;
        set {
            Computer.troopQueueActive = value;
            PlayerPrefs.SetInt("troopQueueActive", value ? 1 : 0);
        }
    }

    public static bool IsValidTroopName(string troopName) {
        if (!string.IsNullOrEmpty(troopName) && troopName.Length <= Constants.MaxTroopNameLength) {
            if (!IsPeerAllowedInCompetitive)
                return troopName != "COMPETITIVE";

            return true;
        }

        return false;
    }

    public static void JoinTroopQueue() {
        if (!IsValidTroopName(TroopName))
            return;

        TroopPopulation = -1;
        JoinQueue(TroopName, true);
        Computer.InvokeMethod("RequestTroopPopulation", true);
    }

    public static void LeaveTroop() {
        if (IsValidTroopName(TroopName))
            Computer.troopToJoin = TroopName;

        TroopPopulation = -1;
        TroopName = string.Empty;
        PlayerPrefs.SetString("troopName", TroopName);
        if (TroopQueueActive)
            JoinQueue("DEFAULT");
        PlayerPrefs.Save();
    }

    public static (bool isSuccessful, string failureMessage) JoinTroop(string newTroopName) {
        if (newTroopName == "" || newTroopName.IsNullOrWhiteSpace())
            return (false, "Input is Empty");

        if (newTroopName.Length > Constants.MaxPeerNameLength)
            return (false, "Input Exceeds Character Limit");

        if (!IsNameAllowed(newTroopName))
            return (false, "Input is Inappropriate");

        if (!IsValidTroopName(newTroopName))
            return (false, "Input is Invalid");

        TroopPopulation = -1;
        TroopName = newTroopName;
        PlayerPrefs.SetString("troopName", newTroopName);

        if (TroopQueueActive)
            Queue = TroopName;

        PlayerPrefs.Save();
        JoinTroopQueue();

        return (true, null);
    }

    #endregion

    #region Group Services

    public static string[] AllowedMapsToJoin => Computer.allowedMapsToJoin ?? Computer.GetField<string[]>("_allowedMapsToJoin");

    public static void JoinGroupMap(int map) {
        map = Mathf.Min(AllowedMapsToJoin.Length - 1, map);

        Computer.groupMapJoin = AllowedMapsToJoin[map].ToUpper();
        Computer.groupMapJoinIndex = map;
        PlayerPrefs.SetString("groupMapJoin", Computer.groupMapJoin);
        PlayerPrefs.SetInt("groupMapJoinIndex", Computer.groupMapJoinIndex);
        PlayerPrefs.Save();

        Computer.OnGroupJoinButtonPress(Mathf.Min(AllowedMapsToJoin.Length - 1, Computer.groupMapJoinIndex), FriendJoinCollider);
    }

    #endregion

    #region Voice Services

    public static bool VoiceChatOn {
        get => PlayerPrefs.GetString("voiceChatOn", "TRUE") == "TRUE";
        set => Computer.InvokeMethod("SetVoice", value, true);
    }

    #endregion

    #region AutoMute Services

    public static EAutoMuteType AutoMuteType {
        get => Computer.autoMuteType switch {
            "OFF" => EAutoMuteType.Off,
            "MODERATE" => EAutoMuteType.Moderate,
            "AGGRESSIVE" => EAutoMuteType.Aggressive,
            _ => throw new ArgumentOutOfRangeException()
        };
        set {
            string autoMuteType = value switch {
                EAutoMuteType.Off => "OFF",
                EAutoMuteType.Moderate => "MODERATE",
                EAutoMuteType.Aggressive => "AGGRESSIVE",
                _ => throw new ArgumentOutOfRangeException()
            };

            Computer.autoMuteType = autoMuteType;

            PlayerPrefs.SetInt("autoMuteType", (int)value);
            PlayerPrefs.Save();

            RigContainer.RefreshAllRigVoices();
        }
    }

    #endregion

    #region Items Services

    public static float InstrumentVolume {
        get => PlayerPrefs.GetFloat("instrumentVolume", 0.1f);
        set {
            Computer.instrumentVolume = value / 50f;
            PlayerPrefs.SetFloat("instrumentVolume", value / 50f);
            PlayerPrefs.Save();
        }
    }

    public static bool ItemParticles {
        get => PlayerPrefs.GetString("disableParticles", "FALSE") == "FALSE";
        set {
            Computer.disableParticles = !value;
            PlayerPrefs.SetString("disableParticles", value ? "FALSE" : "TRUE");
            PlayerPrefs.Save();

            GorillaTagger.Instance.ShowCosmeticParticles(value);
        }
    }

    #endregion

    #region Redemption Services

    public static GorillaComputer.RedemptionResult RedemptionStatus {
        get => Computer.RedemptionStatus;
        set => Computer.RedemptionStatus = value;
    }

    #endregion

    #region Credits Services

    public static GorillaNetworking.CreditsView CreditsView => Computer.creditsView;

    public static PropertyInfo TotalCreditsPages => CreditsView.GetType().GetProperty("TotalPages", BindingFlags.NonPublic | BindingFlags.Instance);
    public static MethodInfo CreditsGetPage => CreditsView.GetType().GetMethod("GetPage", BindingFlags.NonPublic | BindingFlags.Instance);

    #endregion

    #region Support Services

    public static bool DisplaySupportTab;

    internal static string PeerID => PlayFabAuthenticator.instance.GetPlayFabPlayerId();
    internal static string PeerPlatform => PlayFabAuthenticator.instance.platform.PlatformTag;

    internal static string GameVersion => GorillaComputer.instance.version;
    internal static string BuildDate => GorillaComputer.instance.buildDate;
    internal static string PeerSessionID => MothershipClientApiUnity.SessionId;

    #endregion
}