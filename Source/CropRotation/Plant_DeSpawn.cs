using HarmonyLib;
using RimWorld;

namespace CropRotation;

[HarmonyPatch(typeof(Plant), nameof(Plant.DeSpawn))]
public static class Plant_DeSpawn
{
    public static void Prefix(Plant __instance)
    {
        if (!__instance.Spawned)
        {
            return;
        }

        var cropHistoryComponent = __instance.Map?.GetComponent<CropHistoryMapComponent>();
        if (cropHistoryComponent == null)
        {
            CropRotation.LogMessage($"Failed to find the mapcomponent for {__instance.Map}", warning: true);
            return;
        }

        CropRotation.LogMessage($"Saving empty cell at {__instance.Position}");
        cropHistoryComponent.SaveEmptyTimestamp(__instance.Position);
    }
}