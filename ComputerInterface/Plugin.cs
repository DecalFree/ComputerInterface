using System;
using BepInEx;
using GorillaNetworking;
using UnityEngine;

namespace ComputerInterface
{
    [BepInPlugin(PluginInfo.Id, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin {
        internal static CIConfig CIConfig;
        public static CustomComputer CustomComputer;
        public static CommandHandler CommandHandler;
        
        /// <summary>
        /// Specifies if the plugin is loaded
        /// </summary>
        public bool Loaded { get; private set; }
        
        private void Awake() {
            GorillaTagger.OnPlayerSpawned(delegate {
                try {
                    Load();
                }
                catch (Exception exception) {
                    Debug.LogError($"Failed to load ComputerInterface: {exception}");
                }
            });
        }

        private void Load()
        {
            if (Loaded) return;

            HarmonyPatches.ApplyHarmonyPatches();

            Debug.Log("Computer Interface loading");
            
            CIConfig = new CIConfig(Config);
            CustomComputer = FindObjectOfType<GorillaComputer>().gameObject.AddComponent<CustomComputer>();
            CustomComputer.Construct();
            CommandHandler = new CommandHandler();

            Loaded = true;
        }
    }
}
