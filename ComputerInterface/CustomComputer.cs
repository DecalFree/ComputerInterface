using BepInEx.Bootstrap;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using ComputerInterface.Views;
using GorillaExtensions;
using GorillaNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace ComputerInterface
{
    public class CustomComputer : MonoBehaviour, IInitializable
    {
        private bool _initialized;

        private GorillaComputer _gorillaComputer;
        private ComputerViewController _computerViewController;

        private readonly Dictionary<Type, IComputerView> _cachedViews = new();

        private ComputerViewPlaceholderFactory _viewFactory;

        private MainMenuView _mainMenuView;
        private WarnView _warningView;

        private readonly List<CustomScreenInfo> _customScreenInfos = new();

        private List<CustomKeyboardKey> _keys;

        private readonly Mesh CubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

        private AssetsLoader _assetsLoader;

        private CIConfig _config;

        private AudioClip _clickSound;

        private bool _internetConnected => Application.internetReachability != NetworkReachability.NotReachable;
        private bool _connectionError;

        void Awake()
        {
            enabled = false;
        }

        [Inject]
        internal async void Construct(
            CIConfig config,
            AssetsLoader assetsLoader,
            MainMenuView mainMenuView,
            WarnView warningView,
            ComputerViewPlaceholderFactory viewFactory,
            List<IComputerModEntry> computerModEntries
            )
        {
            if (_initialized) return;
            _initialized = true;

            Debug.Log($"Found {computerModEntries.Count} physicalComputer mod entries");

            _config = config;
            _assetsLoader = assetsLoader;

            _mainMenuView = mainMenuView;
            _warningView = warningView;
            _cachedViews.Add(typeof(MainMenuView), _mainMenuView);

            _viewFactory = viewFactory;

            _gorillaComputer = GetComponent<GorillaComputer>();
            _gorillaComputer.enabled = false;
            _gorillaComputer.InvokeMethod("Awake");
            _gorillaComputer.InvokeMethod("SwitchState", GorillaComputer.ComputerState.Startup, true);

            _computerViewController = new ComputerViewController();
            _computerViewController.OnTextChanged += SetText;
            _computerViewController.OnSwitchView += SwitchView;
            _computerViewController.OnSetBackground += SetBGImage;
            PrepareMonitor(SceneManager.GetActiveScene(), SceneManager.GetActiveScene().GetComponentInHierarchy<GorillaComputerTerminal>().gameObject.transform.GetChild(0).GetPath());

            _clickSound = await _assetsLoader.GetAsset<AudioClip>("ClickSound");

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;

            try
            {
                BaseGameInterface.InitAll();
                ShowInitialView(_mainMenuView, computerModEntries);
            }
            catch (Exception ex) { Debug.LogError($"CI: Failed to successfully end initalizing the mod: {ex}"); }

            enabled = true;
            Debug.Log("Initialized computers");
        }

        private void ShowInitialView(MainMenuView view, List<IComputerModEntry> computerModEntries)
        {
            foreach (BepInEx.PluginInfo pluginInfo in Chainloader.PluginInfos.Values)
            {
                if (!_config.IsModDisabled(pluginInfo.Metadata.GUID)) continue;
                pluginInfo.Instance.enabled = false;
            }

            if (NetworkSystem.Instance.WrongVersion)
            {
                _computerViewController.SetView(_warningView, new object[] { new WarnView.OutdatedWarning() });
                return;
            }
            _computerViewController.SetView(view, null);
            view.ShowEntries(computerModEntries);
        }

        public void Initialize() { }

        private void Update()
        {
            // get key state for the key debugging feature
            if (CustomKeyboardKey.KeyDebuggerEnabled && _keys != null)
            {
                foreach (CustomKeyboardKey key in _keys)
                {
                    key.Fetch();
                }
            }

            // Make sure the physicalComputer is ready
            if (_computerViewController.CurrentComputerView != null)
            {
                // Check to see if our connection is off
                if (!_internetConnected && !_connectionError)
                {
                    _connectionError = true;
                    _computerViewController.SetView(_warningView, new object[] { new WarnView.NoInternetWarning() });
                    _gorillaComputer.UpdateFailureText("NO WIFI OR LAN CONNECTION DETECTED.");
                }

                // Check to see if we're back online
                if (_internetConnected && _connectionError)
                {
                    _connectionError = false;
                    _computerViewController.SetView(_computerViewController.CurrentComputerView == _warningView ? _mainMenuView : _computerViewController.CurrentComputerView, null);
                    _gorillaComputer.InvokeMethod("RestoreFromFailureState", null);
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            string sceneName = scene.name;

            if (loadMode == LoadSceneMode.Additive)
            {
                if (sceneName == "Cave")
                    PrepareMonitor(scene, "Cave_Main_Prefab/OldCave/MinesComputer/GorillaComputerObject/ComputerUI");
                
                switch (ZoneManagement.instance.activeZones.First())
                {
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

        private void OnSceneUnloaded(Scene scene)
        {
            string sceneName = scene.name;
            CustomScreenInfo screenInfo = _customScreenInfos.FirstOrDefault(info => info.SceneName == sceneName);

            if (screenInfo != null)
            {
                _customScreenInfos.Remove(screenInfo);
            }
        }

        public void SetText(string text)
        {
            foreach (CustomScreenInfo customScreenInfo in _customScreenInfos)
            {
                customScreenInfo.Text = text;
            }
        }

        public void SetBG(float r, float g, float b) => SetBG(new Color(r, g, b));

        public void SetBG(Color color)
        {
            foreach (CustomScreenInfo customScreenInfo in _customScreenInfos)
            {
                customScreenInfo.Color = color;
                _config.ScreenBackgroundColor.Value = customScreenInfo.Color;
            }
        }

        public Color GetBG()
        {
            return _config.ScreenBackgroundColor.Value;
        }

        public void SetBGImage(ComputerViewChangeBackgroundEventArgs args)
        {
            foreach (CustomScreenInfo customScreenInfo in _customScreenInfos)
            {
                if (args == null || args.Texture == null)
                {
                    customScreenInfo.BackgroundTexture = _config.BackgroundTexture;
                    customScreenInfo.Color = _config.ScreenBackgroundColor.Value;
                    return;
                }

                customScreenInfo.Color = args.ImageColor ?? _config.ScreenBackgroundColor.Value;
                customScreenInfo.BackgroundTexture = args.Texture;
            }
        }

        public void PressButton(CustomKeyboardKey key, bool isLeftHand = false)
        {
            AudioSource audioSource = isLeftHand ? GorillaTagger.Instance.offlineVRRig.leftHandPlayer : GorillaTagger.Instance.offlineVRRig.rightHandPlayer;
            audioSource.PlayOneShot(_clickSound, 0.8f);

            _computerViewController.NotifyOfKeyPress(key.KeyboardKey);
        }

        private void SwitchView(ComputerViewSwitchEventArgs args)
        {
            if (args.SourceType == args.DestinationType) return;

            IComputerView destinationView = GetOrCreateView(args.DestinationType);

            if (destinationView == null)
            {
                return;
            }

            destinationView.CallerViewType = args.SourceType;
            _computerViewController.SetView(destinationView, args.Args);
        }

        private IComputerView GetOrCreateView(Type type)
        {
            if (_cachedViews.TryGetValue(type, out IComputerView view))
            {
                return view;
            }

            ComputerView newView = _viewFactory.Create(type);
            _cachedViews.Add(type, newView);
            return newView;
        }

        private async void PrepareMonitor(Scene scene, string computerPath)
        {
            scene.TryFindByPath(computerPath, out Transform computer);
            GameObject physicalComputer = computer.gameObject;

            try
            {
                ReplaceKeys(physicalComputer);

                CustomScreenInfo screenInfo = await CreateMonitor(physicalComputer, scene.name);
                screenInfo.Text = _computerViewController.CurrentComputerView != null ? _computerViewController.CurrentComputerView.Text : "Loading";
                screenInfo.Color = _config.ScreenBackgroundColor.Value;
                screenInfo.BackgroundTexture = _config.BackgroundTexture;

                _customScreenInfos.Add(screenInfo);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void ReplaceKeys(GameObject computer)
        {
            _keys = new List<CustomKeyboardKey>();

            Dictionary<string, EKeyboardKey> nameToEnum = new();

            foreach (string enumString in Enum.GetNames(typeof(EKeyboardKey)))
            {
                string key = enumString.Replace("NUM", "").ToLower();
                nameToEnum.Add(key, (EKeyboardKey)Enum.Parse(typeof(EKeyboardKey), enumString));
            }

            GorillaKeyboardButton[] buttonArray = computer.transform.parent?.parent?.Find("GorillaComputerObject")?.GetComponentsInChildren<GorillaKeyboardButton>(true) ?? null;
            buttonArray ??= buttonArray = computer.transform.parent?.Find("GorillaComputerObject")?.GetComponentsInChildren<GorillaKeyboardButton>(true) ?? null;
            buttonArray ??= computer.GetComponentsInChildren<GorillaKeyboardButton>(true) ?? null;

            foreach (GorillaKeyboardButton button in buttonArray)
            {
                if (button.characterString == "up" || button.characterString == "down")
                {
                    button.GetComponentInChildren<MeshRenderer>(true).material.color = new Color(0.1f, 0.1f, 0.1f);
                    button.GetComponentInChildren<MeshFilter>().mesh = CubeMesh;
                    button.transform.localPosition -= new Vector3(0, 0.6f, 0);
                    DestroyImmediate(button.GetComponent<BoxCollider>());
                    if (FindText(button.gameObject, button.name + "text")?.GetComponent<TextMeshPro>() is TextMeshPro arrowBtnText)
                    {
                        DestroyImmediate(arrowBtnText);
                    }
                    continue;
                }

                if (!nameToEnum.TryGetValue(button.characterString.ToLower(), out EKeyboardKey key)) continue;

                if (FindText(button.gameObject) is TextMeshPro buttonText)
                {
                    CustomKeyboardKey customButton = button.gameObject.AddComponent<CustomKeyboardKey>();
                    customButton.pressTime = Traverse.Create(computer.GetComponentsInChildren<GorillaKeyboardButton>()).Field("pressTime").GetValue<float>();
                    customButton.functionKey = button.functionKey;

                    button.GetComponent<MeshFilter>().mesh = CubeMesh;
                    DestroyImmediate(button);

                    customButton.Init(this, key, buttonText);
                    _keys.Add(customButton);
                }
            }

            MeshRenderer keyboardRenderer = _keys[0].transform.parent?.parent?.parent?.GetComponent<MeshRenderer>() ?? null;
            keyboardRenderer ??= _keys[0].transform.parent.parent.parent.gameObject?.GetComponent<MeshRenderer>() ?? null;
            keyboardRenderer ??= _keys[0].transform.parent.parent.parent?.parent?.parent?.parent?.Find("Static/keyboard (1)")?.GetComponent<MeshRenderer>() ?? null;

            if (keyboardRenderer) keyboardRenderer.material.color = new Color(0.3f, 0.3f, 0.3f);

            CustomKeyboardKey enterKey = _keys.Last(x => x.KeyboardKey == EKeyboardKey.Enter);
            CustomKeyboardKey mKey = _keys.Last(x => x.KeyboardKey == EKeyboardKey.M);
            CustomKeyboardKey deleteKey = _keys.Last(x => x.KeyboardKey == EKeyboardKey.Delete);

            CreateKey(enterKey.gameObject, "Space", new Vector3(2.6f, 0, 3), EKeyboardKey.Space, "SPACE");
            CreateKey(deleteKey.gameObject, "Back", new Vector3(0, 0, -29.8f), EKeyboardKey.Back, "BACK", ColorUtility.TryParseHtmlString("#8787e0", out Color backButtonColor) ? backButtonColor : Color.white);

            bool arrowColourExists = ColorUtility.TryParseHtmlString("#abdbab", out Color arrowKeyButtonColor);

            CustomKeyboardKey leftKey = CreateKey(mKey.gameObject, "Left", new Vector3(0, 0, 5.6f), EKeyboardKey.Left, "<", arrowColourExists ? arrowKeyButtonColor : Color.white);
            CustomKeyboardKey downKey = CreateKey(leftKey.gameObject, "Down", new Vector3(0, 0, 2.3f), EKeyboardKey.Down, ">", arrowColourExists ? arrowKeyButtonColor : Color.white);
            CreateKey(downKey.gameObject, "Right", new Vector3(0, 0, 2.3f), EKeyboardKey.Right, ">", arrowColourExists ? arrowKeyButtonColor : Color.white);
            CustomKeyboardKey upKey = CreateKey(downKey.gameObject, "Up", new Vector3(-2.3f, 0, 0), EKeyboardKey.Up, ">", arrowColourExists ? arrowKeyButtonColor : Color.white);

            Transform downKeyText = FindText(downKey.gameObject).transform;
            downKeyText.localPosition -= new Vector3(0, 0, 0.05f);
            downKeyText.localEulerAngles += new Vector3(0, 0, -90);

            Transform upKeyText = FindText(upKey.gameObject).transform;
            upKeyText.localPosition += new Vector3(0, 0, 0.05f);
            upKeyText.localEulerAngles += new Vector3(0, 0, 90);
        }

        private static TextMeshPro FindText(GameObject button, string name = null)
        {
            //Debug.Log($"Replacing key {button.name} / {name}");
            if (button.GetComponent<TextMeshPro>() is TextMeshPro text)
            {
                return text;
            }

            if (name.IsNullOrWhiteSpace())
            {
                name = button.name.Replace(" ", "");
            }

            if (name.Contains("enter"))
            {
                name = "enter";
            }

            // Forest
            Transform t = button.transform.parent?.parent?.parent?.parent?.parent?.parent?.parent?.Find(name);

            // Other Maps
            t ??= button.transform.parent?.parent?.Find("Text/" + name);

            return t.GetComponent<TextMeshPro>();
        }

        private CustomKeyboardKey CreateKey(GameObject prefab, string goName, Vector3 offset, EKeyboardKey key,
            string label = null, Color? color = null)
        {
            GameObject newKey = Instantiate(prefab.gameObject, prefab.transform.parent);
            newKey.name = goName;
            newKey.transform.localPosition += offset;
            newKey.GetComponent<MeshFilter>().mesh = CubeMesh;
            newKey.GetComponent<Collider>().enabled = true;

            TextMeshPro keyText = FindText(prefab, prefab.name);
            TextMeshPro newKeyText = Instantiate(keyText.gameObject, keyText.gameObject.transform.parent).GetComponent<TextMeshPro>();
            newKeyText.name = goName;
            newKeyText.transform.localPosition += offset;

            CustomKeyboardKey customKeyboardKey = newKey.GetComponent<CustomKeyboardKey>();

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

        private async Task<CustomScreenInfo> CreateMonitor(GameObject physicalComputer, string sceneName)
        {
            GameObject monitorAsset = await _assetsLoader.GetAsset<GameObject>("Classic Monitor");
            GameObject newMonitor = Instantiate(monitorAsset);

            newMonitor.name = $"Computer Interface (Scene - {sceneName})";
            newMonitor.transform.SetParent(physicalComputer.transform.Find("monitor") ?? physicalComputer.transform.Find("monitor (1)"), false);
            newMonitor.transform.localPosition = new Vector3(-0.0787f, -0.12f, 0.5344f);
            newMonitor.transform.localEulerAngles = Vector3.right * 90f;
            newMonitor.transform.SetParent(physicalComputer.transform.parent, true);
            newMonitor.transform.Find("Main Monitor").gameObject.AddComponent<GorillaSurfaceOverride>();

            CustomScreenInfo info = new()
            {
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

        private void RemoveMonitor(GameObject computer, string sceneName)
        {
            GameObject monitor = null;

            // The GorillaTag and Ranked scene have two objects that aren't properly parented.
            // This causes the text ComputerInterface is replacing to remain visible.
            if (!ZoneManagement.IsInZone(GTZone.monkeBlocks) && !ZoneManagement.IsInZone(GTZone.monkeBlocksShared))
            {
                switch (sceneName)
                {
                    case "GorillaTag":
                    case "Ranked":
                        computer.transform.parent?.parent?.parent?.Find("FunctionSelect").gameObject?.SetActive(false);
                        computer.transform.parent?.parent?.parent?.Find("Data").gameObject?.SetActive(false);
                        break;
                }
            }

            foreach (Transform child in computer.transform)
            {
                if (child.name.StartsWith("monitor"))
                {
                    monitor = child.gameObject;
                    monitor.SetActive(false);
                }
            }

            if (monitor is null)
            {
                Debug.Log("Unable to find monitor");
                return;
            }

            // Stable for now 
            if (computer.TryGetComponent(out GorillaComputerTerminal terminal))
            {
                terminal.monitorMesh?.gameObject?.SetActive(false);
                terminal.myFunctionText?.gameObject?.SetActive(false);
                terminal.myScreenText?.gameObject?.SetActive(false);
            }

            Transform monitorTransform = computer.transform.parent.parent?.Find("GorillaComputerObject/ComputerUI/monitor") ?? null;
            monitorTransform ??= computer.transform.parent?.Find("GorillaComputerObject/ComputerUI/monitor") ?? null;
            monitorTransform ??= computer.transform?.Find("GorillaComputerObject/ComputerUI/monitor") ?? null;
            monitorTransform?.gameObject?.SetActive(false);
        }
    }
}
