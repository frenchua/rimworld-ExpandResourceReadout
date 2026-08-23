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
        // OpenCloseWidget() returns true if the open bit was flipped, false if it was already in the requested state. We only want to 
        // persist state when the user actually changed something.
        if (!__result)
            return;
        // Only persist state if the openMask includes the ResourceReadout bit, since this mod only cares about that one.
        if ((openMask & TreeOpenMasks.ResourceReadout) == 0)
            return;
        Current.Game?.GetComponent<ExpandResourceReadoutComponent>()?.PersistState();
    }
}
