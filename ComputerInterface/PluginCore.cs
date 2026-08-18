using System;
using ComputerInterface.Behaviors;
using ComputerInterface.Loader;
using ComputerInterface.Models;
using ComputerInterface.Tools;
using UnityEngine;

namespace ComputerInterface;

internal static class PluginCore {
    public static IModLoader CurrentModLoader;

    internal static CIConfig CIConfig;

    public static void InitializeModLoader(IModLoader modLoader) {
        if (CurrentModLoader != null)
            throw new Exception($"{Constants.Name} is already loaded.");
        CurrentModLoader = modLoader;

        Logging.Info($"Initializing {Constants.Name} with {CurrentModLoader.LoaderName}");

        GorillaTagger.OnPlayerSpawned(InitializePlugin);
    }

    private static void InitializePlugin() {
        try {
            Logging.Info($"Attempting to load {Constants.Name}");

#if BEPINEX
            CIConfig = new CIConfig(CurrentModLoader.Config);
#elif MELONLOADER
            CIConfig = new CIConfig();
#endif

            Type[] componentsToAdd = [
                typeof(Main),
                typeof(CommandHandler)
            ];
            UnityEngine.Object.DontDestroyOnLoad(new GameObject($"{Constants.Name} v{Constants.Version} - {CurrentModLoader.LoaderName}", componentsToAdd));
        }
        catch (Exception exception) {
            Logging.Error($"Failed to successfully finish loading {Constants.Name}: {exception.Message}");
        }
    }
}