using BepInEx.Bootstrap;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using ComputerInterface.Views;
using GorillaExtensions;
using GorillaNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Tools;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ComputerInterface.Behaviours;

public class CustomComputer : MonoBehaviour {
    public static CustomComputer Singleton { get; private set; }
    private bool _initialized;

    private GorillaComputer _gorillaComputer;
    private ComputerViewController _computerViewController;

    private readonly Dictionary<Type, IComputerView> _cachedViews = new();
        
    private MainMenuView _mainMenuView;
    private WarnView _warningView;

    private readonly List<CustomScreenInfo> _customScreenInfos = new();

    private List<CustomKeyboardKey> _keys;

    private readonly Mesh _cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

    private CIConfig _config;

    private AudioClip _clickSound;

    private bool InternetConnected => Application.internetReachability != NetworkReachability.NotReachable;
    private bool _connectionError;

    private readonly HttpClient _httpClient = new();
        
    void Awake() {
        enabled = false;
        Initialize();
    }

    private async void Initialize() {
        if (_initialized || Singleton)
            return;
        Singleton = this;
        _initialized = true;

        List<IComputerModEntry> computerModEntries = [
            new GameSettingsEntry(),
            new CommandLineEntry(),
            new DetailsEntry(),
            new ModListEntry()
        ];
        var modAssemblies = Chainloader.PluginInfos.Values.Select(pluginInfo => pluginInfo.Instance.GetType().Assembly).Distinct();
        var modEntryTypes = modAssemblies.SelectMany(assembly => assembly.GetTypes()).Where(type => typeof(IComputerModEntry).IsAssignableFrom(type) && !type.IsInterface);
        var modEntries = modEntryTypes.Select(type => (IComputerModEntry)Activator.CreateInstance(type)).Where(entry => computerModEntries.All(existingEntry => existingEntry.GetType() != entry.GetType()));
        computerModEntries.AddRange(modEntries);
        Logging.Info($"Found {computerModEntries.Count} physicalComputer Mod Entries");

        _config = Plugin.CIConfig;

        _mainMenuView = new MainMenuView();
        _warningView = new WarnView();
        _cachedViews.Add(typeof(MainMenuView), _mainMenuView);

        _gorillaComputer = GetComponent<GorillaComputer>();
        _gorillaComputer.enabled = false;
        _gorillaComputer.InvokeMethod("Awake");
        _gorillaComputer.InvokeMethod("SwitchState", GorillaComputer.ComputerState.Startup, true);

        _computerViewController = new ComputerViewController();
        _computerViewController.OnTextChanged += SetText;
        _computerViewController.OnSwitchView += SwitchView;
        _computerViewController.OnSetBackground += SetBGImage;
        Logging.Info($"Found {SceneManager.GetActiveScene().GetComponentsInHierarchy<GorillaComputerTerminal>().Count} Computers in GorillaTag scene.");
        PrepareMonitor(SceneManager.GetActiveScene(), SceneManager.GetActiveScene().GetComponentInHierarchy<GorillaComputerTerminal>().gameObject.transform.GetChild(0).GetPath());

        _clickSound = await AssetLoader.LoadAsset<AudioClip>("ClickSound");

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        try {
            BaseGameInterface.InitAll();
            ShowInitialView(_mainMenuView, computerModEntries);
        }
        catch (Exception exception) {
            Logging.Error($"Computer Interface failed to successfully end initializing: {exception}");
        }

        try {
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://raw.githubusercontent.com/DecalFree/ComputerInterface/main/Version.txt");
            
            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var latestVersion = (await response.Content.ReadAsStringAsync()).Trim();

            if (!string.Equals(Constants.Version, latestVersion, StringComparison.OrdinalIgnoreCase))
                _computerViewController.SetView(_warningView, [ new WarnView.GeneralWarning("A new version of Computer Interface is available.\nIt is recommended to update to avoid any issues.") ]);
        }
        catch (Exception exception) {
            Logging.Error($"Computer Interface failed to check the its version: {exception}");
        }
            
        enabled = true;
        Logging.Info("Initialized computers");
    }

