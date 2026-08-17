using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;
using ExpandResourceReadout;
using System.Reflection;
using System.Collections.Generic;
using System;

namespace ExpandResourceReadout.Patches;

/// <summary>
/// Patches to RimWorld.Listing_ResourceReadout.DoThingDef().
/// </summary>
[HarmonyPatch(typeof(Listing_ResourceReadout), "DoThingDef")]
public static class RimWorld_Listing_ResourceReadout_DoThingDef
{

    // protected/private members: resolved once and cached, not per-call, since
    // this Postfix runs for every visible row on every GUI pass (Layout + Repaint,
    // every frame the panel is shown) and re-resolving via AccessTools each time
    // was severe enough to tank framerate once a stockpile made the panel visible.
    private static readonly AccessTools.FieldRef<Listing_ResourceReadout, float> curYField =
        AccessTools.FieldRefAccess<Listing_ResourceReadout, float>("curY");
    private static readonly PropertyInfo LabelWidthProperty =
        AccessTools.Property(typeof(Listing_ResourceReadout), "LabelWidth");
    private static readonly AccessTools.FieldRef<Listing_ResourceReadout, float> lineHeightField =
        AccessTools.FieldRefAccess<Listing_ResourceReadout, float>("lineHeight");
    private static readonly MethodInfo XAtIndentLevelMethod =
        AccessTools.Method(typeof(Listing_ResourceReadout), "XAtIndentLevel");

    // Vanilla builds this row's Rect from curY as read on entry, then calls EndLine()
    // before returning, advancing curY to the next row. Reading curY in the Postfix
    // therefore lands on the NEXT row, not this one - snapshot it in a Prefix instead,
    // before the original method moves it.
    static void Prefix(Listing_ResourceReadout __instance, out float __state)
    {
        __state = curYField(__instance);
    }

    static void Postfix(Listing_ResourceReadout __instance, ref ThingDef thingDef, ref int nestLevel, float __state)
    {
        float curY = __state;
        float LabelWidth = (float)LabelWidthProperty.GetValue(__instance);
        float lineHeight = lineHeightField(__instance);

        Rect rect1 = new Rect(0.0f, curY, LabelWidth, lineHeight);
        rect1.xMin = (float)XAtIndentLevelMethod.Invoke(__instance, new object[] { nestLevel }) + 18f;

        if (Mouse.IsOver(rect1) && Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            Event.current.Use();

            ExpandResourceReadoutComponent component = Current.Game.GetComponent<ExpandResourceReadoutComponent>();
            TreeNode_ThingCategory parentNode = component.FindContainingNode(thingDef);
            List<FloatMenuOption> options = new List<FloatMenuOption>
            {
                new FloatMenuOption("Expand All", () =>
                    component.OpenSubtree(parentNode)
                ),
                new FloatMenuOption("Close All", () =>
                    component.CloseSubtree(parentNode)
                ),
                new FloatMenuOption("Expand Everything", () =>
                    component.OpenAll()
                ),
                new FloatMenuOption("Close Everything", () =>
                    component.CloseAll()
                )
            };

            Find.WindowStack.Add(new FloatMenu(options));

        }
    }
}