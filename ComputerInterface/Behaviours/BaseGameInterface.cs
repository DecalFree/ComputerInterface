using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using GorillaNetworking;
using GorillaTagScripts;
using Photon.Pun;
using System;
using System.Threading.Tasks;
using ComputerInterface.Enumerations;
using GorillaTagScripts.VirtualStumpCustomMaps;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace ComputerInterface.Behaviours;

public static class BaseGameInterface {
    public const int MaxRoomLength = 10;
    public const int MaxNameLength = 12;
    public const int MaxTroopLength = 12;
    public const int MaxCodeLength = 8; // Not fully sure if any other codes are 8+ characters. Please change the length if they are. -DecalFree

    // WordCheckResult (enum), WordCheckResultToMessage, WordAllowed
    #region Word Checking
    
    public static string WordCheckResultToMessage(EWordCheckResult result) => result switch {
        EWordCheckResult.Allowed => "Input is allowed",
        EWordCheckResult.Empty => "Input is empty",
        EWordCheckResult.Blank => "Input is blank",
        EWordCheckResult.NotAllowed => "Input is not allowed",
        EWordCheckResult.TooLong => "Input is too long",
        EWordCheckResult.ComputerNotFound => "Computer not found",
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
    };

    private static EWordCheckResult WordAllowed(string word) {
        if (word.Length == 0)
            return EWordCheckResult.Empty;
        
        if (string.IsNullOrWhiteSpace(word))
            return EWordCheckResult.Blank;
        
        if (!CheckForComputer(out var computer))
            return EWordCheckResult.ComputerNotFound;
        
        if (!computer.CheckAutoBanListForName(word))
            return EWordCheckResult.NotAllowed;
        
        if (word.Length > MaxNameLength)
            return EWordCheckResult.TooLong;
        
        return EWordCheckResult.Allowed;
    }

    #endregion

    // Disconnect, JoinRoom, GetRoomCode
    #region Room Settings

    public static EWordCheckResult JoinRoom(string roomId) {
        if (!CheckForComputer(out var computer))
            return EWordCheckResult.ComputerNotFound;

        var roomAllowed = WordAllowed(roomId);

        if (roomAllowed == EWordCheckResult.Allowed) {
            if (FriendshipGroupDetection.Instance.IsInParty && !FriendshipGroupDetection.Instance.IsPartyWithinCollider(computer.friendJoinCollider))
                FriendshipGroupDetection.Instance.LeaveParty();

            if (computer.IsPlayerInVirtualStump())
                CustomMapManager.UnloadMap(false);

            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom(roomId, FriendshipGroupDetection.Instance.IsInParty ? JoinType.JoinWithParty : JoinType.Solo);
        }

        return roomAllowed;
    }

    public static async void Disconnect() {
        if (FriendshipGroupDetection.Instance.IsInParty) {
            FriendshipGroupDetection.Instance.LeaveParty();
            await Task.Delay(1000);
        }

        await NetworkSystem.Instance.ReturnToSinglePlayer();
    }

    public static string GetRoomCode() =>
        NetworkSystem.Instance.InRoom ? NetworkSystem.Instance.RoomName : null;

    #endregion

    // GetName, SetName
    #region Name Settings

    public static string GetName() =>
        CheckForComputer(out var computer) ? computer.savedName : null;

    public static EWordCheckResult SetName(string name) {
        if (!CheckForComputer(out var computer))
            return EWordCheckResult.ComputerNotFound;

        var wordAllowed = WordAllowed(name);
        if (wordAllowed == EWordCheckResult.Allowed) {
            name = name.Replace(" ", "");

            computer.currentName = name;
            NetworkSystem.Instance.SetMyNickName(computer.currentName);
            CustomMapsTerminal.RequestDriverNickNameRefresh();
            GorillaTagger.Instance.offlineVRRig.UpdateName();

            computer.SetLocalNameTagText(name);

            computer.savedName = name;
            PlayerPrefs.SetString("playerName", name);
            PlayerPrefs.Save();

            GetColor(out var r, out var g, out var b);
            InitializeNoobMaterial(r, g, b);
        }

        return wordAllowed;
    }

    public static bool GetNametagsEnabled() =>
        CheckForComputer(out var computer) && computer.NametagsEnabled;

