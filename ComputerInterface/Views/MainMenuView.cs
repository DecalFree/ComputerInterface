using BepInEx.Bootstrap;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ComputerInterface.Views
{
    public class MainMenuView : ComputerView
    {
        private List<IComputerModEntry> _modEntries;
        private readonly List<IComputerModEntry> _shownEntries;
        private readonly Dictionary<IComputerModEntry, BepInEx.PluginInfo> _pluginInfoMap;

        private readonly UIElementPageHandler<IComputerModEntry> _pageHandler;
        private readonly UISelectionHandler _selectionHandler;

        public MainMenuView()
        {
            _selectionHandler =
                new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);
            _selectionHandler.OnSelected += ShowModView;
            _selectionHandler.ConfigureSelectionIndicator("<color=#ed6540>></color> ", "", "  ", "");

            _pageHandler = new UIElementPageHandler<IComputerModEntry>(EKeyboardKey.Left, EKeyboardKey.Right);
            _pageHandler.Footer = "<color=#ffffff50>{0}{1}        <align=\"right\"><margin-right=2em>Page {2}/{3}</margin></align></color>";
            _pageHandler.NextMark = "▼";
            _pageHandler.PrevMark = "▲";
            _pageHandler.EntriesPerPage = 8;

            _shownEntries = new List<IComputerModEntry>();
            _pluginInfoMap = new Dictionary<IComputerModEntry, BepInEx.PluginInfo>();
        }

        public void ShowEntries(List<IComputerModEntry> entries)
        {
            _modEntries = entries;

            // Map entries to plugin infos
            _pluginInfoMap.Clear();
            foreach (IComputerModEntry entry in entries)
            {
                System.Reflection.Assembly asm = entry.GetType().Assembly;
                BepInEx.PluginInfo pluginInfo =
                    Chainloader.PluginInfos.Values.FirstOrDefault(x => x.Instance.GetType().Assembly == asm);
                if (pluginInfo != null)
                {
                    _pluginInfoMap.Add(entry, pluginInfo);
                }
            }

            FilterEntries();

            Redraw();
        }

        public void FilterEntries()
        {
            _shownEntries.Clear();
            List<IComputerModEntry> customEntries = new();
            foreach (IComputerModEntry entry in _modEntries)
            {
                if (!_pluginInfoMap.TryGetValue(entry, out BepInEx.PluginInfo info)) continue;

                if (info.Instance.GetType().Assembly == GetType().Assembly)
                {
                    _shownEntries.Add(entry);
                }
                else
                {
                    customEntries.Add(entry);
                }
            }
            _shownEntries.AddRange(customEntries);
            _selectionHandler.MaxIdx = _shownEntries.Count - 1;
            _pageHandler.SetElements(_shownEntries.ToArray());
        }

        public void Redraw()
        {
            StringBuilder builder = new();

            DrawHeader(builder);
            DrawMods(builder);

            SetText(builder);
        }

        public void DrawHeader(StringBuilder str)
        {
            str.BeginCenter().MakeBar('-', SCREEN_WIDTH, 0, "ffffff10");
            str.AppendClr(PluginInfo.Name, PrimaryColor)
                .EndColor()
                .Append(" - v")
                .Append(PluginInfo.Version).AppendLine();

            str.Append("Computer Interface created by ").AppendClr("Toni Macaroni", "9be68a").AppendLine();

            str.MakeBar('-', SCREEN_WIDTH, 0, "ffffff10").EndAlign().AppendLine();
        }

        public void DrawMods(StringBuilder str)
        {
            int lineIdx = _pageHandler.MovePageToIdx(_selectionHandler.CurrentSelectionIndex);

            _pageHandler.EnumarateElements((entry, idx) =>
            {
                str.Append(_selectionHandler.GetIndicatedText(idx, lineIdx, entry.EntryName));
                str.AppendLine();
            });

            _pageHandler.AppendFooter(str);
            str.AppendLine();
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            if (_modEntries == null) return;
            FilterEntries();
            Redraw();
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (_selectionHandler.HandleKeypress(key))
            {
                Redraw();
                return;
            }

            switch (key)
            {
                case EKeyboardKey.Option1:
                    if (NetworkSystem.Instance.InRoom)
                    {
                        BaseGameInterface.Disconnect();
                    }
                    break;
            }
        }

        public void ShowModView(int idx)
        {
            ShowView(_shownEntries[idx].EntryViewType);
        }
    }
}