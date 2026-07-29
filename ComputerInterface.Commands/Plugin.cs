using BepInEx;

namespace ComputerInterface.Commands;

[BepInDependency("tonimacaroni.computerinterface")]
[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
internal class Plugin : BaseUnityPlugin;