using Mlie;
using UnityEngine;
using Verse;

namespace CropRotation;

[StaticConstructorOnStartup]
internal class CropRotationMod : Mod
{
    private const float SliderHeight = 26;

    /// <summary>
    ///     The instance of the settings to be read by the mod
    /// </summary>
    public static CropRotationMod Instance;

    private static string currentVersion;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="content"></param>
    public CropRotationMod(ModContentPack content) : base(content)
    {
        Instance = this;
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
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(rect);
        listingStandard.ColumnWidth = rect.width / 2.05f;

        listingStandard.Label("CropRotation.ChangeValue".Translate(Settings.ChangeValue.ToStringPercent()), -1,
            "CropRotation.ChangeValueTT".Translate());
        var sliderRect = listingStandard.GetRect(SliderHeight);
        sliderRect.width -= SliderHeight;
        sliderRect.x += SliderHeight / 2;
        Settings.ChangeValue = Widgets.HorizontalSlider(sliderRect, Settings.ChangeValue,
            0.01f, 0.25f, false, Settings.ChangeValue.ToStringPercent());
        TooltipHandler.TipRegion(sliderRect, "CropRotation.ChangeValueTT".Translate());
        listingStandard.Gap();

        listingStandard.Label("CropRotation.HighLimit".Translate(Settings.HighLimit.ToStringPercent()), -1,
            "CropRotation.HighLimitTT".Translate());
        sliderRect = listingStandard.GetRect(SliderHeight);
        sliderRect.width -= SliderHeight;
        sliderRect.x += SliderHeight / 2;
        Settings.HighLimit = Widgets.HorizontalSlider(sliderRect, Settings.HighLimit, 1, 2.5f,
            false, Settings.HighLimit.ToStringPercent());
        TooltipHandler.TipRegion(sliderRect, "CropRotation.HighLimitTT".Translate());
        listingStandard.Gap();

        listingStandard.Label("CropRotation.LowLimit".Translate(Settings.LowLimit.ToStringPercent()), -1,
            "CropRotation.LowLimitTT".Translate());
        sliderRect = listingStandard.GetRect(SliderHeight);
        sliderRect.width -= SliderHeight;
        sliderRect.x += SliderHeight / 2;
        Settings.LowLimit = Widgets.HorizontalSlider(sliderRect, Settings.LowLimit, 0, 1f,
            false, Settings.LowLimit.ToStringPercent());
        TooltipHandler.TipRegion(sliderRect, "CropRotation.LowLimitTT".Translate());
        listingStandard.Gap();

        listingStandard.Label("CropRotation.MaxHistory".Translate(Settings.MaxHistory), -1,
            "CropRotation.MaxHistoryTT".Translate());
        sliderRect = listingStandard.GetRect(SliderHeight);
        sliderRect.width -= SliderHeight;
        sliderRect.x += SliderHeight / 2;
        Settings.MaxHistory = (int)Widgets.HorizontalSlider(sliderRect, Settings.MaxHistory,
            2f, CropRotation.MaxSavedCrops,
            false, Settings.MaxHistory.ToString(), null, null, 1);
        TooltipHandler.TipRegion(sliderRect, "CropRotation.MaxHistoryTT".Translate());

        listingStandard.Gap();
        listingStandard.CheckboxLabeled("CropRotation.VerboseLogging".Translate(), ref Settings.VerboseLogging,
            "CropRotation.VerboseLoggingTT".Translate());
        if (listingStandard.ButtonText("Reset".Translate()))
        {
            Settings.Reset();
        }

        if (currentVersion != null)
        {
            listingStandard.Gap();
            GUI.contentColor = Color.gray;
            listingStandard.Label("CropRotation.ModVersion".Translate(currentVersion));
            GUI.contentColor = Color.white;
        }

        listingStandard.NewColumn();

        if (Current.Game != null)
        {
            listingStandard.Label("CropRotation.RequireResearch".Translate(), -1f,
                "CropRotation.RequireResearchOngoingTT".Translate());
        }
        else
        {
            listingStandard.CheckboxLabeled("CropRotation.RequireResearch".Translate(), ref Settings.RequireResearch,
                "CropRotation.RequireResearchTT".Translate());
        }

        listingStandard.CheckboxLabeled("CropRotation.RequireThree".Translate(), ref Settings.RequireThree,
            "CropRotation.RequireThreeTT".Translate());
        listingStandard.CheckboxLabeled("CropRotation.AlwaysLower".Translate(), ref Settings.AlwaysLower,
            "CropRotation.AlwaysLowerTT".Translate());

        listingStandard.CheckboxLabeled("CropRotation.FireIncreases".Translate(), ref Settings.FireIncreases,
            "CropRotation.FireIncreasesTT".Translate());
        if (Settings.FireIncreases)
        {
            listingStandard.Label(
                "CropRotation.FireIncreasePercent".Translate(Settings.FireIncreasePercent.ToStringPercent()), -1,
                "CropRotation.FireIncreasePercentTT".Translate());
            sliderRect = listingStandard.GetRect(SliderHeight);
            sliderRect.width -= SliderHeight;
            sliderRect.x += SliderHeight / 2;
            Settings.FireIncreasePercent = Widgets.HorizontalSlider(sliderRect, Settings.FireIncreasePercent,
                0.01f, 1f, false, Settings.FireIncreasePercent.ToStringPercent());
            TooltipHandler.TipRegion(sliderRect, "CropRotation.FireIncreasePercentTT".Translate());
            listingStandard.Gap();
        }

        listingStandard.CheckboxLabeled("CropRotation.TimeIncreases".Translate(), ref Settings.TimeIncreases,
            "CropRotation.TimeIncreasesTT".Translate());
        if (Settings.TimeIncreases)
        {
            listingStandard.Label(
                "CropRotation.TimeIncreasesPerQuandrum".Translate(Settings.TimeIncreasesPerQuandrum.ToStringPercent()),
                -1,
                "CropRotation.TimeIncreasesPerQuandrumTT".Translate());
            sliderRect = listingStandard.GetRect(SliderHeight);
            sliderRect.width -= SliderHeight;
            sliderRect.x += SliderHeight / 2;
            Settings.TimeIncreasesPerQuandrum = Widgets.HorizontalSlider(sliderRect,
                Settings.TimeIncreasesPerQuandrum,
                0.01f, 1f, false, Settings.TimeIncreasesPerQuandrum.ToStringPercent());
            TooltipHandler.TipRegion(sliderRect, "CropRotation.TimeIncreasesPerQuandrumTT".Translate());
            listingStandard.Gap();
        }

        listingStandard.End();
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        CropRotation.UpdateResearchSetting();
    }
}