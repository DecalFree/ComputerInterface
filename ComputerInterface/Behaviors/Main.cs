using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
#if BEPINEX
using BepInEx;
using BepInEx.Bootstrap;
#endif
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using ComputerInterface.Models;
using ComputerInterface.Tools;
using ComputerInterface.Views;
using GorillaExtensions;
using GorillaNetworking;
using HarmonyLib;
#if MELONLOADER
using MelonLoader;
#endif
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ComputerInterface.Behaviors;

public class Main : MonoBehaviourTick {
    public static Main Singleton { get; private set; }
    private bool _initialized;

    private GameObject _betterComputerAsset;
    private AudioClip _clickSound;

    private readonly List<GorillaComputerTerminal> _initializedTerminals = [];

    private readonly Dictionary<Type, ComputerView> _cachedComputerViews = [];
    internal ComputerView CurrentComputerView { get; private set; }

    private readonly List<ComputerScreenInfo> _computerScreenInfos = [];

    private List<CustomKeyboardButton> _keyboardButtons = [];

    private readonly Mesh _cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

    private MainMenuView _mainMenuView;
    private WarningView _warningView;
    private SafetyWarningView _safetyWarningView;

    public event Action<GorillaComputerTerminal> OnCustomTerminalPrepared;

    private CIConfig _ciConfig;

    private bool InternetConnected => Application.internetReachability != NetworkReachability.NotReachable;
    private bool _connectionError;

    private readonly HttpClient _httpClient = new();

