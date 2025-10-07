using BepInEx.Bootstrap;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using HarmonyLib;
using System;
using System.Linq;
using System.Text;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;

namespace ComputerInterface.Views;

internal class ModListEntry : IComputerModEntry {
    public string EntryName => "Mod Status";
    public Type EntryViewType => typeof(ModListView);
}

internal class ModListView : ComputerView {
    internal class ModListItem {
        private readonly CIConfig _config;

        public BepInEx.PluginInfo PluginInfo { get; private set; }
        public bool Supported { get; private set; }

        public ModListItem(BepInEx.PluginInfo pluginInfo, CIConfig config) {
            _config = config;
            PluginInfo = pluginInfo;
            Supported = DoesModImplementFeature();
        }

        private bool DoesModImplementFeature() {
            var onEnable = AccessTools.Method(PluginInfo.Instance.GetType(), "OnEnable");
            var onDisable = AccessTools.Method(PluginInfo.Instance.GetType(), "OnDisable");
            return onEnable != null && onDisable != null;
        }

        private void EnableMod() {
            PluginInfo.Instance.enabled = true;
            _config.RemoveDisabledMod(PluginInfo.Metadata.GUID);
        }

        private void DisableMod() {
            PluginInfo.Instance.enabled = false;
            _config.AddDisabledMod(PluginInfo.Metadata.GUID);
        }

        public void ToggleMod() {
            if (PluginInfo.Instance.enabled) {
                DisableMod();
            }
            else {
                EnableMod();
            }
        }
    }

    private readonly ModListItem[] _plugins;

    private readonly UIElementPageHandler<ModListItem> _pageHandler = new() {
        EntriesPerPage = 9
    };
    private readonly UISelectionHandler _selectionHandler;

    public ModListView() {
        var pluginInfos = Chainloader.PluginInfos.Values.Where(plugin => !plugin.Metadata.GUID.Contains(Constants.Guid));
        _plugins = pluginInfos.Select(plugin => new ModListItem(plugin, Plugin.CIConfig)).OrderBy(x => !x.Supported).ToArray();
        _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter) {
            MaxIdx = _plugins.Length - 1
        };
        _selectionHandler.OnSelected += SelectMod;
        _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}>> </color>", "", "  ", "");

        _pageHandler.SetElements(_plugins);
    }

    public override void OnShow(object[] args) {
        base.OnShow(args);
        Redraw();
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();

        RedrawHeader(stringBuilder);
        DrawMods(stringBuilder);

        Text = stringBuilder.ToString();
    }

    private void RedrawHeader(StringBuilder stringBuilder) {
        stringBuilder.BeginColor("ffffff50").Append("== ").EndColor();
        stringBuilder.Append($"Mod Status").BeginColor("ffffff50").Append(" ==").EndColor().AppendLine();

        var labelContents = $"{_plugins.Length} mod{(_plugins.Length == 1 ? "" : "s")} loaded, {_plugins.Count(a => a.Supported)} toggleable mod{(_plugins.Count(a => a.Supported) == 1 ? "" : "s")} loaded";
        stringBuilder.Append($"<size=40><margin=0.55em>{labelContents}</margin></size>").Append("\n<size=24> </size>");
    }

    private void DrawMods(StringBuilder stringBuilder) {
        const string enabledPrefix = "<color=#00ff00> + </color>";
        const string disabledPrefix = "<color=#ff0000> - </color>";
        const string unsupportedColor = "ffffff50";

        var lineIdx = _pageHandler.MovePageToIdx(_selectionHandler.CurrentSelectionIndex);

        _pageHandler.EnumerateElements((plugin, idx) => {
            stringBuilder.AppendLine();
            stringBuilder.Append(plugin.PluginInfo.Instance.enabled ? enabledPrefix : disabledPrefix);
            if (!plugin.Supported)
                stringBuilder.BeginColor(unsupportedColor);
            stringBuilder.Append(_selectionHandler.GetIndicatedText(idx, lineIdx, plugin.PluginInfo.Metadata.Name));
            if (!plugin.Supported)
                stringBuilder.EndColor();
            // stringBuilder.Append(plugin.PluginInfo.Instance.enabled ? enabledPrefix : disabledPrefix);
        });

        stringBuilder.AppendLines(2);
        _pageHandler.AppendFooter(stringBuilder);
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        if (_selectionHandler.HandleKeypress(key)) {
            Redraw();
            return;
        }

        switch (key) {
            case EKeyboardKey.Option1:
                ShowView<ModView>(_plugins[_selectionHandler.CurrentSelectionIndex]);
                break;
            case EKeyboardKey.Back:
                ReturnToMainMenu();
                break;
        }
    }

    private void SelectMod(int idx) {
        if (_plugins[idx].Supported)
            _plugins[idx].ToggleMod();
        Redraw();
    }
}