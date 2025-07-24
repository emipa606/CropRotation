using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation;

[StaticConstructorOnStartup]
public static class CropRotation
{
    public const int MaxSavedCrops = 25;

    static CropRotation()
    {
        new Harmony("Mlie.CropRotation").PatchAll(Assembly.GetExecutingAssembly());
        UpdateResearchSetting();
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

    public static void LogMessage(string message, bool force = false, bool warning = false)
    {
        if (warning)
        {
            Log.WarningOnce($"[CropRotation]: {message}", message.GetHashCode());
            return;
        }

        if (!force && !CropRotationMod.Instance.Settings.VerboseLogging)
        {
            return;
        }

        Log.Message($"[CropRotation]: {message}");
    }

    public static void UpdateResearchSetting()
    {
        var basicResearch = DefDatabase<ResearchProjectDef>.GetNamedSilentFail("BasicCropRotation");
        var advancedResearch =
            DefDatabase<ResearchProjectDef>.GetNamedSilentFail("AdvancedCropRotation");
        if (CropRotationMod.Instance.Settings.RequireResearch)
        {
            if (basicResearch == null)
            {
                basicResearch = new ResearchProjectDef
                {
                    defName = "BasicCropRotation",
                    label = "basic crop rotation",
                    generated = true,
                    description =
                        "The idea of rotating crops to keep the yield of the land from lowering.\r\n\r\nAllows rotating between two types of crops.",
                    baseCost = 300,
                    techLevel = TechLevel.Neolithic,
                    tags = [DefDatabase<ResearchProjectTagDef>.GetNamedSilentFail("TribalStart")],
                    researchViewX = 0f,
                    researchViewY = 5.4f
                };
                DefGenerator.AddImpliedDef(basicResearch);
                basicResearch.ResolveReferences();
                ResearchProjectDef.GenerateNonOverlappingCoordinates();
                LogMessage("Adding basic crop rotation research");
            }

            if (advancedResearch != null)
            {
                return;
            }

            advancedResearch = new ResearchProjectDef
            {
                defName = "AdvancedCropRotation",
                label = "advanced crop rotation",
                generated = true,
                description =
                    "Rotating between multiple crops to increase the yield of the land.\r\n\r\nAllows rotating between any amount of crops.",
                baseCost = 700,
                techLevel = TechLevel.Industrial,
                tags = [ResearchProjectTagDefOf.ClassicStart],
                researchViewX = 4f,
                researchViewY = 5.4f,
                prerequisites = [ResearchProjectDef.Named("BasicCropRotation")]
            };
            DefGenerator.AddImpliedDef(advancedResearch);
            advancedResearch.ResolveReferences();
            ResearchProjectDef.GenerateNonOverlappingCoordinates();
            LogMessage("Adding advanced crop rotation research");

            return;
        }

        if (basicResearch != null)
        {
            GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), typeof(ResearchProjectDef), "Remove",
                basicResearch);
            LogMessage("Removing basic crop rotation research");
        }

        if (advancedResearch == null)
        {
            return;
        }

        GenGeneric.InvokeStaticMethodOnGenericType(typeof(DefDatabase<>), typeof(ResearchProjectDef), "Remove",
            advancedResearch);
        LogMessage("Removing advanced crop rotation research");
    }
}