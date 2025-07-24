using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace CropRotation;

public class CropHistoryMapComponent : MapComponent
{
    private Dictionary<IntVec3, int> cropEmptyTimer = new();
    private List<IntVec3> cropEmptyTimerKeys;
    private List<int> cropEmptyTimerValues;

    private Dictionary<IntVec3, string> cropHistory = new();
    private List<IntVec3> cropHistoryKeys;
    private List<string> cropHistoryValues;

    private Dictionary<Zone_Growing, string> extraCrops = new();
    private List<Zone_Growing> extraCropsKeys;
    private List<string> extraCropsValues;

    private Dictionary<Zone_Growing, string> seasonCrops = new();
    private List<Zone_Growing> seasonCropsKeys;
    private List<string> seasonCropsValues;

    private List<Zone_Growing> zoneToBurn = [];

    public CropHistoryMapComponent(Map map) : base(map)
    {
        extraCrops ??= new Dictionary<Zone_Growing, string>();

        cropHistory ??= new Dictionary<IntVec3, string>();

        cropEmptyTimer ??= new Dictionary<IntVec3, int>();

        zoneToBurn ??= [];
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

    public void RemoveExtraCropZone(Zone_Growing zone)
    {
        extraCrops.Remove(zone);
    }

    public float GetYieldModifier(Plant plant)
    {
        var yieldModifier = 1f;
        if (plant == null || !CropRotation.IsValidCrop(plant.def))
        {
            return yieldModifier;
        }

        return GetYieldModifier(plant.Position, plant.Map);
    }

    public float GetYieldModifier(IntVec3 position, Map plantMap)
    {
        var yieldModifier = 1f;

        if (position.GetZone(plantMap) is not Zone_Growing)
        {
            return yieldModifier;
        }

        var history = getCropHistory(position);

        if (!history.Any())
        {
            return yieldModifier;
        }

        var iterator = 0;
        string lastCrop = null;
        string lastLastCrop = null;
        var lowerFactor = 1 - CropRotationMod.Instance.Settings.ChangeValue;
        var higherFactor = 1 + CropRotationMod.Instance.Settings.ChangeValue;
        if (CropRotationMod.Instance.Settings.AlwaysLower)
        {
            higherFactor = 1 - (CropRotationMod.Instance.Settings.ChangeValue / 2);
        }

        var yieldList = new List<float>();

        foreach (var currentCrop in history)
        {
            if (iterator >= CropRotationMod.Instance.Settings.MaxHistory)
            {
                break;
            }

            if (currentCrop.StartsWith("["))
            {
                var effectTuple = currentCrop[1..].Split(']');

                switch (effectTuple[0])
                {
                    case "Fire" when CropRotationMod.Instance.Settings.FireIncreases:
                        if (!float.TryParse(effectTuple[1], out var effectFloat))
                        {
                            CropRotation.LogMessage(
                                $"Failed to parse fire effect in crop history for {position}: {currentCrop}",
                                warning: true);
                            continue;
                        }

                        yieldList.Add(8 + (CropRotationMod.Instance.Settings.FireIncreasePercent * effectFloat));
                        continue;
                    case "Time" when CropRotationMod.Instance.Settings.TimeIncreases:
                        if (!int.TryParse(effectTuple[1], out var effectInt))
                        {
                            CropRotation.LogMessage(
                                $"Failed to parse time effect in crop history for {position}: {currentCrop}",
                                warning: true);
                            continue;
                        }

                        var timeIncrease = (float)effectInt / GenDate.TicksPerQuadrum *
                                           CropRotationMod.Instance.Settings.TimeIncreasesPerQuandrum;
                        yieldList.Add(16 + timeIncrease);

                        continue;
                }

                continue;
            }

            iterator++;

            if (lastCrop == null)
            {
                lastCrop = currentCrop;
                if (cropEmptyTimer?.ContainsKey(position) == true &&
                    GenTicks.TicksGame - cropEmptyTimer[position] > GenDate.TicksPerDay)
                {
                    var timeIncrease = (float)(GenTicks.TicksGame - cropEmptyTimer[position]) /
                                       GenDate.TicksPerQuadrum *
                                       CropRotationMod.Instance.Settings.TimeIncreasesPerQuandrum;
                    yieldList.Add(16 + timeIncrease);
                    CropRotation.LogMessage(
                        $"Increased {position} with {timeIncrease} based on time. EffectInt: {GenTicks.TicksGame - cropEmptyTimer[position]}");
                }

                continue;
            }

            if (CropRotationMod.Instance.Settings.RequireThree)
            {
                if (lastLastCrop == null)
                {
                    if (lastCrop == currentCrop)
                    {
                        yieldList.Add(lowerFactor);
                    }

                    lastLastCrop = lastCrop;
                    lastCrop = currentCrop;
                    continue;
                }

                var cropHashSet = new HashSet<string>
                {
                    currentCrop,
                    lastCrop,
                    lastLastCrop
                };

                switch (cropHashSet.Count)
                {
                    case 1:
                        yieldList.Add(lowerFactor);
                        break;
                    case 3:
                        yieldList.Add(higherFactor);
                        break;
                }

                lastLastCrop = lastCrop;
                lastCrop = currentCrop;
                continue;
            }

            if (lastCrop == currentCrop)
            {
                yieldList.Add(lowerFactor);
                continue;
            }

            yieldList.Add(higherFactor);
        }

        yieldList.Reverse();
        foreach (var yieldMultiplier in yieldList)
        {
            switch (yieldMultiplier)
            {
                case > 16 when yieldModifier >= 1f:
                    continue;
                case > 16:
                    yieldModifier *= yieldMultiplier - 15;
                    continue;
                case > 8:
                    yieldModifier *= yieldMultiplier - 7;
                    continue;
                default:
                    yieldModifier *= yieldMultiplier;
                    break;
            }
        }

        yieldModifier = Math.Min(CropRotationMod.Instance.Settings.HighLimit,
            Math.Max(CropRotationMod.Instance.Settings.LowLimit, yieldModifier));

        return yieldModifier;
    }

    public void SaveCropToHistory(IntVec3 intVec3, Plant plant)
    {
        SaveStringToHistory(intVec3, plant.def.defName);
    }

    public void SaveStringToHistory(IntVec3 intVec3, string effect)
    {
        var history = getCropHistory(intVec3);
        history.Insert(0, effect);
        if (history.Count > CropRotation.MaxSavedCrops * 2)
        {
            history = history.GetRange(0, (CropRotation.MaxSavedCrops * 2) - 1);
        }

        cropHistory[intVec3] = string.Join(",", history);
    }

    public void SaveEmptyTimestamp(IntVec3 intVec3)
    {
        cropEmptyTimer ??= new Dictionary<IntVec3, int>();

        cropEmptyTimer[intVec3] = GenTicks.TicksGame;
    }

    public void SaveZoneToBurn(Zone_Growing zone)
    {
        zoneToBurn ??= [];

        zoneToBurn.Add(zone);
    }

    public void RemoveSeasonalZone(Zone_Growing zone)
    {
        seasonCrops ??= new Dictionary<Zone_Growing, string>();

        seasonCrops.Remove(zone);
    }

    public List<ThingDef> GetSeasonalCrops(Zone_Growing zone)
    {
        var returnValue = new List<ThingDef>();
        var currentSeasonalCrops = getSeasonCrops(zone);

        if (!currentSeasonalCrops.Any())
        {
            return returnValue;
        }

        foreach (var plantDef in currentSeasonalCrops)
        {
            var foundPlant = DefDatabase<ThingDef>.GetNamedSilentFail(plantDef);
            if (foundPlant == null)
            {
                continue;
            }

            returnValue.Add(foundPlant);
        }

        return returnValue.Count != 3 ? [] : returnValue;
    }

    public void SaveSeasonalZone(Zone_Growing zone)
    {
        seasonCrops ??= new Dictionary<Zone_Growing, string>();

        if (seasonCrops.ContainsKey(zone))
        {
            return;
        }

        RemoveExtraCropZone(zone);
        var cropString = zone.PlantDefToGrow.defName;
        seasonCrops[zone] = $"{cropString},{cropString},{cropString}";
    }

    public void SetPlantForSeason(Zone_Growing zone, Season season, ThingDef cropDef)
    {
        var currentSeasonCrops = getSeasonCrops(zone);
        if (currentSeasonCrops.Count != 3)
        {
            CropRotation.LogMessage($"Tried to get season crops for {zone} but got none.", warning: true);
            return;
        }

        switch (season)
        {
            case Season.Summer:
                currentSeasonCrops[0] = cropDef.defName;
                break;
            case Season.Fall:
                currentSeasonCrops[1] = cropDef.defName;
                break;
            case Season.Winter:
                currentSeasonCrops[2] = cropDef.defName;
                break;
        }

        seasonCrops[zone] = $"{currentSeasonCrops[0]},{currentSeasonCrops[1]},{currentSeasonCrops[2]}";
    }

    public List<Zone_Growing> GetZonesToBurn()
    {
        zoneToBurn ??= [];

        return zoneToBurn;
    }

    public void RemoveZoneToBurn(Zone_Growing zone)
    {
        zoneToBurn ??= [];

        if (zoneToBurn.Contains(zone))
        {
            zoneToBurn.Remove(zone);
        }
    }

    public void SaveNotEmptyTimestamp(IntVec3 intVec3)
    {
        cropEmptyTimer ??= new Dictionary<IntVec3, int>();

        if (!cropEmptyTimer.TryGetValue(intVec3, out var value))
        {
            return;
        }

        var emptyTime = GenTicks.TicksGame - value;
        cropEmptyTimer.Remove(intVec3);
        if (emptyTime < GenDate.TicksPerDay)
        {
            return;
        }

        SaveStringToHistory(intVec3, $"[Time]{emptyTime}");
    }

    public ThingDef GetNextCrop(Zone_Growing zone, IntVec3 intVec3, ThingDef baseCrop)
    {
        var seasonic = getSeasonCrops(zone);
        if (seasonic.Count == 3)
        {
            var currentSownCrop = intVec3.GetPlant(zone.Map);
            if (currentSownCrop != null && seasonic.Contains(currentSownCrop.def.defName))
            {
                return currentSownCrop.def;
            }

            switch (GenLocalDate.Season(map))
            {
                case Season.Summer:
                    return ThingDef.Named(seasonic[0]);
                case Season.Fall:
                    return ThingDef.Named(seasonic[1]);
                case Season.Winter:
                    return ThingDef.Named(seasonic[2]);
                default:
                    return zone.PlantDefToGrow;
            }
        }

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

        if (lastCrop != null && lastCrop.defName == crops.Last() && baseCropIsOk ||
            lastCrop != null && !crops.Contains(lastCrop.defName) && baseCropIsOk)
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

    public ThingDef GetPlantDefForSeason(Season season, Zone_Growing zone)
    {
        return getPlantDefForSeason(season, getSeasonCrops(zone));
    }

    private static ThingDef getPlantDefForSeason(Season season, List<string> seasonalCrops)
    {
        if (!seasonalCrops.Any() || seasonalCrops.Count != 3)
        {
            return null;
        }

        switch (season)
        {
            case Season.Summer:
                return ThingDef.Named(seasonalCrops[0]);
            case Season.Fall:
                return ThingDef.Named(seasonalCrops[1]);
            case Season.Winter:
                return ThingDef.Named(seasonalCrops[2]);
            default:
                return null;
        }
    }

    public override void MapGenerated()
    {
        cropHistory = new Dictionary<IntVec3, string>();
        extraCrops = new Dictionary<Zone_Growing, string>();
        seasonCrops = new Dictionary<Zone_Growing, string>();
        cropEmptyTimer = new Dictionary<IntVec3, int>();
    }

    public override void ExposeData()
    {
        Scribe_Collections.Look(ref cropHistory, "cropHistory", LookMode.Value, LookMode.Value,
            ref cropHistoryKeys, ref cropHistoryValues);
        Scribe_Collections.Look(ref seasonCrops, "seasonCrops", LookMode.Reference, LookMode.Value,
            ref seasonCropsKeys, ref seasonCropsValues);
        Scribe_Collections.Look(ref extraCrops, "extraCrops", LookMode.Reference, LookMode.Value,
            ref extraCropsKeys, ref extraCropsValues);
        Scribe_Collections.Look(ref cropEmptyTimer, "cropEmptyTimer", LookMode.Value, LookMode.Value,
            ref cropEmptyTimerKeys, ref cropEmptyTimerValues);
        Scribe_Collections.Look(ref zoneToBurn, "zoneToBurn", LookMode.Reference);
    }

    private ThingDef getLastCrop(IntVec3 intVec3)
    {
        var crops = getCropHistory(intVec3);

        if (!crops.Any())
        {
            return null;
        }

        foreach (var crop in crops)
        {
            if (crop.StartsWith("["))
            {
                continue;
            }

            return DefDatabase<ThingDef>.GetNamedSilentFail(crop);
        }

        return null;
    }

    private List<string> getCropHistory(IntVec3 intVec3)
    {
        cropHistory ??= new Dictionary<IntVec3, string>();

        return cropHistory.TryGetValue(intVec3, out var history) ? history.Split(',').ToList() : [];
    }

    private List<string> getExtraCrops(Zone_Growing zone)
    {
        extraCrops ??= new Dictionary<Zone_Growing, string>();

        return extraCrops.TryGetValue(zone, out var extraPlants) ? extraPlants.Split(',').ToList() : [];
    }

    private List<string> getSeasonCrops(Zone_Growing zone)
    {
        seasonCrops ??= new Dictionary<Zone_Growing, string>();

        return seasonCrops.TryGetValue(zone, out var seasonPlants)
            ? seasonPlants.Split(',').ToList()
            : [];
    }
}