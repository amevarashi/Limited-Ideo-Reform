using Verse;

namespace IdeoReformLimited
{
	public class LirSettings : ModSettings
	{
		public SettingHandle<int> PointsForTheFirstReform { get; private set; }
		public SettingHandle<int> PointsIncrementPerReform { get; private set; }
		public SettingHandle<int> MaxPointsForReform { get; private set; }
		public SettingHandle<int> MaxMemeCount { get; private set; }
		public SettingHandle<int> NumberOfMemesToChooseFromOnReform { get; private set; }
		public SettingHandle<int> NumberOfPreceptsToChooseFromOnReform { get; private set; }
		public SettingHandle<bool> AddRandomPreceptOnReform { get; private set; }
		public SettingHandle<bool> SkipUneditablePrecepts { get; private set; }
		public SettingHandle<int> MaxRerollsPerReform { get; private set; }

		public LirSettings()
		{
			PointsForTheFirstReform = new("needPointReform", "val_needPointReform_t", "val_needPointReform_d", 6, 1);
			PointsIncrementPerReform = new("needPointReformStep", "val_needPointReformStep_t", "val_needPointReformStep_d", 2, 0);
			MaxPointsForReform = new("MaxPointsForReform", "LIR_MaxPointsForReform_t", "LIR_MaxPointsForReform_d", 50, 1);
			MaxMemeCount = new("maxMemeCount", "val_maxMemeCount_t", "val_maxMemeCount_d", 5, 0);
			NumberOfMemesToChooseFromOnReform = new("memeSelectCount", "val_memeSelectCount_t", "val_memeSelectCount_d", 4, 0);
			NumberOfPreceptsToChooseFromOnReform = new("preceptSelectCount", "val_preceptSelectCount_t", "val_preceptSelectCount_d", 3, 0);
			AddRandomPreceptOnReform = new("AddRandomPreceptOnReform", "LIR_AddRandomPreceptOnReform_t", "LIR_AddRandomPreceptOnReform_d", false);
			SkipUneditablePrecepts = new("SkipUneditablePrecepts", "LIR_SkipUneditablePrecepts_t", "LIR_SkipUneditablePrecepts_d", true);
			MaxRerollsPerReform = new("rerollsPerStage", "val_rerollsPerStage_t", "val_rerollsPerStage_d", 1, 0);
		}

		public override void ExposeData()
		{
			PointsForTheFirstReform.Scribe();
			PointsIncrementPerReform.Scribe();
			MaxPointsForReform.Scribe();
			MaxMemeCount.Scribe();
			NumberOfMemesToChooseFromOnReform.Scribe();
			NumberOfPreceptsToChooseFromOnReform.Scribe();
			AddRandomPreceptOnReform.Scribe();
			SkipUneditablePrecepts.Scribe();
			MaxRerollsPerReform.Scribe();
		}
	}
}