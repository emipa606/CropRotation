using Verse;

namespace CropRotation;

/// <summary>
///     Definition of the settings for the mod
/// </summary>
internal class CropRotationSettings : ModSettings
{
    public float ChangeValue = 0.05f;
    public float HighLimit = 1.25f;
    public float LowLimit = 0.5f;
    public int MaxHistory = 10;

    /// <summary>
    ///     Saving and loading the values
    /// </summary>
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref MaxHistory, "MaxHistory", 10);
        Scribe_Values.Look(ref HighLimit, "HighLimit", 1.25f);
        Scribe_Values.Look(ref LowLimit, "LowLimit", 0.5f);
        Scribe_Values.Look(ref ChangeValue, "ChangeValue", 0.05f);
    }
}