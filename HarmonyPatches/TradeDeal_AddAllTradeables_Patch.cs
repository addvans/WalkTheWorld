using HarmonyLib;
using RimWorld.Planet;
using RimWorld;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace WalkTheWorld.HarmonyPatches
{
    [HarmonyPatch(typeof(TradeDeal), "AddAllTradeables")]
    public static class TradeDeal_AddAllTradeables_Patch
    {

        private static readonly MethodInfo AddToTradeablesMethod =
        AccessTools.Method(typeof(TradeDeal), "AddToTradeables", new[] { typeof(Thing), typeof(Transactor) });
        private static readonly FieldInfo TradeablesField =
        AccessTools.Field(typeof(TradeDeal), "tradeables");
        static bool Prefix(TradeDeal __instance)
        {
            if (TradeSession.playerNegotiator.Map != null)
            {
                var map = TradeSession.playerNegotiator.Map;
                if (map.Parent is Settlement settlement)
                    if (settlement.Faction != null)
                        if (settlement.Faction != Faction.OfPlayer)
                        {
                            var tradeables = (List<Tradeable>)TradeablesField.GetValue(__instance);
                            foreach (Thing item in TradeSession.playerNegotiator.inventory.innerContainer)
                            {
                                if (!TradeUtility.PlayerSellableNow(item, TradeSession.trader))
                                    continue;
                                AddToTradeablesMethod.Invoke(__instance, new object[] { item, Transactor.Colony });

                            }
                            if (!TradeSession.giftMode)
                            {
                                foreach (Thing good in TradeSession.trader.Goods)
                                {
                                    AddToTradeablesMethod.Invoke(__instance, new object[] { good, Transactor.Trader });
                                }
                            }

                            if (!TradeSession.giftMode && tradeables.Find((Tradeable x) => x.IsCurrency) == null)
                            {
                                Thing thing = ThingMaker.MakeThing(ThingDefOf.Silver);
                                thing.stackCount = 0;
                                AddToTradeablesMethod.Invoke(__instance, new object[] { thing, Transactor.Trader });
                            }

                            if (TradeSession.TradeCurrency == TradeCurrency.Favor)
                            {
                                tradeables.Add(new Tradeable_RoyalFavor());
                            }
                            return false; 
                        }
            }
            return true;

        }
    }

}
