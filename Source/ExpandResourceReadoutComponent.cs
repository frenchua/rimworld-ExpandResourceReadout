using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace ExpandResourceReadout;

public class ExpandResourceReadoutComponent : GameComponent
{

    private List<ThingCategoryDef> rootCategories;
    private Dictionary<string, bool> categoryOpenStates = new Dictionary<string, bool>();

    public ExpandResourceReadoutComponent(Game g) : base()
    {
        rootCategories = (from cat in DefDatabase<ThingCategoryDef>.AllDefs
                          where cat.resourceReadoutRoot
                          select cat).ToList<ThingCategoryDef>();
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref categoryOpenStates, "categoryOpenStates", LookMode.Value, LookMode.Value);
        categoryOpenStates ??= new Dictionary<string, bool>();
    }

    public override void LoadedGame()
    {
        base.LoadedGame();
        ApplyFromCurrentMode();
    }

    public override void StartedNewGame()
    {
        base.StartedNewGame();
        ApplyFromCurrentMode();
    }

    // Visit node and its descendants. Every tree traversal in this component
    // (open/close all, open/close a single category, capture/apply state) runs
    // through here.
    private static void WalkFrom(TreeNode_ThingCategory node, Action<TreeNode_ThingCategory> visit)
    {
        // Not sure this is ever false for readout nodes, but it's cheap to keep
        // (carried over from the old OpenRecursive/CloseRecursive).
        if (!node.Openable)
            return;
        visit(node);
        foreach (TreeNode_ThingCategory child in node.ChildCategoryNodes)
        {
            // A child can be resourceReadoutRoot even though it's structurally nested
            // under this node in the def graph (e.g. Plant Matter's parent is Raw
            // Resources, but both are independently resourceReadoutRoot) - the readout
            // draws such children as their own separate top-level rows, not nested
            // under this one, so don't cascade into them. Vanilla's own
            // DoCategoryChildren applies this same guard when drawing. Such roots are
            // visited via their own entry in rootCategories instead.
            if (child.catDef.resourceReadoutRoot)
                continue;
            WalkFrom(child, visit);
        }
    }

    private void WalkTree(Action<TreeNode_ThingCategory> visit)
    {
        foreach (ThingCategoryDef thingCat in rootCategories)
        {
            WalkFrom(thingCat.treeNode, visit);
        }
    }

    public void SetAllOpen(bool open)
    {
        WalkTree(node => node.SetOpen(TreeOpenMasks.ResourceReadout, open));
        PersistState();
    }

    public void SetOpenRecursive(TreeNode_ThingCategory node, bool open)
    {
        WalkFrom(node, n => n.SetOpen(TreeOpenMasks.ResourceReadout, open));
        PersistState();
    }

    public void OpenAll() => SetAllOpen(true);

    public void CloseAll() => SetAllOpen(false);

    public Dictionary<string, bool> CaptureState()
    {
        Dictionary<string, bool> state = new Dictionary<string, bool>();
        WalkTree(node => state[node.catDef.defName] = node.IsOpen(TreeOpenMasks.ResourceReadout));
        return state;
    }

    public void ApplyState(Dictionary<string, bool> state)
    {
        WalkTree(node =>
        {
            //bool open = !state.TryGetValue(node.catDef.defName, out bool saved) || saved;
            //node.SetOpen(TreeOpenMasks.ResourceReadout, open);
            if(state.TryGetValue(node.catDef.defName, out bool saved))
                node.SetOpen(TreeOpenMasks.ResourceReadout, saved);
        });
    }

    public void PersistState()
    {
        categoryOpenStates = CaptureState();
        if (!Mod.Settings.persistPerSave)
        {
            Mod.Settings.globalCategoryOpenStates = new Dictionary<string, bool>(categoryOpenStates);
            Mod.Settings.Write();
        }
    }

    public void ApplyFromCurrentMode()
    {
        Dictionary<string, bool> state = Mod.Settings.persistPerSave ? categoryOpenStates : Mod.Settings.globalCategoryOpenStates;
        if (state == null || state.Count == 0)
        {
            //OpenAll();
            return;
        }
        ApplyState(state);
    }
}
