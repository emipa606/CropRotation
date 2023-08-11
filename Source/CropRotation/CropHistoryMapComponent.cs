using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CropRotation;

public class CropHistoryMapComponent : MapComponent
{
    private Dictionary<IntVec3, string> cropHistory = new Dictionary<IntVec3, string>();
    private List<IntVec3> cropHistoryKeys;
    private List<string> cropHistoryValues;

    private Dictionary<Zone_Growing, string> extraCrops = new Dictionary<Zone_Growing, string>();
    private List<Zone_Growing> extraCropsKeys;
    private List<string> extraCropsValues;

    public CropHistoryMapComponent(Map map) : base(map)
    {
        if (extraCrops == null)
        {
            extraCrops = new Dictionary<Zone_Growing, string>();
        }

        if (cropHistory == null)
        {
            cropHistory = new Dictionary<IntVec3, string>();
        }
    }

    public List<ThingDef> GetExtraCropsForZone(Zone_Growing zone)
    {
        var returnValue = new List<ThingDef>();
        var crops = getExtraCrops(zone);
        if (!crops.Any())
        {
            return returnValue;
        }

        foreach (var plantDef in crops)
        {
            var foundPlant = DefDatabase<ThingDef>.GetNamedSilentFail(plantDef);
            if (foundPlant == null)
            {
                continue;
            }

            returnValue.Add(foundPlant);
        }

        return returnValue;
    }

    public void SaveExtraCrop(Zone_Growing zone, ThingDef plantDef, ThingDef replaceDef)
    {
        var crops = GetExtraCropsForZone(zone);

        if (crops.Contains(plantDef))
        {
            return;
        }

        if (crops.Count == 0)
        {
            extraCrops[zone] = plantDef.defName;
            return;
        }

        if (replaceDef != null && extraCrops[zone].Contains(replaceDef.defName))
        {
            extraCrops[zone] = extraCrops[zone].Replace(replaceDef.defName, plantDef.defName);
            return;
        }

        extraCrops[zone] += $",{plantDef.defName}";
    }

    public void RemoveExtraPlant(Zone_Growing zone, ThingDef plantDef)
    {
        if (plantDef == null)
        {
            return;
        }

        var crops = GetExtraCropsForZone(zone);
        if (!crops.Contains(plantDef))
        {
            return;
        }

        crops.Remove(plantDef);

        extraCrops[zone] = string.Join(",", crops);
    }

    public void RemoveZone(Zone_Growing zone)
    {
        if (extraCrops.ContainsKey(zone))
        {
            extraCrops.Remove(zone);
        }
    }

    public float GetYieldModifier(Plant plant)
    {
        var yieldModifier = 1f;

        if (!CropRotation.IsValidCrop(plant.def))
        {
            return yieldModifier;
        }

        var location = plant.Position;
        if (location.GetZone(plant.Map) is not Zone_Growing)
        {
            return yieldModifier;
        }

        var history = getCropHistory(location);

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
                yieldModifier *= 1 + CropRotationMod.instance.Settings.ChangeValue;
                continue;
            }

            yieldModifier *= 1 - CropRotationMod.instance.Settings.ChangeValue;
        }

        yieldModifier = Math.Min(CropRotationMod.instance.Settings.HighLimit,
            Math.Max(CropRotationMod.instance.Settings.LowLimit, yieldModifier));

        return yieldModifier;
    }

    public void SaveCropToHistory(IntVec3 intVec3, Plant plant)
    {
        var history = getCropHistory(intVec3);
        history.Insert(0, plant.def.defName);
        if (history.Count > CropRotation.MaxSavedCrops)
        {
            history = history.GetRange(0, CropRotation.MaxSavedCrops - 1);
        }

        cropHistory[intVec3] = string.Join(",", history);
    }

    public ThingDef GetNextCrop(Zone_Growing zone, IntVec3 intVec3, ThingDef baseCrop)
    {
        var crops = getExtraCrops(zone);
        if (!crops.Any())
        {
            return baseCrop;
        }

        var currentCrop = intVec3.GetPlant(zone.Map);
        if (currentCrop != null && (currentCrop.def == baseCrop || crops.Contains(currentCrop.def.defName)))
        {
            return currentCrop.def;
        }

        var lastCrop = getLastCrop(intVec3);
        var baseCropIsOk = Command_SetExtraPlantToGrow.CheckIfOk(baseCrop, false, zone);
        if (lastCrop == null && baseCropIsOk)
        {
            return baseCrop;
        }

        if (lastCrop == baseCrop)
        {
            foreach (var crop in crops)
            {
                var newCrop = DefDatabase<ThingDef>.GetNamedSilentFail(crop);
                if (Command_SetExtraPlantToGrow.CheckIfOk(newCrop, false, zone))
                {
                    return newCrop;
                }
            }

            return baseCrop;
        }

        if (lastCrop != null && lastCrop.defName == crops.Last() && baseCropIsOk)
        {
            return baseCrop;
        }

        if (lastCrop != null && !crops.Contains(lastCrop.defName) && baseCropIsOk)
        {
            return baseCrop;
        }

        var startIndex = 0;
        if (lastCrop != null)
        {
            startIndex = crops.IndexOf(lastCrop.defName) + 1;
        }

        for (var index = startIndex; index < crops.Count; index++)
        {
            var crop = crops[index];
            var newCrop = DefDatabase<ThingDef>.GetNamedSilentFail(crop);
            if (Command_SetExtraPlantToGrow.CheckIfOk(newCrop, false, zone))
            {
                return newCrop;
            }
        }

        if (baseCropIsOk || startIndex == 0)
        {
            return baseCrop;
        }

        for (var index = 0; index < startIndex; index++)
        {
            var crop = crops[index];
            var newCrop = DefDatabase<ThingDef>.GetNamedSilentFail(crop);
            if (Command_SetExtraPlantToGrow.CheckIfOk(newCrop, false, zone))
            {
                return newCrop;
            }
        }

        return baseCrop;
    }

    public override void MapGenerated()
    {
        cropHistory = new Dictionary<IntVec3, string>();
        extraCrops = new Dictionary<Zone_Growing, string>();
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref cropHistory, "cropHistory", LookMode.Value, LookMode.Value,
            ref cropHistoryKeys, ref cropHistoryValues);
        Scribe_Collections.Look(ref extraCrops, "extraCrops", LookMode.Reference, LookMode.Value,
            ref extraCropsKeys, ref extraCropsValues);
    }

    private ThingDef getLastCrop(IntVec3 intVec3)
    {
        var crops = getCropHistory(intVec3);

        return !crops.Any() ? null : DefDatabase<ThingDef>.GetNamedSilentFail(crops.First());
    }

    private List<string> getCropHistory(IntVec3 intVec3)
    {
        if (cropHistory == null)
        {
            cropHistory = new Dictionary<IntVec3, string>();
        }

        return cropHistory.TryGetValue(intVec3, out var history) ? history.Split(',').ToList() : new List<string>();
    }

    private List<string> getExtraCrops(Zone_Growing zone)
    {
        if (extraCrops == null)
        {
            extraCrops = new Dictionary<Zone_Growing, string>();
        }

        return extraCrops.TryGetValue(zone, out var extaPlants) ? extaPlants.Split(',').ToList() : new List<string>();
    }
}