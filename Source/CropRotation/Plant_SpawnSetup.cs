using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation;

[HarmonyPatch(typeof(Plant), nameof(Plant.SpawnSetup))]
public static class Plant_SpawnSetup
{
    public static void Postfix(Plant __instance, Map map, bool respawningAfterLoad)
    {
        if (!__instance.Spawned)
        {
            return;
        }

        if (respawningAfterLoad)
        {
            return;
        }

        var cropHistoryComponent = map.GetComponent<CropHistoryMapComponent>();
        if (cropHistoryComponent == null)
        {
            CropRotation.LogMessage($"Failed to find the mapcomponent for {__instance.Map}", warning: true);
            return;
        }

        cropHistoryComponent.SaveNotEmptyTimestamp(__instance.Position);
    }
}