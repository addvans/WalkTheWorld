using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WalkTheWorld.HarmonyPatches
{
    [HarmonyPatch(typeof(ExitMapGrid), nameof(ExitMapGrid.MapUsesExitGrid), MethodType.Getter)]
    public static class ExitMapGrid_MapUsesExitGrid_Patch
    {
        private static readonly FieldInfo _mapField =
        typeof(ExitMapGrid).GetField("map", BindingFlags.NonPublic | BindingFlags.Instance);
        public static bool Prefix(ExitMapGrid __instance, ref bool __result)
        {
            Map map = (Map)_mapField.GetValue(__instance);

            if (map.Parent is VisitCell || WalkTheWorldMod.Settings.disableExitMapGridEverywhere)
            {
                __result = true;
                return false;
            }

            return true;
        }
    }
}
