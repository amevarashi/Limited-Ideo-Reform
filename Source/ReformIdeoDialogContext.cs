using System.Collections.Generic;
using RimWorld;

namespace IdeoReformLimited
{
    public class ReformIdeoDialogContext
    {
        public readonly List<MemeDef> limitedMemes = [];
        public readonly HashSet<IssueDef> limitedPreceptIssues = [];
        public MemeCategory CurrentMemeCategory { get; set; }
        public PreceptDef? RandomAddedPrecept { get; set; }

        public void NotifyReroll()
        {
            limitedMemes.Clear();
            limitedPreceptIssues.Clear();
        }
    }
}