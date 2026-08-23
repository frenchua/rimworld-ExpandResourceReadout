using System;
using System.Collections.Generic;
using System.Linq;
using Verse;


namespace ExpandResourceReadout;

public class ExpandResourceReadoutSettings : ModSettings
{
    public bool persistPerSave = false;
    public Dictionary<string, bool> globalCategoryOpenStates = new Dictionary<string, bool>();

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref persistPerSave, "persistPerSave", false);
        Scribe_Collections.Look(ref globalCategoryOpenStates, "globalCategoryOpenStates", LookMode.Value, LookMode.Value);
        globalCategoryOpenStates ??= new Dictionary<string, bool>();
    }
}

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

    public void OpenRecursive(TreeNode_ThingCategory node, int mask)
    {
        // not sure if needed, as all nodes should be openable
        if (!node.Openable)
            return;
        node.SetOpen(mask, true);
        foreach (TreeNode_ThingCategory child in node.ChildCategoryNodes)
        {
            // A child can be resourceReadoutRoot even though it's structurally nested
            // under this node in the def graph (e.g. Plant Matter's parent is Raw
            // Resources, but both are independently resourceReadoutRoot) - the readout
            // draws such children as their own separate top-level rows, not nested
            // under this one, so don't cascade into them. Vanilla's own
            // DoCategoryChildren applies this same guard when drawing.
            if (child.catDef.resourceReadoutRoot)
                continue;
            OpenRecursive(child, mask);
        }
    }

    public void OpenAll()
    {
        foreach (ThingCategoryDef thingCat in rootCategories)
        {
            OpenRecursive(thingCat.treeNode, TreeOpenMasks.ResourceReadout);
        }
        PersistState();
    }

    public void CloseRecursive(TreeNode_ThingCategory node, int mask)
    {
        // not sure if needed, as all nodes should be openable
        if (!node.Openable)
            return;
        node.SetOpen(mask, false);
        foreach (TreeNode_ThingCategory child in node.ChildCategoryNodes)
        {
            // See matching comment in OpenRecursive.
            if (child.catDef.resourceReadoutRoot)
                continue;
            CloseRecursive(child, mask);
        }
    }
    public void CloseAll()
    {
        foreach (ThingCategoryDef thingCat in rootCategories)
        {
            CloseRecursive(thingCat.treeNode, TreeOpenMasks.ResourceReadout);
        }
        PersistState();
    }

    private void WalkTree(Action<TreeNode_ThingCategory> visit)
    {
        void Recurse(TreeNode_ThingCategory node)
        {
            visit(node);
            foreach (TreeNode_ThingCategory child in node.ChildCategoryNodes)
            {
                // See matching comment in OpenRecursive - roots nested under another
                // root are visited via their own entry in rootCategories instead.
                if (child.catDef.resourceReadoutRoot)
                    continue;
                Recurse(child);
            }
        }

        foreach (ThingCategoryDef thingCat in rootCategories)
        {
            Recurse(thingCat.treeNode);
        }
    }

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
            bool open = !state.TryGetValue(node.catDef.defName, out bool saved) || saved;
            node.SetOpen(TreeOpenMasks.ResourceReadout, open);
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
            OpenAll();
            return;
        }
        ApplyState(state);
    }
}
