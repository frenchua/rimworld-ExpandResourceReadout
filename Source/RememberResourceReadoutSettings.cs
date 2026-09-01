using System.Collections.Generic;
using Verse;

namespace RememberResourceReadout;

public class RememberResourceReadoutSettings : ModSettings
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
