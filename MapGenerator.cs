using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Verse;
using Verse.AI;
using Verse.Noise;
using static System.Net.WebRequestMethods;
using static UnityEngine.GraphicsBuffer;

namespace WalkTheWorld
{
    public class MapGenerator
    {
        private readonly int targetTile;
        public Map generatedMap;
        private static int mapSize => WalkTheWorldMod.Settings.mapSize;
        private static int eventChance => WalkTheWorldMod.Settings.eventChance;
        private static int mapCountForEvent => WalkTheWorldMod.Settings.mapCountForEvent;
        static int mapsSinceLastEvent = 0;
        private IntVec3 desidedSize => new IntVec3(mapSize, 1, mapSize);
        WorldObjectDef decidedDef;
        Settlement settlement;
        bool hasLandmark = false;
        public MapGenerator(PlanetTile tile)
        {
            this.targetTile = tile.tileId;
            WorldObjectDef def = DefDatabase<WorldObjectDef>.GetNamed("ExplorationTile");

            WorldObject worldObject = Find.WorldObjects.AllWorldObjects
                .FirstOrDefault(w => w.Tile == targetTile && w.def == def);
            hasLandmark = tile.Tile.Landmark != null;
            // Проверяем существующие объекты на тайле
            var existingObjects = Find.WorldObjects.AllWorldObjects.Where(w => w.Tile == targetTile).ToList();
            // Проверяем, является ли цель дружественным поселением
            bool isFriendlySettlement = existingObjects.OfType<Settlement>()
                                                     .Any(s => s.Faction != null &&
                                                              s.Faction != Faction.OfPlayer &&
                                                              s.Faction.PlayerGoodwill >= 0);

            if (isFriendlySettlement)
            {
                // Для дружественных поселений используем специальный метод входа
                Settlement friendlySettlement = existingObjects.OfType<Settlement>()
                                                             .First(s => s.Faction.PlayerGoodwill >= 0);


                settlement = friendlySettlement;
                decidedDef = friendlySettlement.def;
                // Создаем лорда для визита (не атаки)
                // LordJob_VisitColony lordJob = new LordJob_VisitColony(friendlySettlement.Faction);
                //LordMaker.MakeNewLord(friendlySettlement.Faction, lordJob, map);
            }
            else if (existingObjects.Any(o => o is MapParent || o is Site || o is Settlement))
            {
                // Если есть значимые объекты - используем их логику входа
                var primaryObject = existingObjects.FirstOrDefault(o => o is Settlement || o is Site) ?? existingObjects.First();
                decidedDef = primaryObject.def;

            }
            else
            {
                decidedDef = DefDatabase<WorldObjectDef>.GetNamed("ExplorationTile");
            }

        }
        public static void TransferWeatherEvent(Map fromMap, Map toMap)
        {
            foreach(var b in fromMap.gameConditionManager.ActiveConditions)
                toMap.gameConditionManager.RegisterCondition(b);
        }
        public static bool TryCreateEventForMap(Map map)
        {
            try
            {
                if (eventChance <= 0)
                    return false;
                if (!(UnityEngine.Random.Range(1, 100) <= eventChance || (mapCountForEvent > 0 & (mapsSinceLastEvent >= mapCountForEvent))))
                    return false;
                // Создаем IncidentParms с настройками
                IncidentParms parms = new IncidentParms
                {
                    target = map,
                    points = StorytellerUtility.DefaultThreatPointsNow(map),
                    forced = true // Игнорировать проверки и кулдауны
                };
                var c = DefDatabase<IncidentDef>.AllDefs;
                List<IncidentDef> b = new List<IncidentDef>();
                if (WalkTheWorldMod.Settings.eventsFilter == RandomEventsFilterType.Filtered)
                    b = c.Where(x => x.TargetAllowed(map)).ToList();
                else
                    b = c.ToList();
                
                // Создаем FiringIncident
                FiringIncident fi = new FiringIncident(
                    def: b.RandomElement(), // Если null - будет выбран случайный инцидент
                    Find.Storyteller.storytellerComps.FirstOrDefault(),
                    parms: parms
                );

                // Пытаемся запустить событие
                if (!Find.Storyteller.TryFire(fi))
                    return TryCreateEventForMap(map);
                mapsSinceLastEvent = 0;
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Error generating event! I will try to generate it again.{ex.ToString()}");
                return TryCreateEventForMap(map);
            }

        }
        public static void SpawnSettlementTrader(Map map, Settlement settlement)
        {
            // Создаем паука-торговца
            Pawn traderPawn = map.mapPawns.AllPawnsSpawned.FirstOrDefault(p => p.Faction == settlement.Faction &&
                           p.RaceProps.Humanlike &&
                           p.trader == null &&
                           !p.IsPrisoner &&
                           !p.Downed);
            if (traderPawn.trader == null)
            {
                traderPawn.trader = new Pawn_TraderTracker(traderPawn);
            }

            traderPawn.trader.traderKind = settlement.trader.TraderKind;
            traderPawn.mindState.wantsToTradeWithColony = true;
        }
       
        public void StartGeneration()
        {
            bool transferweather = false;
            Map map = Current.Game.FindMap(targetTile);
            if(map == null)
                transferweather = true;
            if (settlement != null || decidedDef != DefDatabase<WorldObjectDef>.GetNamed("ExplorationTile") || hasLandmark)
               generatedMap = GetOrGenerateMapUtility.GetOrGenerateMap(targetTile, decidedDef);
            else
               generatedMap = GetOrGenerateMapUtility.GetOrGenerateMap(targetTile, desidedSize, decidedDef);
            if(transferweather)
                TransferWeatherEvent(Find.CurrentMap, generatedMap);
            if (settlement != null)
            {
                SpawnSettlementTrader(generatedMap, settlement);
            }
            else
                if (!TryCreateEventForMap(generatedMap))
                   mapsSinceLastEvent += 1;


        }
    }
}
