using Verse;
using UnityEngine;
using HarmonyLib;

namespace RememberResourceReadout;

public class Mod : Verse.Mod {

    public const string HarmonyId = "com.jdfrench.RememberResourceReadout";

    public static RememberResourceReadoutSettings Settings { get; private set; }

    public Mod(ModContentPack content) : base(content) {

        var harmony = new Harmony(HarmonyId);
        harmony.PatchAll();

        Settings = GetSettings<RememberResourceReadoutSettings>();
    }

    public override string SettingsCategory() => Content.Name;

    public override void DoSettingsWindowContents(Rect inRect) {
        Listing_Standard listing = new Listing_Standard();
        listing.Begin(inRect);

        listing.Label("Remember resource readout expand/collapse state:");
        if (listing.RadioButton("Globally (shared by every save)", !Settings.persistPerSave))
        {
            Settings.persistPerSave = false;
            ApplyToCurrentGame();
        }
        if (listing.RadioButton("Per save (each save remembers its own)", Settings.persistPerSave))
        {
            Settings.persistPerSave = true;
            ApplyToCurrentGame();
        }

        listing.End();
        base.DoSettingsWindowContents(inRect);
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        ApplyToCurrentGame();
    }

    private static void ApplyToCurrentGame()
    {
        Current.Game?.GetComponent<RememberResourceReadoutComponent>()?.ApplyFromCurrentMode();
    }
}
