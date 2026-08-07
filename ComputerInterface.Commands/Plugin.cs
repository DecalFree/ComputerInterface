using BepInEx;

namespace ComputerInterface.Commands;

[BepInDependency("tonimacaroni.computerinterface", "2.0.0")]
[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
internal class Plugin : BaseUnityPlugin;