    private void ShowInitialView(MainMenuView view, List<IComputerModEntry> computerModEntries) {
        foreach (var pluginInfo in Chainloader.PluginInfos.Values.Where(pluginInfo => _config.IsModDisabled(pluginInfo.Metadata.GUID)))
            pluginInfo.Instance.enabled = false;

        if (NetworkSystem.Instance.WrongVersion) {
            _computerViewController.SetView(_warningView, [ new WarnView.OutdatedWarning() ]);
            return;
        }
        _computerViewController.SetView(view, null);
        view.ShowEntries(computerModEntries);
    }

    private void Update() {
        // Get key state for the key debugging feature
        if (CustomKeyboardKey.KeyDebuggerEnabled && _keys != null) {
            foreach (var key in _keys)
                key.Fetch();
        }

        // Make sure the physicalComputer is ready
        if (_computerViewController.CurrentComputerView != null) {
            // Check to see if our connection is off
            if (!InternetConnected && !_connectionError) {
                _connectionError = true;
                _computerViewController.SetView(_warningView, [ new WarnView.NoInternetWarning() ]);
                _gorillaComputer.UpdateFailureText("NO WIFI OR LAN CONNECTION DETECTED.");
            }

            // Check to see if we're back online
            if (InternetConnected && _connectionError) {
                _connectionError = false;
                _computerViewController.SetView(_computerViewController.CurrentComputerView == _warningView ? _mainMenuView : _computerViewController.CurrentComputerView, null);
                _gorillaComputer.InvokeMethod("RestoreFromFailureState", null);
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode) {
        try  {
            var sceneName = scene.name;

            if (loadMode == LoadSceneMode.Additive) {
                if (sceneName == "Cave")
                    PrepareMonitor(scene, "Cave_Main_Prefab/OldCave/MinesComputer/GorillaComputerObject/ComputerUI");
                    
                switch (ZoneManagement.instance.activeZones.First()) {
                    case GTZone.monkeBlocks:
                        PrepareMonitor(SceneManager.GetSceneByName("GorillaTag"), "Environment Objects/MonkeBlocksRoomPersistent/MonkeBlocksComputer/GorillaComputerObject/ComputerUI");
                        break;
                    case GTZone.monkeBlocksShared:
                        PrepareMonitor(SceneManager.GetSceneByName("GorillaTag"), "Environment Objects/LocalObjects_Prefab/SharedBlocksMapSelectLobby/GorillaComputerObject/ComputerUI");
                        break;
                    default:
                        PrepareMonitor(scene, scene.GetComponentInHierarchy<GorillaComputerTerminal>().gameObject.transform.GetChild(0).GetPath());
                        break;
                }
            }
        } 
        catch (Exception exception) {
            Logging.Warning($"Computer Interface couldn't find a computer to replace: {exception}");
        }
    }

    private void OnSceneUnloaded(Scene scene) {
        var sceneName = scene.name;
        var customScreenInfo = _customScreenInfos.FirstOrDefault(info => info.SceneName == sceneName);

        if (customScreenInfo != null)
            _customScreenInfos.Remove(customScreenInfo);
    }

    public void SetText(string text) {
        foreach (var customScreenInfo in _customScreenInfos)
            customScreenInfo.Text = text;
    }

    public void SetBG(float r, float g, float b) =>
        SetBG(new Color(r, g, b));

    public void SetBG(Color color) {
        foreach (var customScreenInfo in _customScreenInfos) {
            customScreenInfo.Color = color;
            _config.ScreenBackgroundColor.Value = customScreenInfo.Color;
        }
    }

    public Color GetBG() =>
        _config.ScreenBackgroundColor.Value;

    public void SetBGImage(ComputerViewChangeBackgroundEventArgs args) {
        foreach (var customScreenInfo in _customScreenInfos) {
            if (args == null) {
                customScreenInfo.BackgroundTexture = _config.BackgroundTexture;
                customScreenInfo.Color = _config.ScreenBackgroundColor.Value;
                continue;
            }

            if (args.Texture == null) {
                _config.BackgroundTexture = null;
                customScreenInfo.BackgroundTexture = _config.BackgroundTexture;
                customScreenInfo.Color = _config.ScreenBackgroundColor.Value;
            }
            else {
                _config.BackgroundTexture = args.Texture;
                customScreenInfo.Color = args.ImageColor ?? _config.ScreenBackgroundColor.Value;
                customScreenInfo.BackgroundTexture = args.Texture;
            }
        }
    }
    
    public string GetScreenBackgroundPath() =>
        _config.ScreenBackgroundPath.Value;
    
    public Texture GetTexture(string path) =>
        _config.GetTexture(path);
    
    public void PressButton(CustomKeyboardKey key, bool isLeftHand = false) {
        var audioSource = isLeftHand ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;
        audioSource.PlayOneShot(_clickSound, 0.8f);

        _computerViewController.NotifyOfKeyPress(key.KeyboardKey);
    }

    private void SwitchView(ComputerViewSwitchEventArgs args) {
        if (args.SourceType == args.DestinationType)
            return;

        var destinationView = GetOrCreateView(args.DestinationType);

        if (destinationView == null)
            return;

        destinationView.CallerViewType = args.SourceType;
        _computerViewController.SetView(destinationView, args.Args);
    }

    private IComputerView GetOrCreateView(Type type) {
        if (_cachedViews.TryGetValue(type, out var view))
            return view;
        
        var newView = (ComputerView)Activator.CreateInstance(type);
        _cachedViews.Add(type, newView);
        return newView;
    }

    private async void PrepareMonitor(Scene scene, string computerPath) {
        scene.TryFindByPath(computerPath, out Transform computer);
        var physicalComputer = computer.gameObject;

        try {
            ReplaceKeys(physicalComputer);

            var customScreenInfo = await CreateMonitor(physicalComputer, scene.name);
            customScreenInfo.Text = _computerViewController.CurrentComputerView != null ? _computerViewController.CurrentComputerView.Text : "Loading";
            customScreenInfo.Color = _config.ScreenBackgroundColor.Value;
            customScreenInfo.BackgroundTexture = _config.BackgroundTexture;

            _customScreenInfos.Add(customScreenInfo);
        }
        catch (Exception exception) {
            Logging.Error($"Computer Interface failed to prepare the monitor: {exception}");
        }
    }

    private void ReplaceKeys(GameObject computer) {
        _keys = [];

        Dictionary<string, EKeyboardKey> nameToEnum = new();

        foreach (var enumString in Enum.GetNames(typeof(EKeyboardKey))) {
            var key = enumString.Replace("NUM", "").ToLower();
            nameToEnum.Add(key, (EKeyboardKey)Enum.Parse(typeof(EKeyboardKey), enumString));
        }

        var buttonArray = computer.transform.parent?.parent?.Find("GorillaComputerObject")?.GetComponentsInChildren<GorillaKeyboardButton>(true);
        buttonArray ??= computer.transform.parent?.Find("GorillaComputerObject")?.GetComponentsInChildren<GorillaKeyboardButton>(true);
        buttonArray ??= computer.GetComponentsInChildren<GorillaKeyboardButton>(true);

        foreach (var button in buttonArray) {
            if (button.characterString is "up" or "down")
            {
                button.GetComponentInChildren<MeshRenderer>(true).material.color = new Color(0.1f, 0.1f, 0.1f);
                button.GetComponentInChildren<MeshFilter>().mesh = _cubeMesh;
                button.transform.localPosition -= new Vector3(0, 0.6f, 0);
                DestroyImmediate(button.GetComponent<BoxCollider>());
                if (FindText(button.gameObject, button.name + "text")?.GetComponent<TextMeshPro>() is { } arrowBtnText)
                    DestroyImmediate(arrowBtnText);
                continue;
            }

            if (!nameToEnum.TryGetValue(button.characterString.ToLower(), out var key))
                continue;

            if (FindText(button.gameObject) is { } buttonText) {
                var customButton = button.gameObject.AddComponent<CustomKeyboardKey>();
                customButton.pressTime = Traverse.Create(computer.GetComponentsInChildren<GorillaKeyboardButton>()).Field("pressTime").GetValue<float>();
                customButton.functionKey = button.functionKey;

                button.GetComponent<MeshFilter>().mesh = _cubeMesh;
                DestroyImmediate(button);

                customButton.Init(this, key, buttonText);
                _keys.Add(customButton);
            }
        }

        var keyboardRenderer = _keys[0].transform.parent?.parent?.parent?.GetComponent<MeshRenderer>();
        keyboardRenderer ??= _keys[0].transform.parent?.parent?.parent?.gameObject?.GetComponent<MeshRenderer>();
        keyboardRenderer ??= _keys[0].transform.parent?.parent?.parent?.parent?.parent?.parent?.Find("Static/keyboard (1)")?.GetComponent<MeshRenderer>();

        if (keyboardRenderer)
            keyboardRenderer.material.color = new Color(0.3f, 0.3f, 0.3f);

        var enterKey = _keys.Last(x => x.KeyboardKey == EKeyboardKey.Enter);
        var mKey = _keys.Last(x => x.KeyboardKey == EKeyboardKey.M);
        var deleteKey = _keys.Last(x => x.KeyboardKey == EKeyboardKey.Delete);

        CreateKey(enterKey.gameObject, "Space", new Vector3(2.6f, 0, 3), EKeyboardKey.Space, "SPACE");
        CreateKey(deleteKey.gameObject, "Back", new Vector3(0, 0, -29.8f), EKeyboardKey.Back, "BACK", ColorUtility.TryParseHtmlString("#8787e0", out var backButtonColor) ? backButtonColor : Color.white);

        var arrowColourExists = ColorUtility.TryParseHtmlString("#abdbab", out Color arrowKeyButtonColor);

        var leftKey = CreateKey(mKey.gameObject, "Left", new Vector3(0, 0, 5.6f), EKeyboardKey.Left, "<", arrowColourExists ? arrowKeyButtonColor : Color.white);
        var downKey = CreateKey(leftKey.gameObject, "Down", new Vector3(0, 0, 2.3f), EKeyboardKey.Down, ">", arrowColourExists ? arrowKeyButtonColor : Color.white);
        CreateKey(downKey.gameObject, "Right", new Vector3(0, 0, 2.3f), EKeyboardKey.Right, ">", arrowColourExists ? arrowKeyButtonColor : Color.white);
        var upKey = CreateKey(downKey.gameObject, "Up", new Vector3(-2.3f, 0, 0), EKeyboardKey.Up, ">", arrowColourExists ? arrowKeyButtonColor : Color.white);

        var downKeyText = FindText(downKey.gameObject).transform;
        downKeyText.localPosition -= new Vector3(0, 0, 0.05f);
        downKeyText.localEulerAngles += new Vector3(0, 0, -90);

        var upKeyText = FindText(upKey.gameObject).transform;
        upKeyText.localPosition += new Vector3(0, 0, 0.05f);
        upKeyText.localEulerAngles += new Vector3(0, 0, 90);
    }

    private static TextMeshPro FindText(GameObject button, string name = null) {
        // Logging.Info($"Replacing key {button.name} / {name}");
        if (button.GetComponent<TextMeshPro>() is { } text)
            return text;

        if (name.IsNullOrWhiteSpace())
            name = button.name.Replace(" ", "");

        if (name.Contains("enter"))
            name = "enter";

        // Forest
        var t = button.transform.parent?.parent?.parent?.parent?.parent?.parent?.parent?.Find(name);
            
        // Custom Maps
        t ??= button.transform.parent?.parent?.parent?.parent?.parent?.transform.Find($"UIParent/Text/{name}");

        // Other Maps
        t ??= button.transform.parent?.parent?.Find($"Text/{name}");

        return t?.GetComponent<TextMeshPro>();
    }

    private CustomKeyboardKey CreateKey(GameObject prefab, string goName, Vector3 offset, EKeyboardKey key, string label = null, Color? color = null) {
        var newKey = Instantiate(prefab.gameObject, prefab.transform.parent);
        newKey.name = goName;
        newKey.transform.localPosition += offset;
        newKey.GetComponent<MeshFilter>().mesh = _cubeMesh;
        newKey.GetComponent<Collider>().enabled = true;

        var keyText = FindText(prefab, prefab.name);
        var newKeyText = Instantiate(keyText.gameObject, keyText.gameObject.transform.parent).GetComponent<TextMeshPro>();
        newKeyText.name = goName;
        newKeyText.transform.localPosition += offset;

        var customKeyboardKey = newKey.GetComponent<CustomKeyboardKey>();

        if (label.IsNullOrWhiteSpace())
        {
            customKeyboardKey.Init(this, key);
        }
        else if (color.HasValue)
        {
            customKeyboardKey.Init(this, key, newKeyText, label, color.Value);
        }
        else
        {
            customKeyboardKey.Init(this, key, newKeyText, label);
        }

        _keys.Add(customKeyboardKey);
        return customKeyboardKey;
    }

    private async Task<CustomScreenInfo> CreateMonitor(GameObject physicalComputer, string sceneName) {
        var monitorAsset = await AssetLoader.LoadAsset<GameObject>("Classic Monitor");
        var newMonitor = Instantiate(monitorAsset, physicalComputer.transform.Find("monitor") ?? physicalComputer.transform.Find("monitor (1)"), false);

        newMonitor.name = $"Computer Interface (Scene - {sceneName})";
        newMonitor.transform.localPosition = new Vector3(-0.0787f, -0.12f, 0.5344f);
        newMonitor.transform.localEulerAngles = Vector3.right * 90f;
        newMonitor.transform.SetParent(physicalComputer.transform.parent, true);
        newMonitor.transform.Find("Main Monitor").gameObject.AddComponent<GorillaSurfaceOverride>();

        CustomScreenInfo info = new() {
            SceneName = sceneName,
            Transform = newMonitor.transform,
            TextMeshProUgui = newMonitor.transform.Find("Canvas/Text (TMP)").GetComponent<TextMeshProUGUI>(),
            Renderer = newMonitor.transform.Find("Main Monitor").GetComponent<Renderer>(),
            Background = newMonitor.transform.Find("Canvas/RawImage").GetComponent<RawImage>(),
            Color = new Color(0.05f, 0.05f, 0.05f)
        };

        RemoveMonitor(physicalComputer, sceneName);
        return info;
    }

    private void RemoveMonitor(GameObject computer, string sceneName) {
        GameObject monitor = null;
        
        foreach (Transform child in computer.transform) {
            if (child.name.StartsWith("monitor")) {
                monitor = child.gameObject;
                monitor.SetActive(false);
            }
        }

        if (monitor is null) {
            Logging.Info("Unable to find monitor");
            return;
        }

        // Stable for now 
        if (computer.transform.parent.TryGetComponent(out GorillaComputerTerminal terminal)) {
            terminal.monitorMesh?.gameObject?.SetActive(false);
            terminal.myFunctionText?.gameObject?.SetActive(false);
            terminal.myScreenText?.gameObject?.SetActive(false);
        }

        var monitorTransform = computer.transform.parent.parent?.Find("GorillaComputerObject/ComputerUI/monitor");
        monitorTransform ??= computer.transform.parent?.Find("GorillaComputerObject/ComputerUI/monitor");
        monitorTransform ??= computer.transform.Find("GorillaComputerObject/ComputerUI/monitor");
        monitorTransform?.gameObject?.SetActive(false);
    }
}