using ComputerInterface.Behaviors;
using HarmonyLib;

namespace ComputerInterface.Patches;

[HarmonyPatch(typeof(GorillaComputerTerminal))]
internal static class PatchGorillaComputerTerminal {
    [HarmonyPatch("OnEnable"), HarmonyPostfix]
    private static void PrepareCustomTerminal(GorillaComputerTerminal __instance) => Main.Singleton.PrepareCustomTerminal(__instance);
}