using System;
using BepInEx;
using BepInEx.Logging;
using ComputerInterface.Behaviours;
using ComputerInterface.Models;
using ComputerInterface.Tools;
using GorillaNetworking;
using HarmonyLib;

namespace ComputerInterface;

[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
public class Plugin : BaseUnityPlugin {
    internal new static ManualLogSource Logger;
    internal new static PluginInfo Info;
    
    internal static CIConfig CIConfig;
    
    private void Awake() {
        Logger = base.Logger;
        Info = base.Info;
        
        GorillaTagger.OnPlayerSpawned(delegate {
            try {
                Harmony.CreateAndPatchAll(GetType().Assembly, Constants.Guid);
                Logging.Info("Loading Computer Interface");

                CIConfig = new CIConfig(Config);
                FindFirstObjectByType<GorillaComputer>().gameObject.AddComponent<CustomComputer>();
                new CommandHandler();
            }
            catch (Exception exception) {
                Logging.Error($"Failed to load Computer Interface: {exception}");
            }
        });
    }
}