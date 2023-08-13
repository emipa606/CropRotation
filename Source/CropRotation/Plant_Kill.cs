using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation;

[HarmonyPatch(typeof(Plant), nameof(Plant.Kill))]
public static class Plant_Kill
{
    public static void Prefix(Plant __instance, DamageInfo? dinfo)
    {
        if (!__instance.Spawned || dinfo == null)
        {
            return;
        }

        if (dinfo.Value.Def != DamageDefOf.Flame)
        {
            return;
        }

        var cropHistoryComponent = __instance.Map?.GetComponent<CropHistoryMapComponent>();
        if (cropHistoryComponent == null)
        {
            CropRotation.LogMessage($"Failed to find the mapcomponent for {__instance.Map}", warning: true);
            return;
        }

        var growthPercent = Math.Round(__instance.Growth, 3);
        var plantPlace = __instance.Position;
        CropRotation.LogMessage($"Saving burned crop at {__instance.Position} with percent {growthPercent}");
        cropHistoryComponent.SaveStringToHistory(plantPlace, $"[Fire]{growthPercent}");
    }
}