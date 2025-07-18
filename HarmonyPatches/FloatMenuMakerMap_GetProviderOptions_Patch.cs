using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using Verse;

namespace WalkTheWorld.HarmonyPatches
{
    [HarmonyPatch(typeof(FloatMenuMakerMap), "GetProviderOptions")]
    public static class FloatMenuMakerMap_GetProviderOptions_Patch
    {
        private static bool IsValidQuestGiver(Pawn pawn)
        {
            if (pawn.Faction != null)
                return !pawn.Faction.IsPlayer && pawn.mindState.wantsToTradeWithColony; // Не в коме/сне
            return false;
        }
        private static void GiveQuest(Pawn questGiver, int dif, Pawn negotiator)
        {
            try
            {
                float threatpoints = StorytellerUtility.DefaultThreatPointsNow(Find.World);
                switch (dif)
                {
                    case 1:
                        threatpoints *= 2;
                        break;
                    case 2:
                        threatpoints *= 20;
                        break;
                    case 3:
                        threatpoints *= threatpoints;
                        break;
                }
                Slate slate = new Slate();
                QuestScriptDef chosen;
                slate.Set("asker", questGiver.Faction.leader);
                slate.Set("points", threatpoints);
                
                if (questGiver.Faction.def.techLevel >= TechLevel.Spacer)
                {
                    if (UnityEngine.Random.Range(0, 2) == 0)
                    {
                        slate.Set<TaggedString>("discoveryMethod", "QuestDiscoveredFromOrbitalScanner".Translate(), false);
                        chosen = QuestUtility.GetGiverQuests(QuestGiverTag.OrbitalScanner).RandomElementByWeight(
                            (QuestScriptDef q) => NaturalRandomQuestChooser.GetNaturalRandomSelectionWeight(q, threatpoints, Find.World.StoryState));
                    }
                    else
                    {
                        slate.Set<TaggedString>("discoveryMethod", "QuestDiscoveredFromTrader".Translate(questGiver.Named("TRADER"), negotiator.Named("NEGOTIATOR")), false);
                        chosen = QuestUtility.GetGiverQuests(QuestGiverTag.Traders).RandomElementByWeight(
                                (QuestScriptDef q) => NaturalRandomQuestChooser.GetNaturalRandomSelectionWeight(q, threatpoints, Find.World.StoryState));
                    }
                }
                else
                    {
                    slate.Set<TaggedString>("discoveryMethod", "QuestDiscoveredFromTrader".Translate(questGiver.Named("TRADER"), negotiator.Named("NEGOTIATOR")), false);
                    chosen = QuestUtility.GetGiverQuests(QuestGiverTag.Traders).RandomElementByWeight(
                                (QuestScriptDef q) => NaturalRandomQuestChooser.GetNaturalRandomSelectionWeight(q, threatpoints, Find.World.StoryState));
                    }
                QuestUtility.SendLetterQuestAvailable(QuestUtility.GenerateQuestAndMakeAvailable(chosen, slate));
                
            }
            catch (Exception ex)
            {
                Log.Error($"{questGiver}'s quest thrown an error: {ex}");
            }
        }
        public static void DifficultyWindow(Pawn questGiver, Pawn negotiator)
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            string[] diffs = { "Easy", "Medium", "Hard", "Extreme" };
            for (int i = 0; i < diffs.Length; i++)
            {
                int index = i; // Локальная копия для замыкания
                options.Add(new FloatMenuOption(
                    diffs[index],
                    () => {
                        GiveQuest(questGiver, index, negotiator);
                    }
                ));
            }
            Find.WindowStack.Add(new FloatMenu(options));
        }
        public static void Postfix(FloatMenuContext context, List<FloatMenuOption> options)
        {
            foreach (Pawn clickedPawn in context.ClickedPawns)
            {
                if (!IsValidQuestGiver(clickedPawn)) continue;

                // Расстояние до кликнутого пешки
                float distance = float.MaxValue;
                foreach (Pawn pawn in Find.Selector.SelectedPawns)
                    distance = Mathf.Min(distance, clickedPawn.Position.DistanceTo(pawn.Position));

                if (distance > 5f)
                {
                    options.Add(new FloatMenuOption(
                        "WTW_TooFarToTakeQuest".Translate(),
                        null,
                        MenuOptionPriority.DisabledOption
                    ));
                }
                else
                {
                    options.Add(new FloatMenuOption(
                        "WTW_TakeQuest".Translate(),
                        () => DifficultyWindow(clickedPawn, context.FirstSelectedPawn),
                        revalidateClickTarget: clickedPawn
                    ));
                }
            }
        }
    }
}
