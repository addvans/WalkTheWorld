using HarmonyLib;
using RimWorld.Planet;
using RimWorld;

namespace WalkTheWorld.HarmonyPatches
{
    [HarmonyPatch(typeof(SettlementDefeatUtility), nameof(SettlementDefeatUtility.CheckDefeated))]
    public static class SettlementDefeatUtility_CheckDefeated_Patch
    {
        static bool Prefix(Settlement factionBase)
        {
            if (factionBase.Faction == null)
                return true;
            if (factionBase.Faction == Faction.OfPlayer)
                return true;
            FactionRelationKind relationKind = factionBase.Faction.RelationWith(Faction.OfPlayer).kind;
            if (relationKind == FactionRelationKind.Ally ||
                relationKind == FactionRelationKind.Neutral)
            {
                return false;
            }

            return true;
        }
    }
}
