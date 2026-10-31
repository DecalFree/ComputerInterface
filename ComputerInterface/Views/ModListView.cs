#if BEPINEX

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Bootstrap;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using ComputerInterface.Models;

namespace ComputerInterface.Views;

internal class ModListEntry : IComputerViewEntry {
    public string EntryName => "Mod Status";

    public Type EntryComputerView => typeof(ModListView);
}

internal class ModListView : ComputerView {
    private readonly ModListItem[] _plugins;

    private readonly UIElementPageHandler<ModListItem> _pageHandler = new() {
        EntriesPerPage = 9
    };

    private readonly UISelectionHandler _selectionHandler;

    public ModListView() {
        IEnumerable<PluginInfo> pluginInfos = Chainloader.PluginInfos.Values.Where(plugin => !plugin.Metadata.GUID.Contains(Constants.Guid));
        _plugins = [
            .. pluginInfos.Select(plugin => new ModListItem(plugin, PluginCore.CIConfig)).OrderBy(x => !x.IsSupported)
        ];

        _selectionHandler = new UISelectionHandler(EKeyboardButton.Up, EKeyboardButton.Down, EKeyboardButton.Enter) {
            MaxIndex = _plugins.Length - 1
        };
        _selectionHandler.OnSelected += SelectMod;
        _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}>> </color>", "", "  ", "");

        _pageHandler.SetElements(_plugins);
    }

    private void SelectMod(int index) {
        if (_plugins[index].IsSupported)
            _plugins[index].ToggleMod();

        UpdateViewScreen();
    }

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginColor("ffffff50").Append("== ").EndColor();
        stringBuilder.Append($"Mod Status").BeginColor("ffffff50").Append(" ==").EndColor().AppendLine();

        string labelContents = $"{_plugins.Length} mod{(_plugins.Length == 1 ? "" : "s")} loaded, {_plugins.Count(a => a.IsSupported)} toggleable mod{(_plugins.Count(a => a.IsSupported) == 1 ? "" : "s")} loaded";
        stringBuilder.Append($"<size=40><margin=0.55em>{labelContents}</margin></size>").Append("\n<size=24> </size>");

        const string enabledPrefix = "<color=#00ff00> + </color>";
        const string disabledPrefix = "<color=#ff0000> - </color>";
        const string unsupportedColor = "ffffff50";

        int lineIndex = _pageHandler.MovePageToIndex(_selectionHandler.CurrentSelectionIndex);

        _pageHandler.EnumerateElements((plugin, index) => {
            stringBuilder.AppendLine();
            stringBuilder.Append(plugin.PluginInfo.Instance.enabled ? enabledPrefix : disabledPrefix);
            if (!plugin.IsSupported)
                stringBuilder.BeginColor(unsupportedColor);
            stringBuilder.Append(_selectionHandler.GetIndicatedText(index, lineIndex, plugin.PluginInfo.Metadata.Name));
            if (!plugin.IsSupported)
                stringBuilder.EndColor();
        });

        stringBuilder.AppendLines(2);
        _pageHandler.AppendFooter(stringBuilder);

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        switch (pressedButton) {
            case EKeyboardButton.Option1:
                ShowView<ModView>(_plugins[_selectionHandler.CurrentSelectionIndex]);
                break;
            case EKeyboardButton.Back:
                ReturnToMainMenu();
                break;
            default:
                if (_selectionHandler.HandleButtonPress(pressedButton))
                    UpdateViewScreen();
                break;
        }
    }
}

#endif