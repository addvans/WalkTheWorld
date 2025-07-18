using RimWorld.Planet;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace WalkTheWorld
{
    public class VisitCell : Camp
    {
        public bool affected = false;
        public static bool IsAffectedByPlayer(Map map)
        {
            // Проверяем любые постройки игрока
            if (map.listerBuildings.allBuildingsColonist.Any())
                return true;

            // Проверяем мебель, предметы и другие созданные игроком вещи
            if (map.listerThings.ThingsInGroup(ThingRequestGroup.HaulableAlways).Any(t => t.Faction == Faction.OfPlayer))
                return true;


            // Проверяем зоны, созданные игроком
            if (map.zoneManager.AllZones.Any(z => z is Zone_Stockpile || z is Zone_Growing))
                return true;

            return false;
        }

        public bool DoPawnBlockRemove()
        {
            return base.Map.mapPawns.AnyPawnBlockingMapRemoval;
        }
        public override void Notify_MyMapRemoved(Map map)
        {
            List<WorldObjectComp> allComps = base.AllComps;
            for (int i = 0; i < allComps.Count; i++)
            {
                allComps[i].PostMyMapRemoved();
            }

            QuestUtility.SendQuestTargetSignals(questTags, "MapRemoved", this.Named("SUBJECT"));
        }
        public bool TaskedToRemove = false;
        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            if (TaskedToRemove)
            {
                alsoRemoveWorldObject = true;
                return true;
            }
            if (!base.Map.mapPawns.AnyPawnBlockingMapRemoval)
            {
                if (!affected)
                    affected = IsAffectedByPlayer(this.Map);
                if (!affected)
                {
                    if (ModsConfig.OdysseyActive && this.Map.TileInfo.Landmark != null)
                    {
                        List<TileMutatorDef> listToRemove = WalkTheWorldMod.Settings.mutatorsToDelete
                             .Select(defName => DefDatabase<TileMutatorDef>.GetNamedSilentFail(defName))
                             .Where(def => def != null) // Отфильтровываем null (если Def не найден)
                             .ToList();
                        foreach (var mut in listToRemove)
                            if (this.Map.TileInfo.Mutators.Contains(mut))
                                this.Map.TileInfo.Mutators.Remove(mut);
                    }
                    alsoRemoveWorldObject = true;
                    return true;
                }
            }
            alsoRemoveWorldObject = false;
            return false;
        }
        public override void Notify_CaravanFormed(Caravan caravan)
        {
            base.Notify_CaravanFormed(caravan);
        }
        public override void PostMapGenerate()
        {
            base.PostMapGenerate();

        }
    }
}
