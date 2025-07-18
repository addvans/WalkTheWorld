using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using RimWorld.Planet;
using WalkTheWorld;

namespace WalkTheWorld.HarmonyPatches
{
    [HarmonyPatch(typeof(WorldObject), "GetGizmos")]
    public static class WorldObject_GetGizmos_Patch
    {
        static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> __result, WorldObject __instance)
        {
            foreach (var gizmo in __result)
            {
                yield return gizmo;
            }

            if (__instance is VisitCell visitCell)
            {
                yield return new Command_Action
                {
                    defaultLabel = "CommandAbandonHome".Translate(),
                    defaultDesc = "WTW_AbandonExplorationSite".Translate(),
                    icon = ContentFinder<Texture2D>.Get("GenericWorldSite"),
                    action = () =>
                    {
                        Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                            "WantToContinue".Translate(),
                            () => {
                                if (!visitCell.DoPawnBlockRemove())
                                {
                                    visitCell.TaskedToRemove = true;
                                }
                                else
                                {
                                    Messages.Message("WTW_AbandonImpossibleBecauseOfPawns".Translate(), MessageTypeDefOf.NegativeEvent);
                                }

                            },
                            true
                        ));
                    }
                };
            }
        }
    }


}