    private async void Start() {
        if (_initialized || Singleton != null && Singleton != this) {
            Logging.Info("Failed to start initializing Computer Interface");
            return;
        }
        Singleton = this;
        _initialized = true;

        List<IComputerViewEntry> computerViewEntries = [
            new GameSettingsEntry(),
            new CommandLineEntry(),
            new DetailsEntry(),
#if BEPINEX
            new ModListEntry()
#endif
        ];
#if BEPINEX
        IEnumerable<Assembly> assemblies = Chainloader.PluginInfos.Values.Select(pluginInfo => pluginInfo.Instance.GetType().Assembly).Distinct();
#elif MELONLOADER
        IEnumerable<Assembly> assemblies = MelonMod.RegisteredMelons.Select(melonMod => melonMod.GetType().Assembly).Distinct();
#endif
        IEnumerable<IComputerViewEntry> foundViewEntries = assemblies.SelectMany(assembly => assembly.GetTypes())
            .Where(foundEntry => typeof(IComputerViewEntry).IsAssignableFrom(foundEntry) && !foundEntry.IsInterface)
            .Select(entryType => (IComputerViewEntry)Activator.CreateInstance(entryType)).Where(viewEntry =>
                computerViewEntries.All(existingEntry => existingEntry.GetType() != viewEntry.GetType()));
        computerViewEntries.AddRange(foundViewEntries);
        Logging.Info($"Found {computerViewEntries.Count} physicalComputer View Entries");

        _ciConfig = PluginCore.CIConfig;

        _mainMenuView = new MainMenuView();
        _warningView = new WarningView();
        _safetyWarningView = new SafetyWarningView();

        _cachedComputerViews.Add(typeof(MainMenuView), _mainMenuView);

        if (!GameInterfaceService.Computer.initialized)
            GameInterfaceService.Computer.InvokeMethod("Initialise");
        GameInterfaceService.Computer.enabled = false;

        _betterComputerAsset = await AssetLoader.LoadAsset<GameObject>("Better Computer");
        _clickSound = await AssetLoader.LoadAsset<AudioClip>("ClickSound");

        OnCustomTerminalPrepared += computerTerminal => {
            Logging.Info(computerTerminal.GetSceneIndex());

            GameObject betterComputerAsset = Instantiate(_betterComputerAsset, computerTerminal.transform, false);
            betterComputerAsset.transform.localScale = Vector3.one * 1.02f;

            switch (computerTerminal.GetSceneIndex()) {
                case SceneIndex.GT:
                    if (computerTerminal.gameObject.GetPath().Contains("VirtualStump_CustomMapLobby")) {
                        DestroyImmediate(betterComputerAsset);
                    }
                    else if (computerTerminal.gameObject.GetPath().Contains("SharedBlocksMapSelectLobby")) {
                        betterComputerAsset.transform.localScale = Vector3.one * 1.03f;
                        betterComputerAsset.transform.localPosition = new Vector3(-0.004f, 0.369f, 0.792f);
                        betterComputerAsset.transform.localRotation = Quaternion.Euler(0f, 38f, 359f);
                    }
                    else {
                        goto default;
                    }
                    break;
                case SceneIndex.Bayou:
                    betterComputerAsset.transform.localPosition = new Vector3(1.16f, 0.567f, 1.11f);
                    betterComputerAsset.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
                    break;
                case SceneIndex.Metropolis:
                    betterComputerAsset.transform.localPosition = new Vector3(-0.083f, 0.338f, -0.211f);
                    betterComputerAsset.transform.localRotation = Quaternion.Euler(0f, 1.3f, 0f);
                    break;
                case SceneIndex.GhostReactor:
                case SceneIndex.GhostReactorDrill:
                    betterComputerAsset.transform.localPosition = new Vector3(-0.02f, 0.34f, -0.05f);
                    break;
                default:
                    betterComputerAsset.transform.localPosition = new Vector3(0.01f, 0.34f, -0.25f);
                    break;
            }
        };

        ComputerView.OnTextUpdated += computerText => {
            foreach (ComputerScreenInfo computerScreenInfo in _computerScreenInfos)
                computerScreenInfo.Text = computerText;
        };

        Logging.Info($"Found {SceneManager.GetActiveScene().GetComponentsInHierarchy<GorillaComputerTerminal>().Count} Computers in GorillaTag scene.");
        foreach (GorillaComputerTerminal computerTerminal in SceneManager.GetActiveScene().GetComponentsInHierarchy<GorillaComputerTerminal>()) {
            if (computerTerminal.gameObject.GetPath().Contains("MonkeBlocksRoomPersistent")) {
                computerTerminal.transform.parent.gameObject.SetActive(false);
                continue;
            }

            PrepareCustomTerminal(computerTerminal);
        }

        SceneIndex.MonkeBlocks.AddCallbackOnSceneLoad(() => {
            Scene monkeBlocksScene = SceneManager.GetSceneByBuildIndex((int)SceneIndex.MonkeBlocks);
            GorillaComputerTerminal monkeBlocksTerminal = monkeBlocksScene.GetComponentInHierarchy<GorillaComputerTerminal>();
            monkeBlocksTerminal.transform.parent.gameObject.SetActive(true);
        });

        try {
#if BEPINEX
            foreach (PluginInfo pluginInfo in Chainloader.PluginInfos.Values.Where(pluginInfo => _ciConfig.IsModDisabled(pluginInfo.Metadata.GUID)))
                pluginInfo.Instance.enabled = false;
#endif

            if (NetworkSystem.Instance.WrongVersion) {
                SwitchComputerView(_warningView, [ new WarningView.OutdatedWarning() ]);
                return;
            }

            _mainMenuView.ShowViewEntries(computerViewEntries);
            SwitchComputerView(_mainMenuView, null);
        }
        catch (Exception exception) {
            Logging.Error($"Computer Interface failed to successfully end initializing: {exception.Message}");
        }

        if (!_ciConfig.AcknowledgedSafetyWarning.Value)
            SwitchComputerView(_safetyWarningView, null);

        try {
            using HttpRequestMessage request = new(HttpMethod.Get, "https://raw.githubusercontent.com/DecalFree/ComputerInterface/main/Version.txt");

            using HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string latestVersionRaw = (await response.Content.ReadAsStringAsync()).Trim();
            if (Version.TryParse(latestVersionRaw, out Version latestVersion)) {
                Logging.Info($"Using Computer Interface v{PluginCore.CurrentModLoader.ModVersion} | Latest: {latestVersion}");

                if (latestVersion > Version.Parse(PluginCore.CurrentModLoader.ModVersion))
                    SwitchComputerView(_warningView, [ new WarningView.GeneralWarning($"Computer Interface version {latestVersion} is now available!\nIt is recommended to update to avoid any issues.") ]);
            }
        }
        catch (Exception exception) {
            Logging.Error($"Computer Interface failed to check the its version: {exception.Message}");
        }

        Logging.Info("Successfully ended initializing Computer Interface");
    }

    public override void Tick() {
        if (CurrentComputerView == null)
            return;

        if (!InternetConnected && !_connectionError) {
            _connectionError = true;
            SwitchComputerView(_warningView, [ new WarningView.NoInternetWarning() ]);
            GameInterfaceService.Computer.UpdateFailureText("NO WIFI OR LAN CONNECTION DETECTED.");
        }

        if (InternetConnected && _connectionError) {
            _connectionError = false;
            SwitchComputerView(CurrentComputerView == _warningView ? _mainMenuView : CurrentComputerView, null);
            GameInterfaceService.Computer.InvokeMethod("RestoreFromFailureState", null);
        }
    }

