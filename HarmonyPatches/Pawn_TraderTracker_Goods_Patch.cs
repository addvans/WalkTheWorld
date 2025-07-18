using HarmonyLib;
using RimWorld.Planet;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace WalkTheWorld.HarmonyPatches
{
    [HarmonyPatch(typeof(Pawn_TraderTracker), "Goods", MethodType.Getter)]
    public static class Pawn_TraderTracker_Goods_Patch
    {
        private static readonly FieldInfo pawnFieldInfo = AccessTools.Field(typeof(Pawn_TraderTracker), "pawn");
        public static bool Prefix(Pawn_TraderTracker __instance, ref IEnumerable<Thing> __result)
        {
            Pawn traderPawn = (Pawn)pawnFieldInfo.GetValue(__instance);
            // Получаем поселение, к которому принадлежит торговец
            if (traderPawn.Map.Parent is Settlement settlement)
            {
                if (settlement.Faction == traderPawn.Faction)
                    if (settlement != null && settlement.trader != null)
                    {
                        __result = FilterNotForSale(settlement.trader.StockListForReading, traderPawn);
                        return false;
                    }
            }
            return true;
        }
        private static IEnumerable<Thing> FilterNotForSale(List<Thing> stock, Pawn pawn)
        {
            for (int i = 0; i < stock.Count; i++)
            {
                Thing thing = stock[i];

                if (!pawn.inventory.NotForSale(thing))
                {
                    yield return thing;
                }
            }
        }
    }
}
