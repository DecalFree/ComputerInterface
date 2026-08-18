using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
#if BEPINEX
using BepInEx;
using BepInEx.Bootstrap;
#endif
using ComputerInterface.Behaviors;
using ComputerInterface.Behaviors.UI;
using ComputerInterface.Enumerations;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using ComputerInterface.Models;
#if MELONLOADER
using MelonLoader;
#endif

namespace ComputerInterface.Views;

public class MainMenuView : ComputerView {
    private List<IComputerViewEntry> _viewEntries = [];
    private readonly List<IComputerViewEntry> _shownEntries = [];
#if BEPINEX
    private readonly Dictionary<IComputerViewEntry, PluginInfo> _plugins = [];
#elif MELONLOADER
    private readonly Dictionary<IComputerViewEntry, MelonMod> _plugins = [];
#endif

    private readonly UIElementPageHandler<IComputerViewEntry> _pageHandler = new(EKeyboardButton.Left, EKeyboardButton.Right) {
        Footer = "<color=#ffffff50>{0}{1}        <align=\"right\"><margin-right=2em>Page {2}/{3}</margin></align></color>",
        NextMark = "▼",
        PrevMark = "▲",
        EntriesPerPage = 8
    };
    private readonly UISelectionHandler _selectionHandler = new(EKeyboardButton.Up, EKeyboardButton.Down, EKeyboardButton.Enter);

    public MainMenuView() {
        _selectionHandler.OnSelected += ShowComputerView;
        _selectionHandler.ConfigureSelectionIndicator("<color=#ed6540>></color> ", "", "  ", "");
    }

    private void ShowComputerView(int index) => ShowView(_shownEntries[index].EntryComputerView);

    public void ShowViewEntries(List<IComputerViewEntry> viewEntries) {
        _viewEntries = viewEntries;

        _plugins.Clear();
        foreach (IComputerViewEntry viewEntry in viewEntries) {
            Assembly assembly = viewEntry.GetType().Assembly;
#if BEPINEX
            PluginInfo plugin = Chainloader.PluginInfos.Values.FirstOrDefault(x => x.Instance.GetType().Assembly == assembly);
#elif MELONLOADER
            MelonMod plugin = MelonMod.RegisteredMelons.FirstOrDefault(x => x.GetType().Assembly == assembly);
#endif
            if (plugin != null)
                _plugins.Add(viewEntry, plugin);
        }

        FilterViewEntries();
    }

    private void FilterViewEntries() {
        _shownEntries.Clear();
        List<IComputerViewEntry> customEntries = [];
        foreach (IComputerViewEntry viewEntry in _viewEntries) {
#if BEPINEX
            if (!_plugins.TryGetValue(viewEntry, out PluginInfo info))
                continue;
#elif MELONLOADER
            if  (!_plugins.TryGetValue(viewEntry, out MelonMod info))
                continue;
#endif

            if (info.GetType().Assembly == GetType().Assembly) {
                _shownEntries.Add(viewEntry);
            }
            else {
                customEntries.Add(viewEntry);
            }
        }
        _shownEntries.AddRange(customEntries);
        _selectionHandler.MaxIndex = _shownEntries.Count - 1;
        _pageHandler.SetElements([.. _shownEntries]);
    }

    public override void OnViewShown(object[] arguments) {
        if (_viewEntries == null)
            return;

        FilterViewEntries();
    }

    protected override string GetViewText() {
        StringBuilder stringBuilder = new();

        stringBuilder.BeginCenter().MakeBar('-', ScreenWidth, 0, "ffffff10");
        stringBuilder.AppendClr(Constants.Name, PrimaryColor).EndColor().Append(" - v").Append(Constants.Version).AppendLine();

        stringBuilder.Append("Computer Interface by ").AppendClr("Toni Macaroni", "9be68a").AppendLine();

        stringBuilder.MakeBar('-', ScreenWidth, 0, "ffffff10").EndAlign().AppendLine();

        int lineIndex = _pageHandler.MovePageToIndex(_selectionHandler.CurrentSelectionIndex);

        _pageHandler.EnumerateElements((entry, index) => {
            stringBuilder.Append(_selectionHandler.GetIndicatedText(index, lineIndex, entry.EntryName));
            stringBuilder.AppendLine();
        });

        _pageHandler.AppendFooter(stringBuilder);
        stringBuilder.AppendLine();

        return stringBuilder.ToString();
    }

    public override void OnButtonPressed(EKeyboardButton pressedButton) {
        if (_selectionHandler.HandleButtonPress(pressedButton)) {
            UpdateViewScreen();
            return;
        }

        switch (pressedButton) {
            case EKeyboardButton.Option1:
                if (NetworkSystem.Instance.InRoom)
                    GameInterfaceService.ReturnToSinglePlayer();
                break;
        }
    }
}