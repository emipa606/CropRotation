using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace CropRotation;

public class JobDriver_BurnDownCrops : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed);
    }

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        var watchCell = job.GetTarget(TargetIndex.A).Cell;
        var zone = watchCell.GetZone(Map);
        foreach (var cell in GenAdj.AdjacentCells8WayRandomized())
        {
            if (zone.ContainsCell(watchCell + cell))
            {
                continue;
            }

            watchCell += cell;
            break;
        }

        yield return Toils_Goto.GotoCell(watchCell, PathEndMode.OnCell);
        var wait = ToilMaker.MakeToil();
        wait.initAction = delegate
        {
            var startPlant = zone.AllContainedThings.OrderBy(thing => thing.Position.DistanceTo(pawn.Position)).First();
            var actor = wait.actor;
            var fire = (Fire)ThingMaker.MakeThing(ThingDefOf.Fire);
            fire.fireSize = 1f;
            GenSpawn.Spawn(fire, startPlant.Position, Map, Rot4.North);
            actor.pather.StopDead();
        };
        wait.tickAction = delegate
        {
            var actor = wait.actor;
            var growingZone = job.GetTarget(TargetIndex.A).Cell.GetZone(Map);
            if (growingZone == null)
            {
                actor.jobs.EndCurrentJob(JobCondition.Incompletable);
                return;
            }

            if (!job.GetTarget(TargetIndex.A).Cell.GetZone(Map).ContainsStaticFire)
            {
                actor.jobs.EndCurrentJob(JobCondition.Succeeded);
            }
        };
        wait.AddFinishAction(delegate
        {
            var waitingPawn = (Pawn)job.GetTarget(TargetIndex.A).Thing;
            if (waitingPawn != null && waitingPawn.CurJobDef == JobDefOf.Wait_MaintainPosture)
            {
                waitingPawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        });
        wait.FailOnDespawnedOrNull(TargetIndex.A);
        wait.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
        wait.AddEndCondition(() => JobCondition.Ongoing);
        wait.defaultCompleteMode = ToilCompleteMode.Never;
        wait.activeSkill = () => SkillDefOf.Plants;
        yield return wait;
    }
}