    public async void PrepareCustomTerminal(GorillaComputerTerminal computerTerminal) {
        if (_initializedTerminals.Contains(computerTerminal))
            return;
        _initializedTerminals.Add(computerTerminal);

        Transform computerTerminalScreen = computerTerminal.monitorMesh?.transform ?? computerTerminal.transform.Find("ComputerUI/monitor");

        GameObject monitorAsset = Instantiate(await AssetLoader.LoadAsset<GameObject>("Classic Monitor Screen"), computerTerminalScreen, true);
        monitorAsset.name = $"Computer Interface (Scene - {computerTerminal.gameObject.scene.name})";
        monitorAsset.transform.localPosition = new Vector3(-0.0787f + 0.082f, -0.12f - 0.2f, 0.5344f - 0.02f + 0.003f);
        monitorAsset.transform.localEulerAngles = Vector3.right * 85f;
        monitorAsset.transform.SetParent(computerTerminal.transform);
        monitorAsset.transform.Find("Classic Monitor Prefab").gameObject.AddComponent<GorillaSurfaceOverride>();

        ComputerScreenInfo computerScreenInfo = new() {
            ComputerText = monitorAsset.transform.Find("Canvas/Text (TMP)").GetComponent<TextMeshProUGUI>(),
            ComputerBackground = monitorAsset.transform.Find("Canvas/RawImage").GetComponent<RawImage>(),
            Color = new Color(0.05f, 0.05f, 0.05f)
        };
        computerScreenInfo.Text = CurrentComputerView != null ? CurrentComputerView.Text : "CurrentComputerView is null";
        computerScreenInfo.Color = _ciConfig.ScreenBackgroundColor.Value;
        computerScreenInfo.BackgroundTexture = _ciConfig.BackgroundTexture;
        _computerScreenInfos.Add(computerScreenInfo);

        computerTerminal.monitorMesh?.gameObject.SetActive(false);
        computerTerminal.myFunctionText?.gameObject.SetActive(false);
        computerTerminal.myScreenText?.gameObject.SetActive(false);

        // Let 'activeZones' in ZoneManagement catch up. -DecalFree
        await Task.Delay(3);

        InitializeKeyboard(computerTerminal);
        OnCustomTerminalPrepared?.Invoke(computerTerminal);
    }

