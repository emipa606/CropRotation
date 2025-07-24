using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation;

[HarmonyPatch(typeof(Zone), nameof(Zone.Delete), typeof(bool))]
public static class Zone_Delete
{
    public static void Prefix(Zone_Growing __instance)
    {
        __instance?.Map?.GetComponent<CropHistoryMapComponent>()?.RemoveExtraCropZone(__instance);
    }
}