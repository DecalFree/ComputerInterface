#if BEPINEX

using System;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;

namespace ComputerInterface.Loader;

[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
internal class GorillaBepInExMod : BaseUnityPlugin, IModLoader {
    public string LoaderName => "BepInEx";

    public string ModVersion => Info.Metadata.Version.ToString();

    public Harmony Harmony { get; set; }

    public Action<object> OnLogMessage => Logger.LogMessage;
    public Action<object> OnLogWarning => Logger.LogWarning;
    public Action<object> OnLogError => Logger.LogError;

    public void InitializeHarmony() => Harmony = Harmony.CreateAndPatchAll(GetType().Assembly, Constants.Guid);

    public new ConfigFile Config => base.Config;

    public GorillaBepInExMod() => PluginCore.InitializeModLoader(this);
}

#endif