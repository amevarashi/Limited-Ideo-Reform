using HarmonyLib;
using HugsLib;
using HugsLib.Settings;
using RimWorld;
using Verse;

namespace IdeoReformLimited
{
	public class Core : ModBase
	{
		private static int CurrentStageRerolls => RerollTracker?.CurrentStageRerolls ?? 0;

		public override string ModIdentifier => "IdeoReformLimited";

		public static GameComponent_RerollTracker RerollTracker { get; private set; }
		public static int Seed => Find.FactionManager.OfPlayer.ideos.PrimaryIdeo.development.reformCount + Find.World.ConstantRandSeed + CurrentStageRerolls;

		public static SettingHandle<int> val_needPointReform { get; private set; }
		public static SettingHandle<int> val_needPointReformStep { get; private set; }
		public static SettingHandle<int> val_maxMemeCount { get; private set; }
		public static SettingHandle<int> val_memeSelectCount { get; private set; }
		public static SettingHandle<int> val_preceptSelectCount { get; private set; }
		public static SettingHandle<int> val_rerollsPerStage { get; private set; }

		public override void DefsLoaded()
		{
			val_needPointReform = Settings.GetHandle("val_needPointReform", "val_needPointReform_t".Translate(), "val_needPointReform_d".Translate(), 6, Validators.IntRangeValidator(1, 10000));
			val_needPointReformStep = Settings.GetHandle("val_needPointReformStep", "val_needPointReformStep_t".Translate(), "val_needPointReformStep_d".Translate(), 2, Validators.IntRangeValidator(0, 10000));
			val_maxMemeCount = Settings.GetHandle("val_maxMemeCount", "val_maxMemeCount_t".Translate(), "val_maxMemeCount_d".Translate(), 5, Validators.IntRangeValidator(0, 10000));
			val_memeSelectCount = Settings.GetHandle("val_memeSelectCount", "val_memeSelectCount_t".Translate(), "val_memeSelectCount_d".Translate(), 4, Validators.IntRangeValidator(0, 10000));
			val_preceptSelectCount = Settings.GetHandle("val_preceptSelectCount", "val_preceptSelectCount_t".Translate(), "val_preceptSelectCount_d".Translate(), 3, Validators.IntRangeValidator(0, 10000));
			val_rerollsPerStage = Settings.GetHandle("val_rerollsPerStage", "val_rerollsPerStage_t".Translate(), "val_rerollsPerStage_d".Translate(), 1, Validators.IntRangeValidator(0, 10000));
			SetupFirst();
		}

		public override void SettingsChanged()
		{
			SetupFirst();
		}

		public override void WorldLoaded()
		{
			SetupFirst();
			RerollTracker = Current.Game.GetComponent<GameComponent_RerollTracker>();
		}

		void SetupFirst()
		{
			AccessTools.StaticFieldRefAccess<IntRange>(AccessTools.Field(typeof(IdeoFoundation), "MemeCountRangeAbsolute")).Invoke() = new IntRange(1, val_maxMemeCount);

			if (Current.ProgramState == ProgramState.Playing)
			{
				foreach (var ideo in Find.IdeoManager.IdeosListForReading)
				{
					ideo.Fluid = true;
				}
			}
		}
	}
}