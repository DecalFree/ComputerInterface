using BepInEx.Bootstrap;
using ComputerInterface.Extensions;
using ComputerInterface.Interfaces;
using ComputerInterface.ViewLib;
using HarmonyLib;
using System;
using System.Linq;
using System.Text;

namespace ComputerInterface.Views
{
    internal class ModListEntry : IComputerModEntry
    {
        public string EntryName => "Mod Status";
        public Type EntryViewType => typeof(ModListView);
    }

    internal class ModListView : ComputerView
    {
        internal class ModListItem
        {
            private readonly CIConfig _config;

            public BepInEx.PluginInfo PluginInfo { get; private set; }
            public bool Supported { get; private set; }

            public ModListItem(BepInEx.PluginInfo pluginInfo, CIConfig config)
            {
                _config = config;
                PluginInfo = pluginInfo;
                Supported = DoesModImplementFeature();
            }

            private bool DoesModImplementFeature()
            {
                System.Reflection.MethodInfo onEnable = AccessTools.Method(PluginInfo.Instance.GetType(), "OnEnable");
                System.Reflection.MethodInfo onDisable = AccessTools.Method(PluginInfo.Instance.GetType(), "OnDisable");
                return onEnable != null && onDisable != null;
            }

            public void EnableMod()
            {
                PluginInfo.Instance.enabled = true;
                _config.RemoveDisabledMod(PluginInfo.Metadata.GUID);
            }

            public void DisableMod()
            {
                PluginInfo.Instance.enabled = false;
                _config.AddDisabledMod(PluginInfo.Metadata.GUID);
            }

            public void ToggleMod()
            {
                if (PluginInfo.Instance.enabled)
                {
                    DisableMod();
                }
                else
                {
                    EnableMod();
                }
            }
        }

        private readonly CIConfig _config;

        private readonly ModListItem[] _plugins;

        private readonly UIElementPageHandler<ModListItem> _pageHandler;
        private readonly UISelectionHandler _selectionHandler;

        public ModListView() {
            _config = Plugin.CIConfig;

            System.Collections.Generic.IEnumerable<BepInEx.PluginInfo> pluginInfos = Chainloader.PluginInfos.Values.Where(plugin => !plugin.Metadata.GUID.Contains(PluginInfo.Id));
            _plugins = pluginInfos.Select(plugin => new ModListItem(plugin, _config)).OrderBy(x => !x.Supported).ToArray();
            _selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter)
            {
                MaxIdx = _plugins.Length - 1
            };
            _selectionHandler.OnSelected += SelectMod;
            _selectionHandler.ConfigureSelectionIndicator($"<color=#{PrimaryColor}>> </color>", "", "  ", "");

            _pageHandler = new UIElementPageHandler<ModListItem>
            {
                EntriesPerPage = 9
            };

            _pageHandler.SetElements(_plugins);
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            Redraw();
        }

        private void Redraw()
        {
            StringBuilder builder = new();

            RedrawHeader(builder);
            DrawMods(builder);

            Text = builder.ToString();
        }

        private void RedrawHeader(StringBuilder str)
        {
            str.BeginColor("ffffff50").Append("== ").EndColor();
            str.Append($"Mod Status").BeginColor("ffffff50").Append(" ==").EndColor().AppendLine();

            string labelContents = $"{_plugins.Length} mod{(_plugins.Length == 1 ? "" : "s")} loaded, {_plugins.Count(a => a.Supported)} toggleable mod{(_plugins.Count(a => a.Supported) == 1 ? "" : "s")} loaded";
            str.Append($"<size=40><margin=0.55em>{labelContents}</margin></size>").Append("\n<size=24> </size>");
        }

        private void DrawMods(StringBuilder str)
        {
            string enabledPrefix = "<color=#00ff00> + </color>";
            string disabledPrefix = "<color=#ff0000> - </color>";
            string unsupportedColor = "ffffff50";

            int lineIdx = _pageHandler.MovePageToIdx(_selectionHandler.CurrentSelectionIndex);

            _pageHandler.EnumarateElements((plugin, idx) =>
            {
                str.AppendLine();
                str.Append(plugin.PluginInfo.Instance.enabled ? enabledPrefix : disabledPrefix);
                if (!plugin.Supported) str.BeginColor(unsupportedColor);
                str.Append(_selectionHandler.GetIndicatedText(idx, lineIdx, plugin.PluginInfo.Metadata.Name));
                if (!plugin.Supported) str.EndColor();
                //str.Append(plugin.PluginInfo.Instance.enabled ? enabledPostfix : disabledPostfix);
            });

            str.AppendLines(2);
            _pageHandler.AppendFooter(str);
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
                    ShowView<ModView>(_plugins[_selectionHandler.CurrentSelectionIndex]);
                    break;
                case EKeyboardKey.Back:
                    ReturnToMainMenu();
                    break;
            }
        }

        private void SelectMod(int idx)
        {
            if (_plugins[idx].Supported)
            {
                _plugins[idx].ToggleMod();
            }
            Redraw();
        }
    }
}