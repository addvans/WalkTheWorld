﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace WalkTheWorld
{
    public class WalkTheWorldMod : Mod
    {
        public static WalkTheWorldModSettings Settings;

        public WalkTheWorldMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<WalkTheWorldModSettings>();
        }

        public override string SettingsCategory() => "Walk the World"; // Название в меню настроек
        public int page = 0;
        public override void DoSettingsWindowContents(Rect inRect)
        {
            string[] leavingTypesNames = { "WTW_Settings_EveryoneLeavingTheMap".Translate(), "WTW_Settings_SelecetedLeavingTheMap".Translate(), "WTW_Settings_AskingToLeavingTheMap".Translate() };
            string[] cameraFocusModes = { "WTW_Settings_CameraFocusesPawns".Translate(), "WTW_Settings_CameraFocusesMapCenter".Translate(), "WTW_Settings_CameraFocusesNothing".Translate() };
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            float buttonHeight = 30f;
            float spacing = 8f;
            float buttonWidth = 100f;

            Rect row = listing.GetRect(buttonHeight);
            float x = row.x;

            if (Widgets.ButtonText(new Rect(x, row.y, buttonWidth, buttonHeight), "WTW_Settings_MapButtonName".Translate()))
                page = 0;
            x += buttonWidth + spacing;

            if (Widgets.ButtonText(new Rect(x, row.y, buttonWidth, buttonHeight), "WTW_Settings_MutatorsButtonName".Translate()))
                page = 1;
            x += buttonWidth + spacing;
            if (page == 0)
            {
                listing.Label($"{"WTW_Settings_MapSizeLabel".Translate()}: {Settings.mapSize:F1}");
                Settings.mapSize = (int)listing.Slider(Settings.mapSize, 30, 200);
                if (Settings.eventChance > 0)
                    listing.Label($"{"WTW_Settings_EventChanceLabel".Translate()}: {Settings.eventChance}%");
                else
                    listing.Label("WTW_Settings_EventChanceLabel".Translate() + ": " + "WTW_Settings_EventChanceDisabledLabel".Translate());
                Settings.eventChance = (int)listing.Slider(Settings.eventChance, 0, 100);
                if (Settings.mapCountForEvent > 0)
                    listing.Label("WTW_Settings_EventChancePerMapLabel".Translate(Settings.mapCountForEvent.ToString("N0")));
                else
                    listing.Label("WTW_Settings_EventChancePerMapDisabledLabel".Translate());
                Settings.mapCountForEvent = (int)listing.Slider(Settings.mapCountForEvent, 0, 40);
                listing.CheckboxLabeled("WTW_Settings_LeavingConfirmationMenuLabel".Translate(), ref Settings.showConfirmationPreviewMenu);
                listing.Label("WTW_Settings_WhoLeavingTheMapLabel".Translate());
                if (listing.ButtonText(leavingTypesNames[(int)Settings.leavingType], widthPct: 0.3f))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    for (int i = 0; i < leavingTypesNames.Length; i++)
                    {
                        int index = i; // Локальная копия для замыкания
                        options.Add(new FloatMenuOption(
                            leavingTypesNames[index],
                            () => Settings.leavingType = (LeavingType)index
                        ));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                listing.Gap();
                listing.Label("WTW_Settings_CameraFocusLabel".Translate());
                if (listing.ButtonText(cameraFocusModes[(int)Settings.camFocus], widthPct: 0.3f))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    for (int i = 0; i < cameraFocusModes.Length; i++)
                    {
                        int index = i; // Локальная копия для замыкания
                        options.Add(new FloatMenuOption(
                            cameraFocusModes[index],
                            () => Settings.camFocus = (CameraFocusMode)index
                        ));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                listing.Gap();
                listing.Label("WTW_Settings_FilteredEventsQuestionLabel".Translate());
                if (listing.ButtonText(Settings.eventsFilter.ToString(), widthPct: 0.3f))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (RandomEventsFilterType filter in Enum.GetValues(typeof(RandomEventsFilterType)))
                    {
                        options.Add(new FloatMenuOption(filter.ToString(), () => Settings.eventsFilter = filter));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                string txt = "WTW_Settings_UnfilteredEventsLabel".Translate();
                if (Settings.eventsFilter == RandomEventsFilterType.Filtered)
                    txt = "WTW_Settings_FilteredEventsLabel".Translate();
                listing.Label(txt);
                listing.Gap();
                if (listing.ButtonText("WTW_Settings_DefaultsButton".Translate(), widthPct: 0.15f))
                {
                    Settings.mapSize = 60;
                    Settings.eventChance = 15;
                    Settings.mapCountForEvent = 5;
                    Settings.leavingType = LeavingType.Selected;
                    Settings.eventsFilter = RandomEventsFilterType.Filtered;

                }

            }
            if (page == 1)
            {
                string txt = "WTW_Settings_MapMutatorsDescription".Translate();
                listing.Label(txt);
                listing.Gap();
                if (listing.ButtonText("WTW_Settings_MapMutatorsClearButton".Translate(), widthPct: 0.15f))
                {
                    Settings.mutatorsToDelete = new List<string>();
                }
                if (listing.ButtonText("WTW_Settings_MapMutatorsDefaultButton".Translate(), widthPct: 0.15f))
                {
                    Settings.mutatorsToDelete = DefDatabase<TileMutatorDef>.AllDefs
                        .Where(m => m.defName != null && (
                            m.defName.Contains("Ancient") ||
                            m.defName.Contains("Abandoned") ||
                            m.defName.Contains("Stockpile") ||
                            m.defName.Contains("Ruins")))
                        .Select(m => m.defName) // Сохраняем только имена
                        .ToList();
                }
                if (listing.ButtonText("WTW_Settings_MapMutatorsAddButton".Translate(), widthPct: 0.15f))
                {
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (TileMutatorDef mutator in DefDatabase<TileMutatorDef>.AllDefs.ToList())
                    {
                        options.Add(new FloatMenuOption(mutator.ToString(), () =>
                        {
                            if (!Settings.mutatorsToDelete.Contains(mutator.defName))
                            {
                                Settings.mutatorsToDelete.Add(mutator.defName);
                            }
                        }));
                    }
                    Find.WindowStack.Add(new FloatMenu(options));
                }
                txt = "WTW_Settings_MapMutatorsRemoveDescription".Translate();
                listing.Label(txt);
                if (listing.ButtonText($"{"WTW_Settings_MapMutatorsRemoveButton".Translate()}: {Settings.mutatorsToDelete.Count}", widthPct: 0.15f))
                {
                    if (Settings.mutatorsToDelete.Count > 0)
                    {
                        List<FloatMenuOption> options = new List<FloatMenuOption>();
                        for (int mutindex = 0; mutindex < Settings.mutatorsToDelete.Count; mutindex++)
                        {
                            int index = mutindex;
                            var mutator = Settings.mutatorsToDelete[mutindex];
                            options.Add(new FloatMenuOption(mutator, () => Settings.mutatorsToDelete.RemoveAt(index)));
                        }
                        Find.WindowStack.Add(new FloatMenu(options));
                    }
                }
            }
            listing.End();

        }
    }

}