    private void InitializeKeyboard(GorillaComputerTerminal computerTerminal) {
        _keyboardButtons = [];

        Dictionary<string, EKeyboardButton> nameToEnum = new();

        foreach (string enumString in Enum.GetNames(typeof(EKeyboardButton))) {
            string button = enumString.Replace("NUM", "").ToLower();
            nameToEnum.Add(button, Enum.Parse<EKeyboardButton>(enumString));
        }

        GorillaKeyboardButton[] buttonArray = computerTerminal.transform.parent?.parent?.Find("GorillaComputerObject")?.GetComponentsInChildren<GorillaKeyboardButton>(true);
        buttonArray ??= computerTerminal.transform.parent?.Find("GorillaComputerObject")?.GetComponentsInChildren<GorillaKeyboardButton>(true);
        buttonArray ??= computerTerminal.GetComponentsInChildren<GorillaKeyboardButton>(true);

        foreach (GorillaKeyboardButton button in buttonArray) {
            if (button.characterString is "up" or "down") {
                button.GetComponentInChildren<MeshRenderer>(true).material.color = new Color(0.1f, 0.1f, 0.1f);
                button.GetComponentInChildren<MeshFilter>().mesh = _cubeMesh;
                button.transform.localPosition -= new Vector3(0, 0.6f, 0);
                DestroyImmediate(button.GetComponent<BoxCollider>());
                if (FindText(button.gameObject, button.name + "text")?.GetComponent<TextMeshPro>() is { } arrowBtnText)
                    DestroyImmediate(arrowBtnText);

                continue;
            }

            if (!nameToEnum.TryGetValue(button.characterString.ToLower(), out EKeyboardButton keyboardButton))
                continue;

            if (FindText(button.gameObject) is { } buttonTextMesh) {
                CustomKeyboardButton customButton = button.gameObject.AddComponent<CustomKeyboardButton>();
                customButton.pressTime = Traverse.Create(computerTerminal.GetComponentsInChildren<GorillaKeyboardButton>()).Field("pressTime").GetValue<float>();
                customButton.isFunctionKey = button.functionKey;

                button.GetComponent<MeshFilter>().mesh = _cubeMesh;
                DestroyImmediate(button);

                customButton.InitializeCustomButton(keyboardButton, buttonTextMesh);
                _keyboardButtons.Add(customButton);
            }
        }

        MeshRenderer keyboardRenderer = _keyboardButtons[0].transform.parent?.parent?.parent?.GetComponent<MeshRenderer>();
        keyboardRenderer ??= _keyboardButtons[0].transform.parent?.parent?.parent?.gameObject.GetComponent<MeshRenderer>();
        keyboardRenderer ??= _keyboardButtons[0].transform.parent?.parent?.parent?.parent?.parent?.parent?.Find("Static/keyboard (1)")?.GetComponent<MeshRenderer>();

        if (keyboardRenderer)
            keyboardRenderer.material.color = new Color(0.3f, 0.3f, 0.3f);

        CustomKeyboardButton enterKey = _keyboardButtons.Last(x => x.KeyboardButton == EKeyboardButton.Enter);
        CustomKeyboardButton mKey = _keyboardButtons.Last(x => x.KeyboardButton == EKeyboardButton.M);
        CustomKeyboardButton deleteKey = _keyboardButtons.Last(x => x.KeyboardButton == EKeyboardButton.Delete);

        CreateKeyboardButton(enterKey.gameObject, "Space", new Vector3(2.6f, 0, 3), EKeyboardButton.Space, "SPACE");
        CreateKeyboardButton(deleteKey.gameObject, "Back", new Vector3(0, 0, -29.8f), EKeyboardButton.Back, "BACK", ColorUtility.TryParseHtmlString("#8787e0", out Color backButtonColor) ? backButtonColor : Color.white);

        bool arrowColorExists = ColorUtility.TryParseHtmlString("#abdbab", out Color arrowKeyButtonColor);

        CustomKeyboardButton leftKey = CreateKeyboardButton(mKey.gameObject, "Left", new Vector3(0, 0, 5.6f), EKeyboardButton.Left, "<", arrowColorExists ? arrowKeyButtonColor : Color.white);
        CustomKeyboardButton downKey = CreateKeyboardButton(leftKey.gameObject, "Down", new Vector3(0, 0, 2.3f), EKeyboardButton.Down, ">", arrowColorExists ? arrowKeyButtonColor : Color.white);
        CreateKeyboardButton(downKey.gameObject, "Right", new Vector3(0, 0, 2.3f), EKeyboardButton.Right, ">", arrowColorExists ? arrowKeyButtonColor : Color.white);
        CustomKeyboardButton upKey = CreateKeyboardButton(downKey.gameObject, "Up", new Vector3(-2.3f, 0, 0), EKeyboardButton.Up, ">", arrowColorExists ? arrowKeyButtonColor : Color.white);

        Transform downKeyText = FindText(downKey.gameObject).transform;
        downKeyText.localPosition -= new Vector3(0, 0, 0.05f);
        downKeyText.localEulerAngles += new Vector3(0, 0, -90);

        Transform upKeyText = FindText(upKey.gameObject).transform;
        upKeyText.localPosition += new Vector3(0, 0, 0.05f);
        upKeyText.localEulerAngles += new Vector3(0, 0, 90);
    }

    private static TextMeshPro FindText(GameObject button, string name = null) {
        // Logging.Info($"Replacing key {button.name} / {name}");
        if (button.GetComponent<TextMeshPro>() is { } text)
            return text;

        if (name.IsNullOrWhiteSpace())
            name = button.name.Replace(" ", "");

        if (name!.Contains("enter"))
            name = "enter";

        // Forest
        Transform t = button.transform.parent?.parent?.parent?.parent?.parent?.parent?.parent?.Find(name);

        // Custom Maps
        t ??= button.transform.parent?.parent?.parent?.parent?.parent?.transform.Find($"UIParent/Text/{name}");

        // Other Maps
        t ??= button.transform.parent?.parent?.Find($"Text/{name}");

        return t?.GetComponent<TextMeshPro>();
    }

