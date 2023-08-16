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
            if (done || gizmo is not Command_SetPlantToGrow commandSetPlantToGrow ||
                CropRotationMod.instance.Settings.RequireResearch &&
                !ResearchProjectDef.Named("BasicCropRotation").IsFinished)
            {
                yield return gizmo;
                continue;
            }

            done = true;
            var component = __instance?.Map?.GetComponent<CropHistoryMapComponent>();
            if (component == null)
            {
                yield return commandSetPlantToGrow;
                continue;
            }

            var seasonalCrops = component.GetSeasonalCrops(__instance);
            if (seasonalCrops?.Any() == true)
            {
                commandSetPlantToGrow.defaultLabel = Season.Spring.Label();
            }

            yield return commandSetPlantToGrow;

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

            var switchAction = new Command_Action
            {
                icon = ContentFinder<Texture2D>.Get("Commands/SwitchExtraType"),
                defaultLabel = "CropRotation.SwitchExtraCropType".Translate(),
                defaultDesc = "CropRotation.SwitchExtraCropTypeTT".Translate(),
                action = delegate { component.SaveSeasonalZone(__instance); }
            };

            if (!CropRotation.IsValidCrop(__instance.plantDefToGrow))
            {
                component.RemoveExtraCropZone(__instance);
                component.RemoveSeasonalZone(__instance);
                if (__instance.AllContainedThings.Any(thing => thing.def.IsPlant))
                {
                    yield return burnAction;
                }

                continue;
            }

            if (seasonalCrops?.Any() == true && seasonalCrops.Count == 3)
            {
                switchAction.action = delegate { component.RemoveSeasonalZone(__instance); };
                yield return new Command_SetExtraSesonalPlantToGrow(seasonalCrops[0], Season.Summer)
                {
                    settable = __instance
                };
                yield return new Command_SetExtraSesonalPlantToGrow(seasonalCrops[1], Season.Fall)
                {
                    settable = __instance
                };
                yield return new Command_SetExtraSesonalPlantToGrow(seasonalCrops[2], Season.Winter)
                {
                    settable = __instance
                };
                yield return switchAction;
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

            var season = GenLocalDate.Season(__instance.Map);
            if (season is not Season.PermanentSummer and not Season.PermanentWinter)
            {
                yield return switchAction;
            }

            if (__instance.AllContainedThings.Any(thing => thing.def.IsPlant))
            {
                yield return burnAction;
            }
        }
    }
}