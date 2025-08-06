using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace ExpandResourceReadout {

    public class ExpandResourceReadoutSettings : ModSettings { }

    public class ExpandResourceReadoutComponent : GameComponent {

        private List<ThingCategoryDef> rootCategories;

        public ExpandResourceReadoutComponent(Game g) : base() {
            rootCategories = (from cat in DefDatabase<ThingCategoryDef>.AllDefs
                                   where cat.resourceReadoutRoot
                                   select cat).ToList<ThingCategoryDef>();
        }

        public override void LoadedGame() {
            base.LoadedGame();
            foreach(ThingCategoryDef thingCat in rootCategories) {
                OpenRecursive(thingCat.treeNode, 32);
            }
        }

        public override void StartedNewGame() {
            base.StartedNewGame();
            LoadedGame();
        }

        private void OpenRecursive(TreeNode_ThingCategory node, int mask) {
            // not sure if needed, as all nodes should be openable
            if(!node.Openable)
                return;
            node.SetOpen(mask, true);
            foreach (TreeNode_ThingCategory child in node.ChildCategoryNodes) {
                    OpenRecursive(child, mask);
            }
        }
    }
}
