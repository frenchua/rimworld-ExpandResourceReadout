using Verse;
using RimWorld;
using UnityEngine;
using HarmonyLib;

namespace ExpandResourceReadout;

public class Mod : Verse.Mod {

    public const string HarmonyId = "com.jdfrench.RimWorldExpandResourceReadout";
    public Mod(ModContentPack content) : base(content) {

        Harmony.DEBUG = true;
        var harmony = new Harmony(HarmonyId);
        harmony.PatchAll();
        Log.Message($"[{HarmonyId}] Harmony patches applied.");

        GetSettings<ExpandResourceReadoutSettings>();
    }

    public override void DoSettingsWindowContents(Rect inRect) {
        Log.Message($"[{HarmonyId}] DoSettingsWindowContents called.");
    }
}