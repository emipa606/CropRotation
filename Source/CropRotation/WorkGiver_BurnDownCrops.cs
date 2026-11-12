using System.Collections.Generic;
using System.Linq;
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
            if (zoneGrowing == null || zoneGrowing.Map != pawn.Map)
            {
                continue;
            }

            var cellsInOrder = zoneGrowing.Cells.OrderBy(vec3 => vec3.x + vec3.z).ToArray();
            if (pawn.Map.reservationManager.CanReserve(pawn, cellsInOrder[0]))
            {
                yield return cellsInOrder[0];
            }

            if (pawn.Map.reservationManager.CanReserve(pawn, cellsInOrder[^1]))
            {
                yield return cellsInOrder[^1];
            }
        }
    }


    public override Job JobOnCell(Pawn pawn, IntVec3 cell, bool forced = false)
    {
        if (cell.IsForbidden(pawn))
        {
            return null;
        }

        var job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamedSilentFail("BurnDownCrops"), cell);
        return job;
    }
}