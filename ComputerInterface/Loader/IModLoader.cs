using System;
#if BEPINEX
using BepInEx.Configuration;
#endif

namespace ComputerInterface.Loader;

internal interface IModLoader {
    string LoaderName { get; }

    string ModVersion { get; }

    HarmonyLib.Harmony Harmony { get; }

    Action<object> OnLogMessage { get; }
    Action<object> OnLogWarning { get; }
    Action<object> OnLogError { get; }

#if BEPINEX
    ConfigFile Config { get; }
#endif
}