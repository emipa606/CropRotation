using HarmonyLib;
using RimWorld;

namespace CropRotation;

[HarmonyPatch(typeof(Plant), nameof(Plant.DeliberatelyCultivated))]
public static class Plant_DeliberatelyCultivated
{
    public static void Postfix(Plant __instance, ref bool __result)
    {
        if (__result)
        {
            return;
        }

        if (__instance == null)
        {
            return;
        }

        if (!CropRotation.IsValidCrop(__instance.def))
        {
            return;
        }

        var mapComponent = __instance.Map?.GetComponent<CropHistoryMapComponent>();

        if (mapComponent == null)
        {
            return;
        }

        if (__instance.Map.zoneManager.ZoneAt(__instance.Position) is not Zone_Growing zone)
        {
            return;
        }

        __result = mapComponent.GetExtraCropsForZone(zone).Contains(__instance.def);
    }
}