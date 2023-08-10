using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation;

[HarmonyPatch(typeof(Plant), nameof(Plant.PlantCollected))]
public static class Plant_PlantCollected
{
    public static void Prefix(Plant __instance, PlantDestructionMode plantDestructionMode)
    {
        if (!__instance.def.plant.HarvestDestroys)
        {
            Log.Message($"{__instance} reharvestable");
            return;
        }

        if (__instance.def.plant.IsTree)
        {
            Log.Message($"{__instance} is tree");
            return;
        }

        if (plantDestructionMode != PlantDestructionMode.Chop)
        {
            Log.Message($"{__instance} is not chopped {plantDestructionMode}");
            return;
        }

        var plantPlace = __instance.Position;

        if (plantPlace.GetZone(__instance.Map) is not Zone_Growing)
        {
            Log.Message($"{__instance} is not in zone");
            return;
        }

        var cropHistoryComponent = __instance.Map.GetComponent<CropHistoryMapComponent>();
        if (cropHistoryComponent == null)
        {
            Log.Warning("[CropRotation]: Failed to find the mapcomponent, should not happen.");
            return;
        }

        cropHistoryComponent.SaveCrop(plantPlace, __instance);
    }
}