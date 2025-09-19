using System;
using BepInEx;
using UnityEngine;

namespace ComputerInterface.Commands
{
    [BepInDependency(PluginInfo.Id)]
    [BepInPlugin(PLUGIN_ID, PLUGIN_NAME, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_ID = "tonimacaroni.computerinterface.commands";
        public const string PLUGIN_NAME = "Computer Interface Commands";
        
        private void Awake() {
            GorillaTagger.OnPlayerSpawned(delegate {
                try {
                    new CommandRegistrar().Initialize();
                }
                catch (Exception exception) {
                    Debug.LogError($"Failed to load ComputerInterface.Commands: {exception}");
                }
            });
        }
    }
}
