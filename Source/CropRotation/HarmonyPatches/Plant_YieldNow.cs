using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation;

[HarmonyPatch(typeof(Plant), nameof(Plant.YieldNow))]
public static class Plant_YieldNow
{
    public static void Postfix(Plant __instance, ref int __result)
    {
        var mapComponent = __instance?.Map?.GetComponent<CropHistoryMapComponent>();

        if (mapComponent == null)
        {
            return;
        }

        __result = GenMath.RoundRandom(__result * mapComponent.GetYieldModifier(__instance));
    }
}