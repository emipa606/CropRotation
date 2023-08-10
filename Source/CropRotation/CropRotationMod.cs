using Mlie;
using UnityEngine;
using Verse;

namespace CropRotation;

[StaticConstructorOnStartup]
internal class CropRotationMod : Mod
{
    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static CropRotationMod instance;

    private static string currentVersion;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="content"></param>
    public CropRotationMod(ModContentPack content) : base(content)
    {
        instance = this;
        Settings = GetSettings<CropRotationSettings>();
        currentVersion = VersionFromManifest.GetVersionFromModMetaData(content.ModMetaData);
    }

    /// <summary>
    ///     The instance-settings for the mod
    /// </summary>
    internal CropRotationSettings Settings { get; }

    /// <summary>
    ///     The title for the mod-settings
    /// </summary>
    /// <returns></returns>
    public override string SettingsCategory()
    {
        return "Crop Rotation";
    }

    /// <summary>
    ///     The settings-window
    ///     For more info: https://rimworldwiki.com/wiki/Modding_Tutorials/ModSettings
    /// </summary>
    /// <param name="rect"></param>
    public override void DoSettingsWindowContents(Rect rect)
    {
        var listing_Standard = new Listing_Standard();
        listing_Standard.Begin(rect);
        listing_Standard.Label("CropRotation.ChangeValue".Translate(Settings.ChangeValue.ToStringPercent()), -1,
            "CropRotation.ChangeValueTT".Translate());
        var sliderRect = listing_Standard.GetRect(30);
        Settings.ChangeValue = Widgets.HorizontalSlider_NewTemp(sliderRect, Settings.ChangeValue,
            0.01f, 0.25f, false, Settings.ChangeValue.ToStringPercent());
        TooltipHandler.TipRegion(sliderRect, "CropRotation.ChangeValueTT".Translate());
        listing_Standard.Gap();

        listing_Standard.Label("CropRotation.HighLimit".Translate(Settings.HighLimit.ToStringPercent()), -1,
            "CropRotation.HighLimitTT".Translate());
        sliderRect = listing_Standard.GetRect(30);
        Settings.HighLimit = Widgets.HorizontalSlider_NewTemp(sliderRect, Settings.HighLimit, 1, 2.5f,
            false, Settings.HighLimit.ToStringPercent());
        TooltipHandler.TipRegion(sliderRect, "CropRotation.HighLimitTT".Translate());
        listing_Standard.Gap();

        listing_Standard.Label("CropRotation.LowLimit".Translate(Settings.LowLimit.ToStringPercent()), -1,
            "CropRotation.LowLimitTT".Translate());
        sliderRect = listing_Standard.GetRect(30);
        Settings.LowLimit = Widgets.HorizontalSlider_NewTemp(sliderRect, Settings.LowLimit, 0, 1f,
            false, Settings.LowLimit.ToStringPercent());
        TooltipHandler.TipRegion(sliderRect, "CropRotation.LowLimitTT".Translate());
        listing_Standard.Gap();

        listing_Standard.Label("CropRotation.MaxHistory".Translate(Settings.MaxHistory), -1,
            "CropRotation.MaxHistoryTT".Translate());
        sliderRect = listing_Standard.GetRect(30);
        Settings.MaxHistory = (int)Widgets.HorizontalSlider_NewTemp(sliderRect, Settings.MaxHistory,
            2f, CropRotation.MaxSavedCrops,
            false, Settings.MaxHistory.ToString(), null, null, 1);
        TooltipHandler.TipRegion(sliderRect, "CropRotation.MaxHistoryTT".Translate());

        if (currentVersion != null)
        {
            listing_Standard.Gap();
            GUI.contentColor = Color.gray;
            listing_Standard.Label("CropRotation.ModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listing_Standard.End();
    }
}