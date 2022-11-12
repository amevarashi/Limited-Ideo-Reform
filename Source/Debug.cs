using RimWorld;
using Verse;

namespace IdeoReformLimited
{
	public static class Debug
	{
		[DebugAction("Limited Ideo Reform", "Give reform points", requiresIdeology = true, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		public static void GenerateReformPoints()
		{
			if (Find.FactionManager.OfPlayer.ideos?.PrimaryIdeo?.Fluid != true)
			{
				Messages.Message("Not a fluid ideology", MessageTypeDefOf.NegativeEvent, false);
				return;
			}
			int newDevPoints = Find.FactionManager.OfPlayer.ideos.PrimaryIdeo.development.NextReformationDevelopmentPoints;
			Find.FactionManager.OfPlayer.ideos.PrimaryIdeo.development.points = newDevPoints;
			Messages.Message($"Set reform points to {newDevPoints}", MessageTypeDefOf.NeutralEvent, false);
		}
	}
}