    public static void SetNametagSetting(bool newValue, bool saveValue = true) {
        if (!CheckForComputer(out var computer))
            return;
            
        computer.SetField("<NametagsEnabled>k__BackingField", newValue);
        NetworkSystem.Instance.SetMyNickName(computer.NametagsEnabled ? computer.savedName : NetworkSystem.Instance.GetMyDefaultName());
        GetColor(out var r, out var g, out var b);
        InitializeNoobMaterial(r, g, b);
        computer.GetField<Action<bool>>("onNametagSettingChangedAction").Invoke(computer.NametagsEnabled);
        if (saveValue) {
            PlayerPrefs.SetInt(computer.NameTagPlayerPref, computer.NametagsEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    #endregion

    // SetColor, GetColor, InitializeNoobMaterial
    #region Colour Settings

    public static void SetColor(float r, float g, float b) {
        PlayerPrefs.SetFloat("redValue", Mathf.Clamp01(r));
        PlayerPrefs.SetFloat("greenValue", Mathf.Clamp01(g));
        PlayerPrefs.SetFloat("blueValue", Mathf.Clamp01(b));

        GorillaTagger.Instance.UpdateColor(r, g, b);
        PlayerPrefs.Save();

        InitializeNoobMaterial(r, g, b);
    }
    
    public static void SetColor(Color color) =>
        SetColor(color.r, color.g, color.b);

    public static void GetColor(out float r, out float g, out float b) {
        r = Mathf.Clamp01(PlayerPrefs.GetFloat("redValue"));
        g = Mathf.Clamp01(PlayerPrefs.GetFloat("greenValue"));
        b = Mathf.Clamp01(PlayerPrefs.GetFloat("blueValue"));
    }

    public static Color GetColor() {
        GetColor(out var r, out var g, out var b);
        return new Color(r, g, b);
    }

    public static void InitializeNoobMaterial(float r, float g, float b) =>
        InitializeNoobMaterial(new Color(r, g, b));

    private static void InitializeNoobMaterial(Color color) {
        if (NetworkSystem.Instance.InRoom)
            GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, color.r, color.g, color.b);
    }

    #endregion

    // SetTurnMode, GetTurnMode, SetTurnValue, GetTurnValue
    #region Turn Settings

    public static void SetTurnMode(ETurnMode turnMode) {
        if (!CheckForComputer(out var computer))
            return;

        var turnModeString = turnMode.ToString().ToUpper();
        GorillaSnapTurn.UpdateAndSaveTurnType(turnModeString);
    }

    public static ETurnMode GetTurnMode() {
        var turnMode = PlayerPrefs.GetString("stickTurning");
        if (turnMode.IsNullOrWhiteSpace())
            return ETurnMode.None;
        return (ETurnMode)Enum.Parse(typeof(ETurnMode), string.Concat(turnMode.ToUpper()[0], turnMode.ToLower()[1..]));
    }

    public static void SetTurnValue(int value) {
        if (!CheckForComputer(out var computer))
            return;

        GorillaSnapTurn.UpdateAndSaveTurnFactor(value);
    }

    public static int GetTurnValue() =>
        PlayerPrefs.GetInt("turnFactor", 4);

    #endregion

    // SetInstrumentVolume, GetInstrumentVolume, SetItemMode, GetItemMode
    #region Item Settings

    public static void SetInstrumentVolume(int value) {
        if (!CheckForComputer(out var computer))
            return;

        computer.instrumentVolume = value / 50f;
        PlayerPrefs.SetFloat("instrumentVolume", computer.instrumentVolume);
        PlayerPrefs.Save();
    }

    public static float GetInstrumentVolume() =>
        PlayerPrefs.GetFloat("instrumentVolume", 0.1f);

    public static void SetItemMode(bool disableParticles) {
        if (!CheckForComputer(out var computer))
            return;

        computer.disableParticles = disableParticles;
        PlayerPrefs.SetString("disableParticles", disableParticles ? "TRUE" : "FALSE");
        PlayerPrefs.Save();
        GorillaTagger.Instance.ShowCosmeticParticles(!disableParticles);
    }

    public static bool GetItemMode() =>
        PlayerPrefs.GetString("disableParticles") == "TRUE";

    #endregion

    // SetRedemptionStatus, GetRedemptionStatus
    #region Redemption Settings

    public static void SetRedemptionStatus(GorillaComputer.RedemptionResult newStatus) {
        if (!CheckForComputer(out var computer))
            return;

        computer.RedemptionStatus = newStatus;
    }

    public static GorillaComputer.RedemptionResult GetRedemptionStatus() =>
        !CheckForComputer(out var computer) ? GorillaComputer.RedemptionResult.Empty : computer.RedemptionStatus;

    #endregion

    // SetPttMode, GetPttMode
    #region Microphone Settings

    public static void SetPttMode(EPTTMode mode) {
        if (!CheckForComputer(out var computer))
            return;

        var modeString = mode switch {
            EPTTMode.OpenMic => "OPEN MIC",
            EPTTMode.PushToTalk => "PUSH TO TALK",
            EPTTMode.PushToMute => "PUSH TO MUTE",
            _ => throw new ArgumentOutOfRangeException()
        };

        computer.pttType = modeString;
        PlayerPrefs.SetString("pttType", modeString);
        PlayerPrefs.Save();
    }

    public static EPTTMode GetPttMode() =>
        PlayerPrefs.GetString("pttType", "OPEN MIC") switch {
            "OPEN MIC" => EPTTMode.OpenMic,
            "PUSH TO TALK" => EPTTMode.PushToTalk,
            "PUSH TO MUTE" => EPTTMode.PushToMute,
            _ => throw new ArgumentOutOfRangeException()
        };

    #endregion

    // SetVoiceMode, GetVoiceMode
    #region Voice Settings

    public static void SetVoiceMode(bool humanVoiceOn) {
        if (!CheckForComputer(out var computer))
            return;

        computer.voiceChatOn = humanVoiceOn ? "TRUE" : "FALSE";
        PlayerPrefs.SetString("voiceChatOn", computer.voiceChatOn);
        PlayerPrefs.Save();

        RigContainer.RefreshAllRigVoices();
    }

    public static bool GetVoiceMode() =>
        PlayerPrefs.GetString("voiceChatOn", "TRUE") == "TRUE";

    #endregion

    // SetAutomodMode, GetAutomodMode
    #region Automod Settings

    public static void SetAutomodMode(int value) {
        if (!CheckForComputer(out var computer))
            return;

        var automodModeString = (EAutomodMode)value;
        computer.SetField("autoMuteType", automodModeString.ToString().ToUpper());

        PlayerPrefs.SetInt("autoMute", (int)automodModeString);
        PlayerPrefs.Save();

        RigContainer.RefreshAllRigVoices();
    }

    public static EAutomodMode GetAutomodMode() =>
        (EAutomodMode)PlayerPrefs.GetInt("autoMute", 1);

    #endregion

    // JoinGroupMap, GetGroupJoinMaps
    #region Group Settings

    public static void JoinGroupMap(int map) {
        if (!CheckForComputer(out var computer))
            return;

        var allowedMapsToJoin = GetGroupJoinMaps();

        map = Mathf.Min(allowedMapsToJoin.Length - 1, map);

        computer.groupMapJoin = allowedMapsToJoin[map].ToUpper();
        computer.groupMapJoinIndex = map;
        PlayerPrefs.SetString("groupMapJoin", computer.groupMapJoin);
        PlayerPrefs.SetInt("groupMapJoinIndex", computer.groupMapJoinIndex);
        PlayerPrefs.Save();

        computer.OnGroupJoinButtonPress(Mathf.Min(allowedMapsToJoin.Length - 1, map), computer.friendJoinCollider);
    }

    public static string[] GetGroupJoinMaps() =>
        CheckForComputer(out var computer) ? computer.allowedMapsToJoin : [];

    #endregion

    // displaySupportTab (bool)
    #region Support Settings

    public static bool DisplaySupportTab;

    #endregion

    // SetQueue (IQueueInfo / string), GetQueue, AllowedInCompetitive
    #region Queue Settings

    public static void SetQueue(IQueueInfo queue, bool isTroopQueue = false) =>
        SetQueue(queue.QueueName, isTroopQueue);

    private static void SetQueue(string queueName, bool isTroopQueue = false) {
        if (!CheckForComputer(out var computer))
            return;
            
        if (queueName == "COMPETITIVE" && !AllowedInCompetitive())
            return;
            
        computer.currentQueue = queueName;
        computer.troopQueueActive = isTroopQueue;
        computer.SetField("currentTroopPopulation", -1);
        PlayerPrefs.SetString("currentQueue", queueName);
        PlayerPrefs.SetInt("troopQueueActive", computer.troopQueueActive ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static string GetQueue() =>
        PlayerPrefs.GetString("currentQueue", "DEFAULT");

    public static bool AllowedInCompetitive() =>
        CheckForComputer(out var computer) && computer.allowedInCompetitive;

    #endregion

    // IsValidTroopName, JoinTroopQueue, JoinDefaultQueue, LeaveTroop, JoinTroop, GetCurrentTroop, IsInTroop
    #region Troop Settings
        
    public static bool IsValidTroopName(string troopName) {
        if (!string.IsNullOrEmpty(troopName) && troopName.Length <= MaxTroopLength) {
            if (!AllowedInCompetitive())
                return troopName != "COMPETITIVE";

            return true;
        }

        return false;
    }

    public static void JoinTroopQueue() {
        if (!CheckForComputer(out var computer))
            return;
            
        computer.InvokeMethod("JoinTroopQueue");
    }

    public static void JoinDefaultQueue() =>
        SetQueue("DEFAULT");

    public static void LeaveTroop() {
        if (!CheckForComputer(out var computer))
            return;

        if (IsValidTroopName(computer.troopName))
            computer.troopToJoin = computer.troopName;
            
        computer.SetField("currentTroopPopulation", -1);
        computer.troopName = string.Empty;

        PlayerPrefs.SetString("troopName", computer.troopName);
        if (computer.troopQueueActive)
            JoinDefaultQueue();
        PlayerPrefs.Save();
    }
        
    public static EWordCheckResult JoinTroop(string troopName) {
        if (!CheckForComputer(out var computer))
            return EWordCheckResult.ComputerNotFound;

        var roomAllowed = WordAllowed(troopName);
        if (roomAllowed == EWordCheckResult.Allowed) {
            if (IsValidTroopName(troopName)) {
                computer.SetField("currentTroopPopulation", -1);
                computer.troopName = troopName;
                PlayerPrefs.SetString("troopName", troopName);
                if (computer.troopQueueActive) {
                    computer.currentQueue = troopName;
                    PlayerPrefs.SetString("currentQueue", computer.currentQueue);
                }
                PlayerPrefs.Save();
                JoinTroopQueue();
            }
        }

        return roomAllowed;
    }

    public static string GetCurrentTroop() {
        if (!CheckForComputer(out var computer))
            return string.Empty;

        return computer.troopQueueActive ? computer.troopName : computer.currentQueue;
    }

    public static bool IsInTroop() =>
        CheckForComputer(out var computer) && computer.troopName != string.Empty;

    #endregion

    // InitColorState, InitNameState, InitTurnState, InitMicState, InitTroopState, InitVoiceMode, InitAutomodMode, InitItemMode, InitRedemptionStatus, InitSupportMode, InitAll
    #region Initialization

    public static void InitColorState() =>
        GorillaTagger.Instance.UpdateColor(PlayerPrefs.GetFloat("redValue", 0f), PlayerPrefs.GetFloat("greenValue", 0f), PlayerPrefs.GetFloat("blueValue", 0f));

    public static void InitNameState() {
        var name = PlayerPrefs.GetString("playerName", "gorilla");
        SetName(name);
        GorillaTagger.Instance.offlineVRRig.UpdateName();
            
        var nameTagsEnabled = PlayerPrefs.GetInt("nameTagsOn", -1);
        SetNametagSetting(nameTagsEnabled is -1 or > 0);
    }

    public static void InitTurnState() {
        var gorillaSnapTurn = GorillaTagger.Instance.GetComponent<GorillaSnapTurn>();
        var defaultValue = Application.platform == RuntimePlatform.Android ? "NONE" : "SNAP";
        var turnType = PlayerPrefs.GetString("stickTurning", defaultValue);
        var turnValue = PlayerPrefs.GetInt("turnFactor", 4);
        gorillaSnapTurn.ChangeTurnMode(turnType, turnValue);
    }

    public static void InitMicState() =>
        SetPttMode(GetPttMode());

    public static void InitTroopState() {
        GorillaComputer.instance.troopQueueActive = PlayerPrefs.GetInt("troopQueueActive", 0) == 1;
        var savePlayerPrefs = false;
            
        if (GorillaComputer.instance.troopQueueActive && !IsValidTroopName(GorillaComputer.instance.troopName)) {
            GorillaComputer.instance.troopQueueActive = false;
            PlayerPrefs.SetInt("troopQueueActive", GorillaComputer.instance.troopQueueActive ? 1 : 0);
            GorillaComputer.instance.currentQueue = "DEFAULT";
            PlayerPrefs.SetString("currentQueue", GorillaComputer.instance.currentQueue);
            savePlayerPrefs = true;
        }
        if (savePlayerPrefs)
            PlayerPrefs.Save();
    }

    public static void InitVoiceMode() =>
        SetVoiceMode(GetVoiceMode());

    public static void InitAutomodMode() =>
        SetAutomodMode((int)GetAutomodMode());

    public static void InitItemMode() =>
        SetItemMode(GetItemMode());

    public static void InitRedemptionStatus() =>
        SetRedemptionStatus(GorillaComputer.RedemptionResult.Empty);

    public static void InitSupportMode() =>
        DisplaySupportTab = false;

    public static void InitAll()
    {
        InitColorState();
        InitNameState();
        InitTurnState();
        InitMicState();
        InitTroopState();
        InitVoiceMode();
        InitAutomodMode();
        InitItemMode();
        InitRedemptionStatus();
        InitSupportMode();

        if (CheckForComputer(out var computer))
            computer.InvokeMethod("Start");
    }

    #endregion

    public static bool CheckForComputer(out GorillaComputer computer) {
        if (GorillaComputer.instance == null) {
            computer = null;
            return false;
        }

        computer = GorillaComputer.instance;
        return true;
    }
}