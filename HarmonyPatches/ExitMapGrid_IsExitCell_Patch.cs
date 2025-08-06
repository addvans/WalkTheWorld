using HarmonyLib;
using System.Reflection;
using Verse;

namespace WalkTheWorld.HarmonyPatches
{
    [HarmonyPatch(typeof(ExitMapGrid), nameof(ExitMapGrid.IsExitCell))]
    public static class ExitMapGrid_IsExitCell_Patch
    {
        private static readonly FieldInfo _mapField =
            typeof(ExitMapGrid).GetField("map", BindingFlags.NonPublic | BindingFlags.Instance);
        public static bool Prefix(ExitMapGrid __instance, ref bool __result)
        {

            Map map = (Map)_mapField.GetValue(__instance);
            if (map?.Parent is VisitCell || WalkTheWorldMod.Settings.disableExitMapGridEverywhere)
            {
                __result = false;
                return false;
            }

            return true;
        }
    }
}
