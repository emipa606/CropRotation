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
}