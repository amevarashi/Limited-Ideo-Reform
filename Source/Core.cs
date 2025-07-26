using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace IdeoReformLimited
{
	public class Core : Mod
	{
		private static int CurrentStageRerolls => RerollTracker?.CurrentStageRerolls ?? 0;

		public override string SettingsCategory() => "IdeoReformLimited";

		public Core(ModContentPack content) : base(content)
		{
			Settings = GetSettings<LirSettings>();
			SetupFirst();
		}

		public static GameComponent_RerollTracker? RerollTracker { get; set; }
		public static int Seed => Find.FactionManager.OfPlayer.ideos.PrimaryIdeo.development.reformCount + Find.World.ConstantRandSeed + CurrentStageRerolls;

		/// <summary>
		/// This should exist only when reform dialog is open
		/// </summary>
		public static ReformIdeoDialogContext? ReformIdeoDialogContext { get; set; }

		private static LirSettings? Settings;

		public static SettingHandle<int> PointsForTheFirstReform => Settings!.PointsForTheFirstReform;
		public static SettingHandle<int> PointsIncrementPerReform => Settings!.PointsIncrementPerReform;
		public static SettingHandle<int> MaxPointsForReform => Settings!.MaxPointsForReform;
		public static SettingHandle<int> MaxMemeCount => Settings!.MaxMemeCount;
		public static SettingHandle<int> NumberOfMemesToChooseFromOnReform => Settings!.NumberOfMemesToChooseFromOnReform;
		public static SettingHandle<int> NumberOfPreceptsToChooseFromOnReform => Settings!.NumberOfPreceptsToChooseFromOnReform;
		public static SettingHandle<bool> SkipUneditablePrecepts => Settings!.SkipUneditablePrecepts;
		public static SettingHandle<int> MaxRerollsPerReform => Settings!.MaxRerollsPerReform;

		public static void SetupFirst()
		{
			ApplyMaxMemeCount();

			if (Current.ProgramState == ProgramState.Playing)
			{
				foreach (var ideo in Find.IdeoManager.IdeosListForReading)
				{
					ideo.Fluid = true;
				}
			}
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			int maxMemeCount = MaxMemeCount;

			Listing_Standard listing = new();
			listing.Begin(inRect);
			Settings!.PointsForTheFirstReform.DoSetting(listing);
			Settings!.PointsIncrementPerReform.DoSetting(listing);
			Settings!.MaxPointsForReform.DoSetting(listing);
			Settings!.MaxMemeCount.DoSetting(listing);
			Settings!.NumberOfMemesToChooseFromOnReform.DoSetting(listing);
			Settings!.NumberOfPreceptsToChooseFromOnReform.DoSetting(listing);
			Settings!.SkipUneditablePrecepts.DoSetting(listing);
			Settings!.MaxRerollsPerReform.DoSetting(listing);
			listing.End();

			if (maxMemeCount != MaxMemeCount)
			{
				ApplyMaxMemeCount();
			}
		}

		private static void ApplyMaxMemeCount()
		{
			if (Prefs.DevMode) Log.Message("New MaxMemeCount applied");

			AccessTools.StaticFieldRefAccess<IntRange>(AccessTools.Field(typeof(IdeoFoundation), nameof(IdeoFoundation.MemeCountRangeAbsolute)))
				.Invoke() = new IntRange(1, MaxMemeCount);
		}
	}
}