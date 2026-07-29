using System;
using BepInEx;
using BepInEx.Logging;
using ComputerInterface.Behaviors;
using ComputerInterface.Models;
using ComputerInterface.Tools;
using HarmonyLib;
using UnityEngine;

namespace ComputerInterface;

[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
internal class Plugin : BaseUnityPlugin {
    internal new static ManualLogSource Logger;
    internal new static PluginInfo Info;

    internal static CIConfig CIConfig;

    private void Awake() {
        Logger = base.Logger;
        Info = base.Info;

        GorillaTagger.OnPlayerSpawned(delegate {
            try {
                Logging.Info($"Attempting to load {Constants.Name}");

                Harmony.CreateAndPatchAll(GetType().Assembly, Constants.Guid);

                CIConfig = new CIConfig(Config);

                Type[] componentsToAdd = [
                    typeof(Main),
                    typeof(CommandHandler)
                ];
                DontDestroyOnLoad(new GameObject($"{Constants.Name} v{Constants.Version}", componentsToAdd));
            }
            catch (Exception exception) {
                Logging.Error($"Failed to successfully finish loading {Constants.Name}: {exception.Message}");
            }
        });
    }
}