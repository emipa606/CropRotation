using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation;

[HarmonyPatch(typeof(Plant), nameof(Plant.PlantCollected))]
public static class Plant_PlantCollected
{
    public static void Prefix(Plant __instance, PlantDestructionMode plantDestructionMode)
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
            Log.Warning("[CropRotation]: Failed to find the mapcomponent, should not happen.");
            return;
        }

        cropHistoryComponent.SaveCropToHistory(plantPlace, __instance);
    }
}