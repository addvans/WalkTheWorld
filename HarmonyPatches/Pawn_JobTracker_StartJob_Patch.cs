using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
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
                {
                    List<JobDef> blacklist = new List<JobDef>
                                {
                                    JobDefOf.Equip,
                                    JobDefOf.HaulToContainer,
                                    JobDefOf.HaulToCell,
                                    JobDefOf.HaulToTransporter,
                                    JobDefOf.ForceTargetWear,
                                    JobDefOf.Ingest,
                                    JobDefOf.TakeFromOtherInventory,
                                    JobDefOf.TakeInventory,
                                    JobDefOf.PickupToHold
                                };
                    if (blacklist.Contains(newJob.def) && newJob.targetA.Thing != null)
                    {
                        Thing itm = newJob.targetA.Thing;
                        if (IsNativeMapItem(itm, itm.Map))
                        {
                            itm.Map.ParentFaction.TryAffectGoodwillWith(Faction.OfPlayer, -(int)(itm.MarketValue * itm.stackCount * 0.1f), reason: HistoryEventDefOf.UsedForbiddenThing); // Штраф к отношениям
                        }

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