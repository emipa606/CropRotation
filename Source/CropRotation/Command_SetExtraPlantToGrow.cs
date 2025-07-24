using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CropRotation;

[StaticConstructorOnStartup]
public class Command_SetExtraPlantToGrow : Command
{
    private static readonly List<ThingDef> tmpAvailablePlants = [];

    private static readonly Texture2D noPlantSelected = ContentFinder<Texture2D>.Get("Commands/ExtraPlantToGrow");

    private readonly CropHistoryMapComponent component;

    private readonly ThingDef extraCrop;

    private readonly Zone_Growing zone;

    public IPlantToGrowSettable Settable;

    private List<IPlantToGrowSettable> settables;

    public Command_SetExtraPlantToGrow(ThingDef currentCrop)
    {
        if (Find.Selector.SelectedZone is not Zone_Growing selectedZone)
        {
            return;
        }

        zone = selectedZone;

        component = Find.CurrentMap?.GetComponent<CropHistoryMapComponent>();
        if (component == null)
        {
            return;
        }

        extraCrop = currentCrop;
        if (extraCrop == null)
        {
            icon = noPlantSelected;
            defaultLabel = "CropRotation.NoExtraPlantSelected".Translate();
            defaultDesc = "CropRotation.ExtraPlantDesc".Translate();
            return;
        }

        icon = extraCrop.uiIcon;
        iconAngle = extraCrop.uiIconAngle;
        iconOffset = extraCrop.uiIconOffset;
        defaultLabel = "CommandSelectPlantToGrow".Translate(extraCrop.LabelCap);
        defaultDesc = "CropRotation.ExtraPlantDesc".Translate();
    }

    public override void ProcessInput(Event ev)
    {
        base.ProcessInput(ev);
        var list = new List<FloatMenuOption>();

        var currentExtraCrops = component.GetExtraCropsForZone(zone);

        settables ??= [];

        if (!settables.Contains(Settable))
        {
            settables.Add(Settable);
        }

        tmpAvailablePlants.Clear();
        foreach (var thingDef in PlantUtility.ValidPlantTypesForGrowers(settables))
        {
            if (!IsPlantAvailable(thingDef, Settable.Map))
            {
                continue;
            }

            if (thingDef == zone.PlantDefToGrow)
            {
                continue;
            }

            if (currentExtraCrops.Contains(thingDef) && thingDef != extraCrop)
            {
                continue;
            }

            tmpAvailablePlants.Add(thingDef);
        }

        tmpAvailablePlants.SortBy(x => -GetPlantListPriority(x), x => x.label);
        foreach (var plantDef in tmpAvailablePlants)
        {
            string text = plantDef.LabelCap;
            if (plantDef.plant.sowMinSkill > 0)
            {
                text = string.Concat(text, " (" + "MinSkill".Translate() + ": ", plantDef.plant.sowMinSkill, ")");
            }

            var def = plantDef;
            list.Add(new FloatMenuOption(text, delegate
                {
                    component.SaveExtraCrop(zone, plantDef, extraCrop);
                    if (def.plant.interferesWithRoof)
                    {
                        using var enumerator2 = Settable.Cells.GetEnumerator();
                        while (enumerator2.MoveNext())
                        {
                            if (!enumerator2.Current.Roofed(Settable.Map))
                            {
                                continue;
                            }

                            Messages.Message(
                                "MessagePlantIncompatibleWithRoof".Translate(
                                    Find.ActiveLanguageWorker.Pluralize(def.LabelCap)),
                                MessageTypeDefOf.CautionInput, false);
                            break;
                        }
                    }

                    CheckIfOk(def, true, zone);
                }, plantDef, null, false, MenuOptionPriority.Default, null, null, 29f,
                rect => Widgets.InfoCardButton(rect.x + 5f, rect.y + ((rect.height - 24f) / 2f), plantDef)));
        }

        list.Add(new FloatMenuOption("None".Translate(),
            delegate { component.RemoveExtraPlant(zone, extraCrop); }));
        Find.WindowStack.Add(new FloatMenu(list));
    }

    public override bool InheritInteractionsFrom(Gizmo other)
    {
        settables ??= [];

        settables.Add(((Command_SetExtraPlantToGrow)other).Settable);
        return false;
    }

    public static bool CheckIfOk(ThingDef plantDef, bool notify, Zone_Growing growingZone)
    {
        if (plantDef.plant.sowMinSkill > 0)
        {
            foreach (var pawn in growingZone.Map.mapPawns.FreeColonistsSpawned)
            {
                if (pawn.skills.GetSkill(SkillDefOf.Plants).Level >= plantDef.plant.sowMinSkill && !pawn.Downed &&
                    pawn.workSettings.WorkIsActive(WorkTypeDefOf.Growing))
                {
                    return true;
                }
            }

            if (ModsConfig.BiotechActive &&
                MechanitorUtility.AnyPlayerMechCanDoWork(WorkTypeDefOf.Growing, plantDef.plant.sowMinSkill, out _))
            {
                return true;
            }

            if (notify)
            {
                Find.WindowStack.Add(new Dialog_MessageBox("NoGrowerCanPlant"
                    .Translate(plantDef.label, plantDef.plant.sowMinSkill).CapitalizeFirst()));
            }

            return false;
        }

        if (!plantDef.plant.cavePlant)
        {
            return true;
        }

        var cell = IntVec3.Invalid;
        foreach (var intVec in growingZone.Cells)
        {
            if (intVec.Roofed(growingZone.Map) &&
                !(growingZone.Map.glowGrid.GroundGlowAt(intVec, true) > 0f))
            {
                continue;
            }

            cell = intVec;
            break;
        }

        if (!cell.IsValid)
        {
            return true;
        }

        if (notify)
        {
            Messages.Message("MessageWarningCavePlantsExposedToLight".Translate(plantDef.LabelCap),
                new TargetInfo(cell, growingZone.Map), MessageTypeDefOf.RejectInput);
        }

        return false;
    }

    protected static bool IsPlantAvailable(ThingDef plantDef, Map map)
    {
        if (!CropRotation.IsValidCrop(plantDef))
        {
            return false;
        }

        var sowResearchPrerequisites = plantDef.plant.sowResearchPrerequisites;
        if (sowResearchPrerequisites == null)
        {
            return true;
        }

        foreach (var researchProjectDef in sowResearchPrerequisites)
        {
            if (!researchProjectDef.IsFinished)
            {
                return false;
            }
        }

        return !plantDef.plant.mustBeWildToSow || map.Biome.AllWildPlants.Contains(plantDef);
    }

    protected static float GetPlantListPriority(ThingDef plantDef)
    {
        if (plantDef.plant.IsTree)
        {
            return 1f;
        }

        switch (plantDef.plant.purpose)
        {
            case PlantPurpose.Food:
                return 4f;
            case PlantPurpose.Health:
                return 3f;
            case PlantPurpose.Beauty:
                return 2f;
            default:
                return 0f;
        }
    }
}