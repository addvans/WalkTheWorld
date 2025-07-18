using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse.AI.Group;
using Verse;

namespace WalkTheWorld.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn_TraderTracker), nameof(Pawn_TraderTracker.GiveSoldThingToPlayer))]
    public static class Pawn_TraderTracker_GiveSoldThingToPlayer_Patch
    {
        private static readonly FieldInfo pawnFieldInfo = AccessTools.Field(typeof(Pawn_TraderTracker), "pawn");
        private static readonly FieldInfo soldPrisonersFieldInfo = AccessTools.Field(typeof(Pawn_TraderTracker), "soldPrisoners");
        public static bool Prefix(Pawn_TraderTracker __instance, Thing toGive, int countToGive, Pawn playerNegotiator)
        {
            Pawn traderPawn = (Pawn)pawnFieldInfo.GetValue(__instance);
            List<Pawn> soldPrisoners = (List<Pawn>)soldPrisonersFieldInfo.GetValue(__instance);
            if (toGive is Pawn pawn)
            {
                pawn.PreTraded(TradeAction.PlayerBuys, playerNegotiator, traderPawn);
                pawn.GetLord()?.Notify_PawnLost(pawn, PawnLostCondition.Undefined);
                if (soldPrisoners.Contains(pawn))
                {
                    soldPrisoners.Remove(pawn);
                }
                if (!pawn.Spawned)
                    GenSpawn.Spawn(pawn, playerNegotiator.Position, playerNegotiator.Map);
                return false;
            }

            IntVec3 positionHeld = toGive.PositionHeld;
            Map mapHeld = toGive.MapHeld;
            Thing thing = toGive.SplitOff(countToGive);
            thing.PreTraded(TradeAction.PlayerBuys, playerNegotiator, traderPawn);
            if (GenPlace.TryPlaceThing(thing, traderPawn.Position, traderPawn.Map, ThingPlaceMode.Near))
            {
                traderPawn.GetLord()?.extraForbiddenThings.Add(thing);
                return false;
            }

            string obj = thing?.ToString();
            IntVec3 intVec = positionHeld;
            thing.Destroy();
            return false;
        }
    }
}
