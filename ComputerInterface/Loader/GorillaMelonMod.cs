#if MELONLOADER

using System;
using ComputerInterface.Loader;
using MelonLoader;

[assembly: VerifyLoaderVersion(0, 7, 2, true)]

[assembly: MelonInfo(typeof(GorillaMelonMod), ComputerInterface.Constants.Name, ComputerInterface.Constants.Version, ComputerInterface.Constants.Author)]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]

namespace ComputerInterface.Loader;

internal class GorillaMelonMod : MelonMod, IModLoader {
    public string LoaderName => "MelonLoader";

    public string ModVersion => $"{Info.SemanticVersion.Major}.{Info.SemanticVersion.Minor}.{Info.SemanticVersion.Patch}";

    public new HarmonyLib.Harmony Harmony { get; private set; }

    public Action<object> OnLogMessage => LoggerInstance.Msg;
    public Action<object> OnLogWarning => LoggerInstance.Warning;
    public Action<object> OnLogError => LoggerInstance.Error;

    public override void OnInitializeMelon() {
        PluginCore.InitializeModLoader(this);
        Harmony = HarmonyInstance;
    }
}

#endif