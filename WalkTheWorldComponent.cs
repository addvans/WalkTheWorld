using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld.Planet;
using WalkTheWorld;
using System.Reflection;

namespace WalkTheWorld
{
    public class WalkTheWorld : GameComponent
    {
        public static WalkTheWorld Instance;
        public int TicksCooldown = 64;
        public int lastEnterTick = 0;
        public IntVec3 lastEnterPos = IntVec3.Zero;

        public WalkTheWorld(Game game)
        {
            Instance = this;
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            Instance = this;

        }


        public void FinalizeTravel(Pawn pawn, Map targetMap)
        {
            var pawns = Find.CurrentMap.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer).ToList();
            if (WalkTheWorldMod.Settings?.leavingType == LeavingType.Selected)
            {
                pawns = Find.Selector.SelectedPawns
                                                .Where(p => p.IsColonistPlayerControlled || p.Drafted)
                                                .ToList();
            }
            else if (WalkTheWorldMod.Settings?.leavingType == LeavingType.AlwaysAsk)
            {//НАДОПЕРЕВЕСТИ!!
                Find.WindowStack.Add(new Dialog_MessageBox($"Who should visit next tile?",
                               "All my Pawns", () =>
                               {
                               }, "Selected only", () =>
                               {
                                   pawns = Find.Selector.SelectedPawns
                                                .Where(p => p.IsColonistPlayerControlled || p.Drafted)
                                                .ToList();
                               }));
            }

            var oldPos = pawn.Position;
            var camPos = IntVec3.Zero;
            var oldSize = pawn.Map.Size;
            var caravan = CaravanExitMapUtility.ExitMapAndCreateCaravan(pawns, Faction.OfPlayer, Find.CurrentMap.Tile, Direction8Way.North, targetMap.Tile, sendMessage: false);

            CaravanEnterMapUtility.Enter(caravan, targetMap, CaravanEnterMode.Edge,
                extraCellValidator: WalkTheWorld_WorldTileUtility.GetEntryPredicate(targetMap, oldPos, oldSize, out camPos),
                draftColonists: true);

            Current.Game.CurrentMap = targetMap;
            var c = Find.CameraDriver.MapPosition;
            var b = Find.CameraDriver.RootSize;
            Find.Selector.Select(pawn);
            if (WalkTheWorldMod.Settings.camFocus == CameraFocusMode.OnEnteredPawns)
                Find.CameraDriver.JumpToCurrentMapLoc(camPos);
            else if (WalkTheWorldMod.Settings.camFocus == CameraFocusMode.Centered)
            {
                Find.CameraDriver.JumpToCurrentMapLoc(targetMap.Center);
                Find.CameraDriver.SetRootSize(b);
            }
            else
            {
                Find.CameraDriver.JumpToCurrentMapLoc(c);
                Find.CameraDriver.SetRootSize(b);
            }
            lastEnterPos = pawn.Position;
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame - lastEnterTick < TicksCooldown)
                return;
            if (WorldRendererUtility.DrawingMap)
            {
                if (Find.Selector.SelectedPawns.Count > 0)
                {
                    lastEnterTick = Find.TickManager.TicksGame;
                    Pawn pawn = GetLeavingPawn();
                    if (pawn == null)
                        return;
                    PlanetTile targetTile = WalkTheWorld_WorldTileUtility.GetTileInDirection(Find.CurrentMap.Tile, WalkTheWorld_WorldTileUtility.ToDirection8Way(WalkTheWorld_WorldTileUtility.GetDirectionFromCenter(Find.CurrentMap, pawn.Position)));
                    lastEnterPos = pawn.Position;
                    if (!WalkTheWorld_WorldTileUtility.isTileWalkable(targetTile))
                        return;
                    if (targetTile == -1)
                        return;

                    if (WalkTheWorldMod.Settings.showConfirmationPreviewMenu)
                    {
                        ShowConfirmationWindow(targetTile, pawn);

                    }
                    else
                    {
                        var mapGenerator = new MapGenerator(targetTile);
                        mapGenerator.StartGeneration();
                        FinalizeTravel(pawn, mapGenerator.generatedMap);
                    }
                }


            }
        }
    
        

        Pawn GetLeavingPawn()
        {
            foreach (Pawn pawn in Find.Selector.SelectedPawns)
            {
                if (pawn.Drafted && WalkTheWorld_WorldTileUtility.IsOnEdge(pawn.Position, Find.CurrentMap) && pawn.Position != lastEnterPos)
                    return pawn;
            }
            return null;
        }
        void ShowConfirmationWindow(PlanetTile targetTile, Pawn pawn)
        {
            Find.World.renderer.wantedMode = WorldRenderMode.Planet;
            Find.WorldCameraDriver.JumpTo(targetTile);
            Find.WorldCameraDriver.ResetAltitude();
            Find.WorldSelector.SelectedTile = targetTile;
            var dialog = new Dialog_MessageBoxAdjusted($"{"LetterLabelAreaRevealed".Translate()}:\n\n{WalkTheWorld_WorldTileUtility.GetTileName(targetTile)}\n\n{"WantToContinue".Translate()}",
             "Confirm".Translate(), () => {
                 Find.World.renderer.wantedMode = WorldRenderMode.None;
                 var mapGenerator = new MapGenerator(targetTile);
                 mapGenerator.StartGeneration();
                 FinalizeTravel(pawn, mapGenerator.generatedMap);

             }, "GoBack".Translate(), () =>
             {
                 Find.World.renderer.wantedMode = WorldRenderMode.None;
                 Find.CameraDriver.JumpToCurrentMapLoc(pawn.Position);
                 lastEnterTick = Find.TickManager.TicksGame + 60;
             });
            Find.WindowStack.Add(dialog);
        }
        
        
       
        public override void GameComponentOnGUI()
        {
            base.GameComponentOnGUI();
        }
   
    }




    

}
