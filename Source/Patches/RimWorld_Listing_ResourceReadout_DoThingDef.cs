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

    static void Postfix(Listing_ResourceReadout __instance, ref ThingDef thingDef, ref int nestLevel)
    {
        // protected/private members:
        var curYField = AccessTools.FieldRefAccess<Listing_ResourceReadout, float>("curY");
        var LabelWidthProperty = AccessTools.Property(typeof(Listing_ResourceReadout), "LabelWidth");
        var lineHeightField = AccessTools.FieldRefAccess<Listing_ResourceReadout, float>("lineHeight");
        var XAtIndentLevelMethod = AccessTools.Method(typeof(Listing_ResourceReadout), "XAtIndentLevel");

        float curY = (float)curYField(__instance);
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
                    Current.Game.GetComponent<ExpandResourceReadoutComponent>().OpenAll()
                ),
                new FloatMenuOption("Contract All", () => 
                    Current.Game.GetComponent<ExpandResourceReadoutComponent>().CloseAll()
                )
            };

            Find.WindowStack.Add(new FloatMenu(options));

        }
    }
}