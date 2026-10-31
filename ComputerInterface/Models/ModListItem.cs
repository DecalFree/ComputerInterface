#if BEPINEX

using System.Reflection;
using BepInEx;
using HarmonyLib;

namespace ComputerInterface.Models;

internal class ModListItem {
    private readonly CIConfig _config;

    public PluginInfo PluginInfo { get; }
    public bool IsSupported { get; }

    public ModListItem(PluginInfo pluginInfo, CIConfig config) {
        _config = config;
        PluginInfo = pluginInfo;
        IsSupported = IsModSupported();
    }

    private bool IsModSupported() {
        MethodInfo onEnable = AccessTools.Method(PluginInfo.Instance.GetType(), "OnEnable");
        MethodInfo onDisable = AccessTools.Method(PluginInfo.Instance.GetType(), "OnDisable");
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

#endif