using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CropRotation;

public class CropHistoryMapComponent : MapComponent
{
    private Dictionary<IntVec3, string> cropHistory;
    private List<IntVec3> cropHistoryKeys;
    private List<string> cropHistoryValues;

    public CropHistoryMapComponent(Map map) : base(map)
    {
    }

    public List<string> GetCropHistory(IntVec3 intVec3)
    {
        return cropHistory.TryGetValue(intVec3, out var history) ? history.Split(',').ToList() : new List<string>();
    }

    public float GetYieldModifier(Plant plant)
    {
        var yieldModifier = 1f;

        var location = plant.Position;
        if (location.GetZone(plant.Map) is not Zone_Growing)
        {
            return yieldModifier;
        }

        var history = GetCropHistory(location);

        if (!history.Any())
        {
            return yieldModifier;
        }

        if (history.Count == 1)
        {
            return yieldModifier;
        }

        for (var i = 0; i < Math.Min(history.Count, CropRotationMod.instance.Settings.MaxHistory) - 1; i++)
        {
            if (history[i] != history[i + 1])
            {
                yieldModifier *= CropRotationMod.instance.Settings.ChangeValue;
                break;
            }

            yieldModifier *= 1 - CropRotationMod.instance.Settings.ChangeValue;
        }

        yieldModifier = Math.Min(CropRotationMod.instance.Settings.HighLimit,
            Math.Max(CropRotationMod.instance.Settings.LowLimit, yieldModifier));

        return yieldModifier;
    }

    public void SaveCrop(IntVec3 intVec3, Plant plant)
    {
        var history = GetCropHistory(intVec3);
        history.Insert(0, plant.def.defName);
        if (history.Count > CropRotation.MaxSavedCrops)
        {
            history = history.GetRange(0, CropRotation.MaxSavedCrops - 1);
        }

        cropHistory[intVec3] = string.Join(",", history);
    }

    public override void MapGenerated()
    {
        cropHistory = new Dictionary<IntVec3, string>();
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref cropHistory, "cropHistory", LookMode.Value, LookMode.Value,
            ref cropHistoryKeys, ref cropHistoryValues);
    }
}