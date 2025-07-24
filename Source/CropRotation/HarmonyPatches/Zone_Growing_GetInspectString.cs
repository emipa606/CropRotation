using System.Text;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation;

[HarmonyPatch(typeof(Zone_Growing), nameof(Zone_Growing.GetInspectString))]
public static class Zone_Growing_GetInspectString
{
    public static void Postfix(Zone_Growing __instance, ref string __result)
    {
        if (string.IsNullOrEmpty(__result))
        {
            CropRotation.LogMessage("Null result for zone");
            return;
        }

        var cropHistoryComponent = __instance?.Map?.GetComponent<CropHistoryMapComponent>();
        if (cropHistoryComponent == null)
        {
            CropRotation.LogMessage("Failed to find the mapcomponent", warning: true);
            return;
        }

        var yieldModifier = 0f;
        foreach (var intVec3 in __instance.Cells)
        {
            yieldModifier += cropHistoryComponent.GetYieldModifier(intVec3, __instance.Map);
        }

        yieldModifier /= __instance.Cells.Count;

        var modifiedText = new StringBuilder(__result);
        modifiedText.AppendLine();
        switch (yieldModifier)
        {
            case 1f:
                CropRotation.LogMessage("Result is 1f");
                return;
            case < 1f:
                modifiedText.AppendLine(
                    "CropRotation.ZoneLowYield".Translate(yieldModifier.ToStringPercent()));
                break;
            default:
                modifiedText.AppendLine(
                    "CropRotation.ZoneHighYield".Translate(yieldModifier.ToStringPercent()));
                break;
        }

        __result = modifiedText.ToString().TrimEndNewlines();
    }
}