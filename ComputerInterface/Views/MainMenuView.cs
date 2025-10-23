using BepInEx.Bootstrap;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ComputerInterface.Behaviours;
using ComputerInterface.Enumerations;
using ComputerInterface.Models;
using ComputerInterface.Models.UI;

namespace ComputerInterface.Views;

public class MainMenuView : ComputerView {
    private List<IComputerModEntry> _modEntries;
    private readonly List<IComputerModEntry> _shownEntries = [];
    private readonly Dictionary<IComputerModEntry, BepInEx.PluginInfo> _pluginInfoMap = new();

    private readonly UIElementPageHandler<IComputerModEntry> _pageHandler = new(EKeyboardKey.Left, EKeyboardKey.Right) {
        Footer = "<color=#ffffff50>{0}{1}        <align=\"right\"><margin-right=2em>Page {2}/{3}</margin></align></color>",
        NextMark = "▼",
        PrevMark = "▲",
        EntriesPerPage = 8
    };
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);

    public MainMenuView() {
        _selectionHandler.OnSelected += ShowModView;
        _selectionHandler.ConfigureSelectionIndicator("<color=#ed6540>></color> ", "", "  ", "");
    }

    public void ShowEntries(List<IComputerModEntry> entries) {
        _modEntries = entries;

        // Map entries to plugin infos
        _pluginInfoMap.Clear();
        foreach (var entry in entries) {
            var assembly = entry.GetType().Assembly;
            var pluginInfo = Chainloader.PluginInfos.Values.FirstOrDefault(x => x.Instance.GetType().Assembly == assembly);
            if (pluginInfo != null)
                _pluginInfoMap.Add(entry, pluginInfo);
        }

        FilterEntries();

        Redraw();
    }

    private void FilterEntries() {
        _shownEntries.Clear();
        List<IComputerModEntry> customEntries = [];
        foreach (var entry in _modEntries) {
            if (!_pluginInfoMap.TryGetValue(entry, out var info))
                continue;

            if (info.Instance.GetType().Assembly == GetType().Assembly) {
                _shownEntries.Add(entry);
            }
            else {
                customEntries.Add(entry);
            }
        }
        _shownEntries.AddRange(customEntries);
        _selectionHandler.MaxIdx = _shownEntries.Count - 1;
        _pageHandler.SetElements(_shownEntries.ToArray());
    }

    private void Redraw() {
        var stringBuilder = new StringBuilder();

        DrawHeader(stringBuilder);
        DrawMods(stringBuilder);

        SetText(stringBuilder);
    }

    private void DrawHeader(StringBuilder stringBuilder) {
        stringBuilder.BeginCenter().MakeBar('-', ScreenWidth, 0, "ffffff10");
        stringBuilder.AppendClr(Constants.Name, PrimaryColor).EndColor().Append(" - v").Append(Constants.Version).AppendLine();

        stringBuilder.Append("Computer Interface by ").AppendClr("Toni Macaroni", "9be68a").AppendLine();

        stringBuilder.MakeBar('-', ScreenWidth, 0, "ffffff10").EndAlign().AppendLine();
    }

    private void DrawMods(StringBuilder stringBuilder) {
        var lineIdx = _pageHandler.MovePageToIdx(_selectionHandler.CurrentSelectionIndex);

        _pageHandler.EnumerateElements((entry, idx) => {
            stringBuilder.Append(_selectionHandler.GetIndicatedText(idx, lineIdx, entry.EntryName));
            stringBuilder.AppendLine();
        });

        _pageHandler.AppendFooter(stringBuilder);
        stringBuilder.AppendLine();
    }

    public override void OnShow(object[] args) {
        base.OnShow(args);
        if (_modEntries == null)
            return;
        FilterEntries();
        Redraw();
    }

    public override void OnKeyPressed(EKeyboardKey key) {
        if (_selectionHandler.HandleKeypress(key)) {
            Redraw();
            return;
        }

        switch (key) {
            case EKeyboardKey.Option1:
                if (NetworkSystem.Instance.InRoom)
                    BaseGameInterface.Disconnect();
                break;
        }
    }

    private void ShowModView(int idx) =>
        ShowView(_shownEntries[idx].EntryViewType);
}