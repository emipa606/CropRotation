using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation;

[HarmonyPatch(typeof(WorkGiver_GrowerSow), nameof(WorkGiver_GrowerSow.JobOnCell))]
public static class WorkGiver_GrowerSow_JobOnCell
{
    public static void Prefix(IntVec3 c, Pawn pawn)
    {
        if (pawn.Map.zoneManager.ZoneAt(c) is not Zone_Growing zone)
        {
            return;
        }

        var mapComponent = pawn.Map.GetComponent<CropHistoryMapComponent>();

        if (mapComponent == null)
        {
            return;
        }

        WorkGiver_Grower.wantedPlantDef = mapComponent.GetNextCrop(zone, c, zone.plantDefToGrow);
    }
}