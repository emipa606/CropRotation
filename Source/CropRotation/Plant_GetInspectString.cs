using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation;

[HarmonyPatch(typeof(Plant), nameof(Plant.GetInspectString))]
public static class Plant_GetInspectString
{
    public static void Postfix(ref string __result, Plant __instance)
    {
        var mapComponent = __instance.Map.GetComponent<CropHistoryMapComponent>();

        if (mapComponent == null)
        {
            return;
        }

        var yieldModifier = mapComponent.GetYieldModifier(__instance);
        var modifiedText = new StringBuilder(__result);
        modifiedText.AppendLine();
        switch (yieldModifier)
        {
            case 1f:
                return;
            case < 1f:
                modifiedText.AppendLine(
                    "CropRotation.ModifiedYieldLowInfo".Translate(yieldModifier.ToStringPercent()));
                break;
            default:
                modifiedText.AppendLine(
                    "CropRotation.ModifiedYieldHighInfo".Translate(yieldModifier.ToStringPercent()));
                break;
        }

        __result = modifiedText.ToString().TrimEndNewlines();
    }
}