using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace ExpandResourceReadout;

public class ExpandResourceReadoutSettings : ModSettings { }

public class ExpandResourceReadoutComponent : GameComponent
{

    private List<ThingCategoryDef> rootCategories;

    public ExpandResourceReadoutComponent(Game g) : base()
    {
        rootCategories = (from cat in DefDatabase<ThingCategoryDef>.AllDefs
                          where cat.resourceReadoutRoot
                          select cat).ToList<ThingCategoryDef>();
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
    }
}
