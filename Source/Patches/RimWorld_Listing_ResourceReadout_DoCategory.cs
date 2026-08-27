using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;
using System.Collections.Generic;

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
    // was severe enough to tank framerate once the panel had rows to render.
    private static readonly AccessTools.FieldRef<Listing_ResourceReadout, float> curYField =
        AccessTools.FieldRefAccess<Listing_ResourceReadout, float>("curY");
    private static readonly PropertyInfo LabelWidthProperty =
        AccessTools.Property(typeof(Listing_ResourceReadout), "LabelWidth");
    private static readonly AccessTools.FieldRef<Listing_ResourceReadout, float> lineHeightField =
        AccessTools.FieldRefAccess<Listing_ResourceReadout, float>("lineHeight");
    private static readonly MethodInfo XAtIndentLevelMethod =
        AccessTools.Method(typeof(Listing_ResourceReadout), "XAtIndentLevel");

    // A Prefix runs before the original method touches curY, so reading it here is
    // already correct - no need to snapshot/pass state to a Postfix.
    //
    // Vanilla only draws (and advances curY for) this row if countIn != 0; it skips
    // entirely otherwise. We have to replicate that guard, or our hit-test runs against
    // a stale curY left over from the last row that actually drew, for every zero-count
    // node in the tree - letting a skipped node's stale rect steal a click meant for a
    // completely different, unrelated row.
    static void Prefix(Listing_ResourceReadout __instance, ref TreeNode_ThingCategory node, ref int nestLevel, ref int openMask)
    {
        if (Find.CurrentMap.resourceCounter.GetCountIn(node.catDef) == 0)
            return;

        float curY = curYField(__instance);
        float LabelWidth = (float)LabelWidthProperty.GetValue(__instance);
        float lineHeight = (float)lineHeightField(__instance);
        Rect rect = new Rect(0.0f, curY, LabelWidth, lineHeight);
        rect.xMin = (float)XAtIndentLevelMethod.Invoke(__instance, new object[] { nestLevel }) + 18f;

        TreeNode_ThingCategory node_ = node;

        if(Mouse.IsOver(rect) && Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            Event.current.Use();
            
            List<FloatMenuOption> options = new List<FloatMenuOption>
            {
                new FloatMenuOption("Expand All", () => 
                    Current.Game.GetComponent<ExpandResourceReadoutComponent>().OpenAll() 
                ),
                new FloatMenuOption("Close All", () => 
                    Current.Game.GetComponent<ExpandResourceReadoutComponent>().CloseAll() 
                ),
                new FloatMenuOption("Expand This Category", () =>
                    Current.Game.GetComponent<ExpandResourceReadoutComponent>().SetOpenRecursive(node_, true)
                ),
                new FloatMenuOption("Close This Category", () =>
                    Current.Game.GetComponent<ExpandResourceReadoutComponent>().SetOpenRecursive(node_, false)
                )
            };

            Find.WindowStack.Add(new FloatMenu(options));

        }
    }
}
