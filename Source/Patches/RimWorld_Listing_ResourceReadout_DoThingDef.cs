using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;
using System.Collections.Generic;

namespace RememberResourceReadout.Patches;

/// <summary>
/// Patches to RimWorld.Listing_ResourceReadout.DoThingDef().
/// </summary>
[HarmonyPatch(typeof(Listing_ResourceReadout), "DoThingDef")]
public static class RimWorld_Listing_ResourceReadout_DoThingDef
{

    // protected/private members: resolved once and cached, not per-call, since
    // this Postfix runs for every visible row on every GUI pass (Layout + Repaint,
    // every frame the panel is shown) and re-resolving via AccessTools each time
    // was severe enough to tank framerate once the panel had rows to render.
    private static readonly AccessTools.FieldRef<Listing_ResourceReadout, float> curYField =
        AccessTools.FieldRefAccess<Listing_ResourceReadout, float>("curY");
    private static readonly PropertyInfo LabelWidthProperty =
        AccessTools.Property(typeof(Listing_ResourceReadout), "LabelWidth");
    private static readonly AccessTools.FieldRef<Listing_ResourceReadout, float> lineHeightField =
        AccessTools.FieldRefAccess<Listing_ResourceReadout, float>("lineHeight");
    private static readonly MethodInfo XAtIndentLevelMethod =
        AccessTools.Method(typeof(Listing_ResourceReadout), "XAtIndentLevel");

    /// <summary>
    /// Runs before the vanilla Listing_ResourceReadout.DoThingDef() method to add a right-click 
    /// context menu to the resource readout rows.
    /// </summary>
    /// <param name="__instance">Original Listing_ResourceReadout instance</param>
    /// <param name="thingDef">The ThingDef for which to display resource information</param>
    /// <param name="nestLevel">The nesting level for indentation</param>
    static void Prefix(Listing_ResourceReadout __instance, ref ThingDef thingDef, ref int nestLevel)
    {
        // logic from vanilla Listing_ResourceReadout.DoThingDef()
        if (Find.CurrentMap.resourceCounter.GetCount(thingDef) == 0)
            return;

        float curY = curYField(__instance);
        float LabelWidth = (float)LabelWidthProperty.GetValue(__instance);
        float lineHeight = (float)lineHeightField(__instance);

        Rect rect1 = new Rect(0.0f, curY, LabelWidth, lineHeight);
        rect1.xMin = (float)XAtIndentLevelMethod.Invoke(__instance, new object[] { nestLevel }) + 18f;

        if (Mouse.IsOver(rect1) && Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            Event.current.Use();
            
            List<FloatMenuOption> options = new List<FloatMenuOption>
            {
                new FloatMenuOption("Expand All", () => 
                    Current.Game.GetComponent<RememberResourceReadoutComponent>().OpenAll()
                ),
                new FloatMenuOption("Contract All", () => 
                    Current.Game.GetComponent<RememberResourceReadoutComponent>().CloseAll()
                )
            };

            Find.WindowStack.Add(new FloatMenu(options));

        }
    }
}