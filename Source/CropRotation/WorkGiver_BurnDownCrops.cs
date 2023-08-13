using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace CropRotation;

public class WorkGiver_BurnDownCrops : WorkGiver_Scanner
{
    public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

    public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
    {
        var cropHistoryComponent = pawn.Map?.GetComponent<CropHistoryMapComponent>();
        if (cropHistoryComponent == null)
        {
            CropRotation.LogMessage($"Failed to find the mapcomponent for {pawn.Map}", warning: true);
            yield break;
        }

        var zoneToBurn = cropHistoryComponent.GetZonesToBurn();
        if (zoneToBurn == null)
        {
            yield break;
        }

        foreach (var zoneGrowing in zoneToBurn)
        {
            if (pawn.Map.reservationManager.CanReserve(pawn, zoneGrowing.Position))
            {
                yield return zoneGrowing.Position;
            }
        }
    }


    public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
    {
        if (c.IsForbidden(pawn))
        {
            return null;
        }

        var job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamedSilentFail("BurnDownCrops"), c);
        return job;
    }
}