#if BEPINEX
using BepInEx;

namespace ComputerInterface.Commands;

[BepInDependency("tonimacaroni.computerinterface", "2.1.0")]
[BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
internal class Plugin : BaseUnityPlugin;
#elif MELONLOADER
using ComputerInterface.Commands;
using MelonLoader;

[assembly: VerifyLoaderVersion(0, 7, 2, true)]

[assembly: MelonInfo(typeof(Plugin), ComputerInterface.Commands.Constants.Name, ComputerInterface.Commands.Constants.Version, ComputerInterface.Commands.Constants.Author)]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]

[assembly: MelonAdditionalDependencies("ComputerInterface")]

namespace ComputerInterface.Commands;

internal class Plugin : MelonMod;
#endif