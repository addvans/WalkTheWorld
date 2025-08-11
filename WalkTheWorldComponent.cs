using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using RimWorld.Planet;
using System;

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
            var oldPos = pawn.Position;
            var camPos = IntVec3.Zero;
            var oldSize = pawn.Map.Size;
            Caravan caravan = LeaveMap(targetMap);
            EnterMap(targetMap, caravan, WalkTheWorld_WorldTileUtility.GetEntryPredicate(targetMap, oldPos, oldSize, out camPos));
            
        }

       public void EnterMap(Map targetMap, Caravan caravan, Predicate<IntVec3> predicate = null)
        {
            Pawn pawn = caravan.pawns[0];
            CaravanEnterMapUtility.Enter(caravan, targetMap, CaravanEnterMode.Edge,
                extraCellValidator: predicate,
                draftColonists: true);
            Current.Game.CurrentMap = targetMap;
            Find.Selector.Select(pawn);
            ResetCamera(GetNewCameraPosition(pawn, targetMap));
            lastEnterPos = pawn.Position;
        }

        IntVec3 GetNewCameraPosition(Pawn pawn, Map newMap)
        {
            Find.World.renderer.wantedMode = WorldRenderMode.None;
            if (WalkTheWorldMod.Settings.camFocus == CameraFocusMode.OnEnteredPawns)
                return pawn.Position;
            if (WalkTheWorldMod.Settings.camFocus == CameraFocusMode.Centered)
                return newMap.Center;
            if (WalkTheWorldMod.Settings.camFocus == CameraFocusMode.Ignore)
                return Find.CameraDriver.MapPosition;
            return Find.CameraDriver.MapPosition;
        }
        void ResetCamera(IntVec3 camPos)
        {
            float zoom = Find.CameraDriver.ZoomRootSize;
            Find.CameraDriver.JumpToCurrentMapLoc(camPos);
            Find.CameraDriver.SetRootSize(zoom);

        }

        Caravan LeaveMap(Map targetMap)
        {
            List<Pawn> pawns = GetLeavingPawns();
            var caravan = CaravanExitMapUtility.ExitMapAndCreateCaravan(pawns, Faction.OfPlayer, Find.CurrentMap.Tile, Direction8Way.North, targetMap.Tile, sendMessage: false);
            return caravan;
        }

        List<Pawn> GetLeavingPawns()
        {
            List<Pawn> pawns = new List<Pawn>();
            if (WalkTheWorldMod.Settings?.leavingType == LeavingType.Selected)
            {
                pawns = Find.Selector.SelectedPawns
                                                .Where(p => p.IsColonistPlayerControlled)
                                                .ToList();
            }
            else if (WalkTheWorldMod.Settings?.leavingType == LeavingType.AlwaysAsk)
            {
                pawns = ShowChoosingWindow();
            }
            return pawns;
        }
        List<Pawn> ShowChoosingWindow()
        {
            List<Pawn> result = new List<Pawn>();
            Find.WindowStack.Add(new Dialog_MessageBox($"WTW_Settings_WhoLeavingTheMapLabel".Translate(),//НАДОПЕРЕВЕСТИ!!
                              "WTW_Settings_EveryoneLeavingTheMap".Translate(), () =>
                              {
                                  result = Find.CurrentMap.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer).ToList();

                              }, "WTW_Settings_SelecetedLeavingTheMap".Translate(), () =>
                              {
                                  result = Find.Selector.SelectedPawns
                                               .Where(p => p.IsColonistPlayerControlled )
                                               .ToList();
                              }));
            return result;  
        }
        public override void GameComponentTick()
        {
            base.GameComponentTick();
            if (Find.TickManager.TicksGame - lastEnterTick < TicksCooldown || !WorldRendererUtility.DrawingMap || Find.Selector.SelectedPawns.Count <= 0)
                return;
            lastEnterTick = Find.TickManager.TicksGame;
            Pawn pawn = GetLeavingPawn();
            if (pawn == null)
                return;
            PlanetTile targetTile = WalkTheWorld_WorldTileUtility.GetTileInDirection(Find.CurrentMap.Tile, WalkTheWorld_WorldTileUtility.ToDirection8Way(WalkTheWorld_WorldTileUtility.GetDirectionFromCenter(Find.CurrentMap, pawn.Position)));
            lastEnterPos = pawn.Position;
            if (!WalkTheWorld_WorldTileUtility.isTileWalkable(targetTile) || targetTile == -1)
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
            FocusCameraOnTile(targetTile);
            var dialog = new Dialog_MessageBoxAdjusted($"{"LetterLabelAreaRevealed".Translate()}:\n\n{WalkTheWorld_WorldTileUtility.GetTileName(targetTile)}\n\n{"WantToContinue".Translate()}",
             "Confirm".Translate(), () => {
                 Find.World.renderer.wantedMode = WorldRenderMode.None;
                 var mapGenerator = new MapGenerator(targetTile);
                 mapGenerator.StartGeneration();
                 FinalizeTravel(pawn, mapGenerator.generatedMap);

             }, "GoBack".Translate(), () =>
             {
                 FocusCameraOnPawn(pawn);
                 lastEnterTick = Find.TickManager.TicksGame + TicksCooldown;
             });
            Find.WindowStack.Add(dialog);
        }
        
        void FocusCameraOnTile(PlanetTile targetTile)
        {
            Find.World.renderer.wantedMode = WorldRenderMode.Planet;
            Find.WorldCameraDriver.JumpTo(targetTile);
            Find.WorldCameraDriver.ResetAltitude();
            Find.WorldSelector.SelectedTile = targetTile;
        }

        void FocusCameraOnPawn(Pawn pawn)
        {
            Find.World.renderer.wantedMode = WorldRenderMode.None;
            Find.CameraDriver.JumpToCurrentMapLoc(pawn.Position);
        }
        
        public override void GameComponentOnGUI()
        {
            base.GameComponentOnGUI();
        }
   
    }
}
