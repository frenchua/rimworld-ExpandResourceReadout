using HarmonyLib;
using UnityEngine;
using Verse;
using RimWorld;
using System.Reflection;
using System;

namespace ExpandResourceReadout.Patches;

[HarmonyPatch(typeof(Listing_ResourceReadout), "DoCategory")]
public static class RimWorld_Listing_ResourceReadout_DoCategory
{

    static void Prefix(Listing_ResourceReadout __instance, ref TreeNode_ThingCategory node, ref int nestLevel, ref int openMask)
    {
        var curYField = AccessTools.FieldRefAccess<Listing_ResourceReadout, float>("curY");
        var LabelWidthProperty = AccessTools.Property(typeof(Listing_ResourceReadout), "LabelWidth");
        var lineHeightField = AccessTools.FieldRefAccess<Listing_ResourceReadout, float>("lineHeight");
        float curY = (float)curYField(__instance);
        float LabelWidth = (float)LabelWidthProperty.GetValue(__instance);
        float lineHeight = (float)lineHeightField(__instance);
        Rect rect = new Rect(0.0f, curY, LabelWidth, lineHeight);
        //rect.xMin = XAtIndentLevel(nestLevel) + 18f;
        Log.Error($"Prefix called: {curY} LabelWidth: {LabelWidthProperty.GetValue(__instance)} LineHeight: {lineHeightField(__instance)} curY: {curY}");
        }
    
    static void PostFix()
    {
        Log.Error("DoCategory PostFix");
    }
}

[HarmonyPatch(typeof(Listing_ResourceReadout), "DoThingDef")]
public static class RimWorld_Listing_ResourceReadout_DoThingDef
{
    static void Prefix(ref ThingDef thingDef, ref int nestLevel)
    {
        //Log.Error("DoThingDef Prefix called");
    }
    
    static void PostFix() {
        Log.Error("DoThingDef PostFix");
    }
}