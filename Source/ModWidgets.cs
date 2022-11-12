using UnityEngine;
using Verse;

namespace IdeoReformLimited
{
	public static class ModWidgets
	{
		/// <summary>
		/// Widgets.ButtonText() but shown only if player does not use fluid ideology
		/// </summary>
		public static bool ButtonTextSkipIfFluid(Rect rect, string label, bool drawBackground = true, bool doMouseoverSound = true, bool active = true, TextAnchor? overrideTextAnchor = null)
		{
			if (Find.FactionManager.OfPlayer.ideos?.PrimaryIdeo?.Fluid == true)
			{
				return false;
			}
			return Widgets.ButtonText(rect, label, drawBackground, doMouseoverSound, active, overrideTextAnchor);
		}

		/// <summary>
		/// If player is using fluid ideology, does reroll button instead whatever game have passed
		/// </summary>
		public static bool RerollButton(Rect rect, string label, bool drawBackground = true, bool doMouseoverSound = true, bool active = true, TextAnchor? overrideTextAnchor = null)
		{
			if (Find.FactionManager.OfPlayer.ideos?.PrimaryIdeo?.Fluid != true)
			{
				return Widgets.ButtonText(rect, label, drawBackground, doMouseoverSound, active, overrideTextAnchor);
			}

			int rerollsLeft = Core.val_rerollsPerStage - Core.RerollTracker.CurrentStageRerolls;

			if (Widgets.ButtonText(rect, "LIR_Reroll".Translate(rerollsLeft), drawBackground, doMouseoverSound, rerollsLeft > 0, overrideTextAnchor))
			{
				Core.RerollTracker.NotifyReroll();
			}

			// It is "inside" another button, remember? =)
			return false;
		}
	}
}
