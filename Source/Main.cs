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

    public override void LoadedGame()
    {
        base.LoadedGame();
        OpenAll();
    }

    public override void StartedNewGame()
    {
        base.StartedNewGame();
        LoadedGame();
    }

    private void OpenRecursive(TreeNode_ThingCategory node, int mask)
    {
        // not sure if needed, as all nodes should be openable
        if (!node.Openable)
            return;
        node.SetOpen(mask, true);
        foreach (TreeNode_ThingCategory child in node.ChildCategoryNodes)
        {
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

    public void OpenSubtree(TreeNode_ThingCategory node)
    {
        OpenRecursive(node, TreeOpenMasks.ResourceReadout);
    }

    private void CloseRecursive(TreeNode_ThingCategory node, int mask)
    {
        // not sure if needed, as all nodes should be openable
        if (!node.Openable)
            return;
        node.SetOpen(mask, false);
        foreach (TreeNode_ThingCategory child in node.ChildCategoryNodes)
        {
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

    public void CloseSubtree(TreeNode_ThingCategory node)
    {
        CloseRecursive(node, TreeOpenMasks.ResourceReadout);
    }

    public TreeNode_ThingCategory FindContainingNode(ThingDef thingDef)
    {
        foreach (ThingCategoryDef thingCat in rootCategories)
        {
            TreeNode_ThingCategory found = FindContainingNodeRecursive(thingCat.treeNode, thingDef);
            if (found != null)
                return found;
        }
        return null;
    }

    private TreeNode_ThingCategory FindContainingNodeRecursive(TreeNode_ThingCategory node, ThingDef thingDef)
    {
        if (node.catDef.childThingDefs.Contains(thingDef))
            return node;
        foreach (TreeNode_ThingCategory child in node.ChildCategoryNodes)
        {
            TreeNode_ThingCategory found = FindContainingNodeRecursive(child, thingDef);
            if (found != null)
                return found;
        }
        return null;
    }
}
