using HarmonyLib;
using Verse;

namespace ExpandResourceReadout.Patches;

/// <summary>
/// Patch to Verse.Listing_Tree.OpenCloseWidget(), which is what actually flips a
/// category's open bit when the player clicks the vanilla expand/collapse arrow
/// (right-click menus in this mod go through ExpandResourceReadoutComponent
/// directly and persist their own state - this patch is what captures manual
/// clicks so the readout's view is fully remembered, not just mod-driven changes).
/// </summary>
[HarmonyPatch(typeof(Listing_Tree), "OpenCloseWidget")]
public static class RimWorld_Listing_Tree_OpenCloseWidget
{
    static void Postfix(int openMask, bool __result)
    {
        if (!__result)
            return;
        if ((openMask & TreeOpenMasks.ResourceReadout) == 0)
            return;
        Current.Game?.GetComponent<ExpandResourceReadoutComponent>()?.PersistState();
    }
}
