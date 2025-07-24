using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation;

[HarmonyPatch(typeof(Zone_Growing), nameof(Zone_Growing.SetPlantDefToGrow))]
public static class Zone_Growing_SetPlantDefToGrow
{
    public static void Postfix(ThingDef plantDef, Zone_Growing __instance)
    {
        __instance?.Map?.GetComponent<CropHistoryMapComponent>()?.RemoveExtraPlant(__instance, plantDef);
    }
}