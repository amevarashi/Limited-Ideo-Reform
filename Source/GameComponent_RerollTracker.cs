using RimWorld;
using Verse;

namespace IdeoReformLimited
{
	public class GameComponent_RerollTracker : GameComponent
	{
		private int currentStageRerolls;

		public int CurrentStageRerolls => currentStageRerolls;

		public GameComponent_RerollTracker(Game game)
		{
		}

		public void NotifyReroll()
		{
			currentStageRerolls++;
		}

		public void NotifyIdeoReformed(Ideo ideo)
		{
			currentStageRerolls = 0;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref currentStageRerolls, "currentStageRerolls", 0);
		}
	}
}
