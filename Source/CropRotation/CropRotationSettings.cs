using Verse;

namespace CropRotation;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class CropRotationSettings : ModSettings
{
    public bool AlwaysLower;
    public float ChangeValue = 0.05f;
    public float FireIncreasePercent = 0.5f;
    public bool FireIncreases = true;
    public float HighLimit = 1.25f;
    public float LowLimit = 0.5f;
    public int MaxHistory = 10;
    public bool RequireResearch = true;
    public bool RequireThree;
    public bool TimeIncreases = true;
    public float TimeIncreasesPerQuandrum = 0.25f;
    public bool VerboseLogging;

    /// <summary>
    ///     Saving and loading the values
    /// </summary>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref VerboseLogging, "VerboseLogging");
        Scribe_Values.Look(ref RequireResearch, "RequireResearch", true);
        Scribe_Values.Look(ref TimeIncreasesPerQuandrum, "TimeIncreasesPerQuandrum", 0.25f);
        Scribe_Values.Look(ref TimeIncreases, "TimeIncreases", true);
        Scribe_Values.Look(ref FireIncreasePercent, "FireIncreasePercent", 0.5f);
        Scribe_Values.Look(ref FireIncreases, "FireIncreases", true);
        Scribe_Values.Look(ref RequireThree, "RequireThree");
        Scribe_Values.Look(ref AlwaysLower, "AlwaysLower");
        Scribe_Values.Look(ref MaxHistory, "MaxHistory", 10);
        Scribe_Values.Look(ref HighLimit, "HighLimit", 1.25f);
        Scribe_Values.Look(ref LowLimit, "LowLimit", 0.5f);
        Scribe_Values.Look(ref ChangeValue, "ChangeValue", 0.05f);
    }

    public void Reset()
    {
        VerboseLogging = false;
        RequireResearch = true;
        TimeIncreasesPerQuandrum = 0.25f;
        TimeIncreases = true;
        FireIncreasePercent = 0.5f;
        FireIncreases = true;
        RequireThree = false;
        AlwaysLower = false;
        MaxHistory = 10;
        HighLimit = 1.25f;
        LowLimit = 0.5f;
        ChangeValue = 0.05f;
    }
}