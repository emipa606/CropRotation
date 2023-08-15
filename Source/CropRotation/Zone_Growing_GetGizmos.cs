using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CropRotation;

[HarmonyPatch(typeof(Zone_Growing), nameof(Zone_Growing.GetGizmos))]
public static class Zone_Growing_GetGizmos
{
    public static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> values, Zone_Growing __instance)
    {
        var done = false;
        foreach (var gizmo in values)
        {
            yield return gizmo;
            if (done || gizmo is not Command_SetPlantToGrow || CropRotationMod.instance.Settings.RequireResearch &&
                !ResearchProjectDef.Named("BasicCropRotation").IsFinished)
            {
                continue;
            }

            done = true;

            var component = __instance?.Map?.GetComponent<CropHistoryMapComponent>();
            if (component == null)
            {
                continue;
            }

            var burnAction = new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("Commands/BurnCrops"),
                defaultLabel = "CropRotation.BurnCrops".Translate(),
                defaultDesc = "CropRotation.BurnCropsTT".Translate(),
                action = delegate { component.SaveZoneToBurn(__instance); }
            };

            if (component.GetZonesToBurn().Contains(__instance))
            {
                burnAction = new Command_Action
                {
                    icon = ContentFinder<Texture2D>.Get("Commands/DontBurnCrops"),
                    defaultLabel = "CropRotation.BurnCrops".Translate(),
                    defaultDesc = "CropRotation.BurnCropsTT".Translate(),
                    action = delegate { component.RemoveZoneToBurn(__instance); }
                };
            }

            if (!CropRotation.IsValidCrop(__instance.plantDefToGrow))
            {
                component.RemoveZone(__instance);
                if (__instance.AllContainedThings.Any(thing => thing.def.IsPlant))
                {
                    yield return burnAction;
                }

                continue;
            }

            var extraCrops = component.GetExtraCropsForZone(__instance);
            var showAddNew = true;
            foreach (var extraCrop in extraCrops)
            {
                yield return new Command_SetExtraPlantToGrow(extraCrop)
                {
                    settable = __instance
                };

                if (!CropRotationMod.instance.Settings.RequireResearch ||
                    ResearchProjectDef.Named("AdvancedCropRotation").IsFinished)
                {
                    continue;
                }

                showAddNew = false;
                break;
            }

            if (!showAddNew)
            {
                if (__instance.AllContainedThings.Any(thing => thing.def.IsPlant))
                {
                    yield return burnAction;
                }

                continue;
            }

            yield return new Command_SetExtraPlantToGrow(null)
            {
                settable = __instance
            };

            if (__instance.AllContainedThings.Any(thing => thing.def.IsPlant))
            {
                yield return burnAction;
            }
        }
    }
}