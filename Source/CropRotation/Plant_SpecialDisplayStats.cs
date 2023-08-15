using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation;

[HarmonyPatch(typeof(Plant), nameof(Plant.SpecialDisplayStats))]
public static class Plant_SpecialDisplayStats
{
    public static IEnumerable<StatDrawEntry> Postfix(IEnumerable<StatDrawEntry> values, Plant __instance)
    {
        foreach (var statDrawEntry in values)
        {
            yield return statDrawEntry;
        }

        var mapComponent = __instance?.Map?.GetComponent<CropHistoryMapComponent>();

        if (mapComponent == null)
        {
            yield break;
        }

        var yieldModifier = mapComponent.GetYieldModifier(__instance);
        switch (yieldModifier)
        {
            case 1f:
                yield break;
            case < 1f:
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "CropRotation.ModifiedYieldLow".Translate(),
                    yieldModifier.ToStringPercent(), "CropRotation.ModifiedYieldLowTT".Translate(), 4157);
                break;
            default:
                yield return new StatDrawEntry(StatCategoryDefOf.Basics, "CropRotation.ModifiedYieldHigh".Translate(),
                    yieldModifier.ToStringPercent(), "CropRotation.ModifiedYieldHighTT".Translate(), 4157);
                break;
        }
    }
}