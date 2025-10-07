using ComputerInterface.Extensions;
using GorillaNetworking;
using HarmonyLib;
using System;
using UnityEngine;

namespace ComputerInterface.Patches;

[HarmonyPatch(typeof(GorillaComputer))]
internal static class PatchGorillaComputer {
    [HarmonyPatch("CheckAutoBanListForName"), HarmonyPrefix]
    public static void CheckAutoBanListForNamePrefix(GorillaComputer __instance) {
        if (__instance.GetField<object>("anywhereTwoWeek") == null)
            __instance.SetField("anywhereTwoWeek", __instance.anywhereTwoWeekFile.text.Split(Environment.NewLine));
        
        if (__instance.GetField<object>("anywhereOneWeek") == null)
            __instance.SetField("anywhereOneWeek", __instance.anywhereOneWeekFile.text.Split(Environment.NewLine));
        
        if (__instance.GetField<object>("exactOneWeek") == null)
            __instance.SetField("exactOneWeek", __instance.exactOneWeekFile.text.Split(Environment.NewLine));
    }
    
    [HarmonyPatch("InitializeGameMode"), HarmonyPrefix]
    public static bool InitializeGameModePrefix(GorillaComputer __instance) {
        __instance.leftHanded = PlayerPrefs.GetInt("leftHanded", 0) == 1;
        __instance.OnModeSelectButtonPress(PlayerPrefs.GetString("currentGameMode", "INFECTION"), __instance.leftHanded);
        return false;
    }
}