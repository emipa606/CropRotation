using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace CropRotation;

[StaticConstructorOnStartup]
public class Command_SetExtraSesonalPlantToGrow : Command_SetExtraPlantToGrow
{
    private static readonly List<ThingDef> tmpAvailablePlants = new List<ThingDef>();

    private static readonly Texture2D NoPlantSelected = ContentFinder<Texture2D>.Get("Commands/ExtraPlantToGrow");

    private readonly CropHistoryMapComponent component;

    private readonly Season season;

    private readonly Zone_Growing zone;

    private List<IPlantToGrowSettable> settables;

    public Command_SetExtraSesonalPlantToGrow(ThingDef currentCrop, Season currentSeason) : base(currentCrop)
    {
        if (Find.Selector.SelectedZone is not Zone_Growing selectedZone)
        {
            return;
        }

        zone = selectedZone;
        season = currentSeason;

        component = Find.CurrentMap?.GetComponent<CropHistoryMapComponent>();
        if (component == null)
        {
            return;
        }

        icon = currentCrop.uiIcon;
        iconAngle = currentCrop.uiIconAngle;
        iconOffset = currentCrop.uiIconOffset;
        defaultLabel = season.Label();
        defaultDesc = "CropRotation.ExtraPlantForSeasonDesc".Translate(season.Label());
    }

    public override void ProcessInput(Event ev)
    {
        base.ProcessInput(ev);
        var list = new List<FloatMenuOption>();

        if (settables == null)
        {
            settables = new List<IPlantToGrowSettable>();
        }

        if (!settables.Contains(settable))
        {
            settables.Add(settable);
        }

        tmpAvailablePlants.Clear();
        foreach (var thingDef in PlantUtility.ValidPlantTypesForGrowers(settables))
        {
            if (!IsPlantAvailable(thingDef, settable.Map))
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
                    component.SetPlantForSeason(zone, season, plantDef);
                    if (def.plant.interferesWithRoof)
                    {
                        using var enumerator2 = settable.Cells.GetEnumerator();
                        while (enumerator2.MoveNext())
                        {
                            if (!enumerator2.Current.Roofed(settable.Map))
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

        Find.WindowStack.Add(new FloatMenu(list));
    }

    public override bool InheritInteractionsFrom(Gizmo other)
    {
        if (settables == null)
        {
            settables = new List<IPlantToGrowSettable>();
        }

        settables.Add(((Command_SetExtraSesonalPlantToGrow)other).settable);
        return false;
    }
}