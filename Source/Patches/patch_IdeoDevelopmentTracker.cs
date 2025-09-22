using HarmonyLib;
using RimWorld;
using System;
using Verse;

namespace IdeoReformLimited.Patches
{
	[HarmonyPatch(typeof(IdeoDevelopmentTracker))]
	public static class Patch_IdeoDevelopmentTracker
	{
		/// <summary>
		/// Replace original getter with our implementation
		/// </summary>
		/// <param name="__instance">Harmony passes the instance here</param>
		/// <param name="__result">Harmony passes the return result of original method here</param>
		/// <returns>Run original implementation</returns>
		[HarmonyPatch(nameof(IdeoDevelopmentTracker.NextReformationDevelopmentPoints), MethodType.Getter)]
		[HarmonyPrefix]
		public static bool RedefineNextReformationDevelopmentPoints(IdeoDevelopmentTracker __instance, ref int __result)
		{
			__result = Math.Min(
				Core.PointsForTheFirstReform + (__instance.reformCount * Core.PointsIncrementPerReform),
				Core.MaxPointsForReform);
			return false;
		}

		/// <summary>
		/// Pass IdeoReformed event
		/// </summary>
		/// <param name="__instance">Harmony passes the instance here</param>
		[HarmonyPatch(nameof(IdeoDevelopmentTracker.Notify_Reformed))]
		[HarmonyPostfix]
		public static void NotifyIdeoReformed(IdeoDevelopmentTracker __instance)
		{
			Core.RerollTracker?.NotifyIdeoReformed(__instance.ideo);
			if (Core.ReformIdeoDialogContext?.RandomAddedPrecept is PreceptDef addedDef)
			{
				TaggedString letterText = "LIR_AddedPreceptText".Translate(
					addedDef.issue.LabelCap.Named("ISSUE"),
					addedDef.LabelCap.Named("PRECEPT"),
					__instance.ideo.name.Named("IDEOLOGY"));
				Letter letter = LetterMaker.MakeLetter("LIR_AddedPreceptLabel".Translate(), letterText, LetterDefOf.NeutralEvent);
				Find.LetterStack.ReceiveLetter(letter);
			}
			Core.ReformIdeoDialogContext = null;
		}
	}
}