    private CustomKeyboardButton CreateKeyboardButton(GameObject prefab, string objectName, Vector3 offset, EKeyboardButton keyboardButton, string label = null, Color? color = null) {
        GameObject newKey = Instantiate(prefab.gameObject, prefab.transform.parent);
        newKey.name = objectName;
        newKey.transform.localPosition += offset;
        newKey.GetComponent<MeshFilter>().mesh = _cubeMesh;
        newKey.GetComponent<Collider>().enabled = true;

        TextMeshPro keyTextMesh = FindText(prefab, prefab.name);
        TextMeshPro newKeyTextMesh = Instantiate(keyTextMesh.gameObject, keyTextMesh.gameObject.transform.parent).GetComponent<TextMeshPro>();
        newKeyTextMesh.name = objectName;
        newKeyTextMesh.transform.localPosition += offset;

        CustomKeyboardButton customKeyboardButton = newKey.GetComponent<CustomKeyboardButton>();

        if (label.IsNullOrWhiteSpace()) {
            customKeyboardButton.InitializeCustomButton(keyboardButton);
        }
        else if (color.HasValue) {
            customKeyboardButton.InitializeCustomButton(keyboardButton, newKeyTextMesh, label, color.Value);
        }
        else {
            customKeyboardButton.InitializeCustomButton(keyboardButton, newKeyTextMesh, label);
        }

        _keyboardButtons.Add(customKeyboardButton);
        return customKeyboardButton;
    }

    internal async void SwitchComputerView(Type sourceView, Type destinationView, object[] arguments) {
        if (sourceView == destinationView)
            return;

        ComputerView newDestinationView = GetOrCreateComputerView(destinationView);
        if (destinationView == null)
            return;
        newDestinationView.CallerComputerView = sourceView;

        CurrentComputerView = newDestinationView;
        try {
            CurrentComputerView.OnViewShown(arguments);
            await Task.Yield();
            CurrentComputerView.UpdateViewScreen();
        }
        catch (Exception exception) {
            Logging.Error($"Error while showing {CurrentComputerView.GetType().Name}: {exception.Message}");
        }
    }

    internal void SwitchComputerView(ComputerView computerView, object[] arguments) => SwitchComputerView(null, computerView.GetType(), arguments);

    internal ComputerView GetOrCreateComputerView(Type type) {
        if (_cachedComputerViews.TryGetValue(type, out ComputerView computerView))
            return computerView;

        ComputerView newComputerView = Activator.CreateInstance(type) as ComputerView;
        _cachedComputerViews.Add(type, newComputerView);
        return newComputerView;
    }

    internal void PressKeyboardButton(CustomKeyboardButton keyboardButton, bool isLeftHand = false) {
        AudioSource audioSource = isLeftHand ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;
        audioSource.PlayOneShot(_clickSound, 0.8f);

        try {
            CurrentComputerView?.OnButtonPressed(keyboardButton.KeyboardButton);
        }
        catch (Exception exception) {
            Logging.Error($"Error in OnButtonPressed for {keyboardButton.KeyboardButton} Button in {CurrentComputerView?.GetType().Name}: {exception.Message}");
        }
    }

    public void SetBackgroundColor(float r, float g, float b) => SetBackgroundColor(new Color(r, g, b));

    public void SetBackgroundColor(Color color) {
        foreach (ComputerScreenInfo computerScreenInfo in _computerScreenInfos) {
            computerScreenInfo.Color = color;
            _ciConfig.ScreenBackgroundColor.Value = computerScreenInfo.Color;
        }
    }

    public Color GetBackgroundColor() => _ciConfig.ScreenBackgroundColor.Value;

    public void SetBackgroundImage(Texture texture, Color? imageColor = null) {
        foreach (ComputerScreenInfo computerScreenInfo in _computerScreenInfos) {
            if (texture == null && imageColor == null) {
                computerScreenInfo.BackgroundTexture = _ciConfig.BackgroundTexture;
                computerScreenInfo.Color = _ciConfig.ScreenBackgroundColor.Value;
                continue;
            }

            if (texture == null) {
                _ciConfig.BackgroundTexture = null;
                computerScreenInfo.BackgroundTexture = _ciConfig.BackgroundTexture;
                computerScreenInfo.Color = _ciConfig.ScreenBackgroundColor.Value;
            }
            else {
                _ciConfig.BackgroundTexture = texture;
                computerScreenInfo.Color = imageColor ?? _ciConfig.ScreenBackgroundColor.Value;
                computerScreenInfo.BackgroundTexture = texture;
            }
        }
    }

    public string GetScreenBackgroundPath() => _ciConfig.ScreenBackgroundPath.Value;

    public Texture GetTexture(string path) => _ciConfig.GetTexture(path);
}