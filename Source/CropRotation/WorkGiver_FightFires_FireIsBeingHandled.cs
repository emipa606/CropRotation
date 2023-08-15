using HarmonyLib;
using RimWorld;
using Verse;

namespace CropRotation;

[HarmonyPatch(typeof(WorkGiver_FightFires), nameof(WorkGiver_FightFires.FireIsBeingHandled))]
public static class WorkGiver_FightFires_FireIsBeingHandled
{
    public static void Postfix(Fire f, ref bool __result)
    {
        if (__result)
        {
            return;
        }

        if (f?.Position.GetZone(f.Map) is not Zone_Growing zone)
        {
            return;
        }

        var mapComponent = f.Map?.GetComponent<CropHistoryMapComponent>();

        if (mapComponent == null)
        {
            return;
        }

        if (!mapComponent.GetZonesToBurn().Contains(zone))
        {
            return;
        }

        __result = true;
    }
}