using HarmonyLib;
using System.Reflection;

namespace ComputerInterface
{
    public class HarmonyPatches
    {
        public static Harmony Harmony { get; private set; }

        public static bool IsPatched { get; private set; }
        public const string InstanceId = PluginInfo.Id;

        internal static void ApplyHarmonyPatches()
        {
            if (!IsPatched)
            {
                Harmony ??= new Harmony(InstanceId);
                Harmony.PatchAll(Assembly.GetExecutingAssembly());
                IsPatched = true;
            }
        }

        internal static void RemoveHarmonyPatches()
        {
            if (Harmony != null && IsPatched)
            {
                Harmony.UnpatchSelf();
                IsPatched = false;
            }
        }
    }
}
