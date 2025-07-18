using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace WalkTheWorld
{
    public class WalkTheWorldModSettings : ModSettings
    {

        public int mapSize = 60;
        public int eventChance = 15;
        public int mapCountForEvent = 5;
        public bool showConfirmationPreviewMenu = true;
        public LeavingType leavingType = LeavingType.Selected;
        public CameraFocusMode camFocus = CameraFocusMode.OnEnteredPawns;
        public RandomEventsFilterType eventsFilter = RandomEventsFilterType.Filtered;
        public List<string> mutatorsToDelete = DefDatabase<TileMutatorDef>.AllDefs
                        .Where(m => m.defName != null && (
                            m.defName.Contains("Ancient") ||
                            m.defName.Contains("Abandoned") ||
                            m.defName.Contains("Stockpile") ||
                            m.defName.Contains("Ruins")))
                        .Select(m => m.defName) // Сохраняем только имена
                        .ToList();
        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref mapSize, "mapSize", 60);
            Scribe_Values.Look(ref eventChance, "eventChance", 15);
            Scribe_Values.Look(ref mapCountForEvent, "mapCountForEvent", 5);
            Scribe_Values.Look(ref leavingType, "leavingType", LeavingType.Selected);
            Scribe_Values.Look(ref eventsFilter, "eventsFilter", RandomEventsFilterType.Filtered);
            Scribe_Values.Look(ref camFocus, "camFocus", CameraFocusMode.OnEnteredPawns);
            Scribe_Values.Look(ref showConfirmationPreviewMenu, "showConfirmationPreviewMenu", true);
            // Сохраняем как список имён
            Scribe_Collections.Look(ref mutatorsToDelete, "mutatorsToDeleteNames", LookMode.Value);
        }
    }

}
