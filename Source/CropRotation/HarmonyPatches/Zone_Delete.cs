using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation;

[HarmonyPatch(typeof(Zone), nameof(Zone.Delete), typeof(bool))]
public static class Zone_Delete
{
    public static void Prefix(Zone_Growing __instance)
    {
        var mapComponent = __instance?.Map?.GetComponent<CropHistoryMapComponent>();
        if (mapComponent == null)
        {
            return;
        }

        mapComponent.RemoveExtraCropZone(__instance);
        mapComponent.RemoveZoneToBurn(__instance);
        mapComponent.RemoveSeasonalZone(__instance);
    }
}