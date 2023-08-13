using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation;

[HarmonyPatch(typeof(Plant), nameof(Plant.PlantCollected))]
public static class Plant_PlantCollected
{
    public static void Prefix(Plant __instance)
    {
        if (!CropRotation.IsValidCrop(__instance.def))
        {
            return;
        }

        var plantPlace = __instance.Position;

        if (plantPlace.GetZone(__instance.Map) is not Zone_Growing)
        {
            return;
        }

        var cropHistoryComponent = __instance.Map.GetComponent<CropHistoryMapComponent>();
        if (cropHistoryComponent == null)
        {
            CropRotation.LogMessage($"Failed to find the mapcomponent for {__instance.Map}", warning: true);
            return;
        }

        cropHistoryComponent.SaveCropToHistory(plantPlace, __instance);
    }
}