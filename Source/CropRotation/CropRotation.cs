using System.Reflection;
using HarmonyLib;
using Verse;

namespace CropRotation;

[StaticConstructorOnStartup]
public static class CropRotation
{
    public const int MaxSavedCrops = 25;

    static CropRotation()
    {
        new Harmony("Mlie.CropRotation").PatchAll(Assembly.GetExecutingAssembly());
    }

    public static bool IsValidCrop(ThingDef thingDef)
    {
        if (thingDef?.plant == null)
        {
            return false;
        }

        if (thingDef.plant.choppedThingDef != null || thingDef.plant.burnedThingDef != null ||
            thingDef.plant.smashedThingDef != null)
        {
            return false;
        }

        if (!thingDef.plant.HarvestDestroys)
        {
            return false;
        }

        if (thingDef.plant.isStump)
        {
            return false;
        }

        return thingDef.plant.harvestedThingDef != null;
    }
}