using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;
using System.Collections.Generic;
using System;

namespace ExpandResourceReadout.Patches;

/// <summary>
/// Patches to RimWorld.Listing_ResourceReadout.DoCategory().
/// </summary>
[HarmonyPatch(typeof(Listing_ResourceReadout), "DoCategory")]
public static class RimWorld_Listing_ResourceReadout_DoCategory
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
    // (and, if open, DoCategoryChildren()) before returning, advancing curY well past
    // this row. Reading curY in the Postfix therefore lands on the NEXT row, not this
    // one - snapshot it in a Prefix instead, before the original method moves it.
    static void Prefix(Listing_ResourceReadout __instance, out float __state)
    {
        __state = curYField(__instance);
    }

    static void Postfix(Listing_ResourceReadout __instance, ref TreeNode_ThingCategory node, ref int nestLevel, ref int openMask, float __state)
    {
        float curY = __state;
        float LabelWidth = (float)LabelWidthProperty.GetValue(__instance);
        float lineHeight = lineHeightField(__instance);
        Rect rect = new Rect(0.0f, curY, LabelWidth, lineHeight);
        rect.xMin = (float)XAtIndentLevelMethod.Invoke(__instance, new object[] { nestLevel }) + 18f;

        if(Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            Event.current.Use();

            TreeNode_ThingCategory clickedNode = node;
            List<FloatMenuOption> options = new List<FloatMenuOption>
            {
                new FloatMenuOption("Expand All", () =>
                    Current.Game.GetComponent<ExpandResourceReadoutComponent>().OpenSubtree(clickedNode)
                ),
                new FloatMenuOption("Close All", () =>
                    Current.Game.GetComponent<ExpandResourceReadoutComponent>().CloseSubtree(clickedNode)
                ),
                new FloatMenuOption("Expand Everything", () =>
                    Current.Game.GetComponent<ExpandResourceReadoutComponent>().OpenAll()
                ),
                new FloatMenuOption("Close Everything", () =>
                    Current.Game.GetComponent<ExpandResourceReadoutComponent>().CloseAll()
                )
            };

            Find.WindowStack.Add(new FloatMenu(options));

        }
    }
}
