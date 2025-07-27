using ComputerInterface.Extensions;
using GorillaNetworking;
using HarmonyLib;
using System;

namespace ComputerInterface.Patches
{
    [HarmonyPatch(typeof(GorillaComputer), "CheckAutoBanListForName")]
    public class ComputerBanCheckPatch
    {
        public static void Prefix(GorillaComputer __instance)
        {
            if (__instance.GetField<object>("anywhereTwoWeek") == null) __instance.SetField("anywhereTwoWeek", __instance.anywhereTwoWeekFile.text.Split(Environment.NewLine));
            if (__instance.GetField<object>("anywhereOneWeek") == null) __instance.SetField("anywhereOneWeek", __instance.anywhereOneWeekFile.text.Split(Environment.NewLine));
            if (__instance.GetField<object>("exactOneWeek") == null) __instance.SetField("exactOneWeek", __instance.exactOneWeekFile.text.Split(Environment.NewLine));
        }
    }
}
