using System.Collections.Generic;
using RimWorld;

namespace IdeoReformLimited
{
    public class ReformIdeoDialogContext
    {
        public readonly List<MemeDef> limitedMemes = new List<MemeDef>();
        public readonly HashSet<IssueDef> limitedPreceptIssues = new HashSet<IssueDef>();
        public MemeCategory CurrentMemeCategory { get; set; }


        public void NotifyReroll()
        {
            limitedMemes.Clear();
            limitedPreceptIssues.Clear();
        }
    }
}