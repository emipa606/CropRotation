using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation;

[HarmonyPatch(typeof(Fire), nameof(Fire.TrySpread))]
public static class Fire_TrySpread
{
    public static bool Prefix(Fire __instance)
    {
        if (__instance.Position.GetZone(__instance.Map) is not Zone_Growing zone)
        {
            return true;
        }

        var mapComponent = __instance.Map.GetComponent<CropHistoryMapComponent>();

        if (mapComponent == null)
        {
            return true;
        }

        if (!mapComponent.GetZonesToBurn().Contains(zone))
        {
            return true;
        }

        var newCell = __instance.Position + GenRadial.ManualRadialPattern[Rand.RangeInclusive(1, 8)];
        if (!zone.ContainsCell(newCell))
        {
            return false;
        }

        var startRect = CellRect.SingleCell(__instance.Position);
        var endRect = CellRect.SingleCell(newCell);
        if (!GenSight.LineOfSight(__instance.Position, newCell, __instance.Map, startRect, endRect))
        {
            return false;
        }

        ((Spark)GenSpawn.Spawn(ThingDefOf.Spark, __instance.Position, __instance.Map)).Launch(__instance, newCell,
            newCell, ProjectileHitFlags.All);

        return false;
    }
}