using HarmonyLib;
using RimWorld;
using System;
using Verse.AI;
using Verse;
using RimWorld.Planet;

namespace WalkTheWorld.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
    public static class Pawn_JobTracker_StartJob_Patch
    {
        private static bool IsNativeMapItem(Thing item, Map map)
        {
            try
            {
                if (map == null)
                    return false;
                if (item.spawnedTick <= map.generationTick)
                    return true;
                return item.SpawnedOrAnyParentSpawned &&
               (item.ParentHolder as Pawn)?.Faction == Faction.OfPlayer;
            }
            catch (Exception)
            {
                return false;
            }
        }
        static void Postfix(Pawn ___pawn, Job newJob)
        {
            try
            {
                if (___pawn.Faction == null)
                    return;
                if (___pawn.Faction != Faction.OfPlayer)
                    return;
                if (newJob == null && newJob.targetA == null)
                    return;
                if (___pawn.Map.Parent is Settlement)
                    if ((newJob.def == JobDefOf.Equip || newJob.def == JobDefOf.ForceTargetWear || newJob.def == JobDefOf.Ingest || newJob.def == JobDefOf.TakeFromOtherInventory || newJob.def == JobDefOf.TakeInventory || newJob.def == JobDefOf.PickupToHold) && newJob.targetA.Thing != null)
                    {
                        Thing itm = newJob.targetA.Thing;
                        Log.Message($"{itm}");

                        if (IsNativeMapItem(itm, itm.Map))
                        {
                            itm.Map.ParentFaction.TryAffectGoodwillWith(Faction.OfPlayer, -10, reason: HistoryEventDefOf.UsedForbiddenThing); // Штраф к отношениям
                        }

                    }
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}