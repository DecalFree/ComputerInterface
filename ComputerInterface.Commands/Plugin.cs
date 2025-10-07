using BepInEx;

namespace ComputerInterface.Commands {
    [BepInDependency(Constants.Guid)]
    [BepInPlugin(PluginID, PluginName, Constants.Version)]
    public class Plugin : BaseUnityPlugin {
        private const string PluginID = "tonimacaroni.computerinterface.commands";
        private const string PluginName = "Computer Interface Commands";
    }
}