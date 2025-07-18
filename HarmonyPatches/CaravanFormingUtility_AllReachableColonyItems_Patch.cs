using HarmonyLib;
using RimWorld.Planet;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace WalkTheWorld.HarmonyPatches
{
    [HarmonyPatch(typeof(CaravanFormingUtility), nameof(CaravanFormingUtility.AllReachableColonyItems))]
    public static class CaravanFormingUtility_AllReachableColonyItems_Patch
    {
        static void Postfix(Map map, ref List<Thing> __result)
        {
            if (map.Parent is Settlement settlement && settlement.Faction != Faction.OfPlayer)
            {
                __result.RemoveAll(t => IsNativeMapItem(t, map));
            }
        }

        private static bool IsNativeMapItem(Thing item, Map map)
        {
            return item.spawnedTick <= map.generationTick ||
                   (item.SpawnedOrAnyParentSpawned &&
                   (item.ParentHolder as Pawn)?.Faction != Faction.OfPlayer);
        }
    }
}
