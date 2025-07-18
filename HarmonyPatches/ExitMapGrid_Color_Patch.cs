using HarmonyLib;
using System.Reflection;
using UnityEngine;
using Verse;

namespace WalkTheWorld.HarmonyPatches
{
    [HarmonyPatch(typeof(ExitMapGrid), "Color", MethodType.Getter)]
    public static class ExitMapGrid_Color_Patch
    {
        private static readonly FieldInfo _mapField =
            typeof(ExitMapGrid).GetField("map", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(ExitMapGrid __instance, ref Color __result)
        {
            if (_mapField == null) return;

            Map map = (Map)_mapField.GetValue(__instance);
            if (map?.Parent is VisitCell)
            {
                __result = Color.clear;
            }
        }
    }
}
