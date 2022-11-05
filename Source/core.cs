using UnityEngine;
using Verse;
using RimWorld;
using HugsLib;
using HugsLib.Settings;
using System.Linq;
using RimWorld.Planet;
using System.Collections.Generic;
using System;
using HarmonyLib;


namespace IdeoReformLimited
{

    public class core : ModBase
    {
        public override string ModIdentifier => "IdeoReformLimited";

		private SettingHandle<int> val_needPointReform_s;
		public static int val_needPointReform;

		private SettingHandle<int> val_needPointReformStep_s;
		public static int val_needPointReformStep;

		private SettingHandle<int> val_maxMemeCount_s;
        public static int val_maxMemeCount;

		private SettingHandle<int> val_memeSelectCount_s;
		public static int val_memeSelectCount;

		private SettingHandle<int> val_preceptSelectCount_s;
		public static int val_preceptSelectCount;

		private SettingHandle<bool> val_getPoint_s;
		public static bool val_getPoint;


		public override void DefsLoaded()
        {
			val_needPointReform_s = Settings.GetHandle<int>("val_needPointReform", "val_needPointReform_t".Translate(), "val_needPointReform_d".Translate(), 6);
			val_needPointReformStep_s = Settings.GetHandle<int>("val_needPointReformStep", "val_needPointReformStep_t".Translate(), "val_needPointReformStep_d".Translate(), 2);
			val_maxMemeCount_s = Settings.GetHandle<int>("val_maxMemeCount", "val_maxMemeCount_t".Translate(), "val_maxMemeCount_d".Translate(), 5);
			val_memeSelectCount_s = Settings.GetHandle<int>("val_memeSelectCount", "val_memeSelectCount_t".Translate(), "val_memeSelectCount_d".Translate(), 4);
			val_preceptSelectCount_s = Settings.GetHandle<int>("val_preceptSelectCount", "val_preceptSelectCount_t".Translate(), "val_preceptSelectCount_d".Translate(), 3);
			val_getPoint_s = Settings.GetHandle<bool>("val_getPoint", "val_getPoint_t".Translate(), "val_getPoint_d".Translate(), false);

			SettingsChanged();
        }

        public override void SettingsChanged()
        {
			val_needPointReform_s.Value = Mathf.Clamp(val_needPointReform_s.Value, 1, 10000);
			val_needPointReform = val_needPointReform_s.Value;

			val_needPointReformStep_s.Value = Mathf.Clamp(val_needPointReformStep_s.Value, 0, 10000);
			val_needPointReformStep = val_needPointReformStep_s.Value;

			val_maxMemeCount_s.Value = Mathf.Clamp(val_maxMemeCount_s.Value, 0, 10000);
            val_maxMemeCount = val_maxMemeCount_s.Value;

			val_memeSelectCount_s.Value = Mathf.Clamp(val_memeSelectCount_s.Value, 0, 10000);
			val_memeSelectCount = val_memeSelectCount_s.Value;

			val_preceptSelectCount_s.Value = Mathf.Clamp(val_preceptSelectCount_s.Value, 0, 10000);
			val_preceptSelectCount = val_preceptSelectCount_s.Value;

			val_getPoint_s.Value = val_getPoint_s.Value;
			val_getPoint = val_getPoint_s.Value;

			setupFirst();
		}

        // -----------------------------------------


        static public int tickGame = 0;

        public override void WorldLoaded()
        {
			setupFirst();
		}
		void setupFirst()
        {
			AccessTools.StaticFieldRefAccess<IntRange>(AccessTools.Field(typeof(IdeoFoundation), "MemeCountRangeAbsolute")).Invoke() = new IntRange(1, val_maxMemeCount);

			if(Current.ProgramState == ProgramState.Playing)
            {
				foreach (var ideo in Find.IdeoManager.IdeosListForReading)
				{
					ideo.Fluid = true;
				}
			}
            

		}



        //테스트용 치트


        public override void Tick(int currentTick)
        {
			if (!val_getPoint) return;

            tickGame = Find.TickManager.TicksGame;

            if (tickGame % 10 == 0)
            {
                Find.FactionManager.OfPlayer.ideos.PrimaryIdeo.development.points++;
            }

        }

        static List<MemeDef> memesInCategory = new List<MemeDef>();
		static List<MemeDef> finalSelectedMemes = new List<MemeDef>();
		static List<MemeDef> memesPlayerHave = new List<MemeDef>();
		static MemeDef meme;
		static bool ReformingFluidIdeo;
		static bool ConfiguringNewFluidIdeo;
		static IntRange MemeCountRangeAbsolute;
		static public int seed => Find.FactionManager.OfPlayer.ideos.PrimaryIdeo.development.reformCount + Find.World.ConstantRandSeed;
		static int n;


		// 가르침 선택지 선정
		static public void DoMemeSelector(Dialog_ChooseMemes __instance, Ideo ideo, Rect viewRect, MemeCategory category, ref float curY)
		{
			ReformingFluidIdeo = (bool)AccessTools.Property(typeof(Dialog_ChooseMemes), "ReformingFluidIdeo").GetValue(__instance);
			ConfiguringNewFluidIdeo = (bool)AccessTools.Property(typeof(Dialog_ChooseMemes), "ConfiguringNewFluidIdeo").GetValue(__instance);
			MemeCountRangeAbsolute = (IntRange)AccessTools.Property(typeof(Dialog_ChooseMemes), "MemeCountRangeAbsolute").GetValue(__instance);
			//List<MemeDef> memesInCategory = (List<MemeDef>)AccessTools.Field(typeof(Dialog_ChooseMemes), "memesInCategory").GetValue(__instance);
			

			curY += 17f;
			string text;
			if (category == MemeCategory.Structure)
			{
				text = "ChooseStructureMemesInfo".Translate();
			}
			else if (ConfiguringNewFluidIdeo)
			{
				text = "ChooseNormalMemesFluidIdeoInfo".Translate(MemeCountRangeAbsolute.min);
			}
			else
			{
				text = ((!ReformingFluidIdeo) ? ((string)"ChooseNormalMemesInfo".Translate(MemeCountRangeAbsolute.min, MemeCountRangeAbsolute.max)) : ((string)"ChooseOrRemoveMeme".Translate()));
				text += " " + "SomeMemesHaveMoreImpact".Translate();
			}
			Widgets.Label(viewRect.x, ref curY, viewRect.width, text);
			curY += 27f;

			// clear
			memesInCategory.Clear();
			finalSelectedMemes.Clear();
			memesPlayerHave.Clear();

			foreach (MemeDef item in DefDatabase<MemeDef>.AllDefsListForReading)
			{
				bool CanUseMeme = (bool)AccessTools.Method(typeof(Dialog_ChooseMemes), "CanUseMeme").Invoke(__instance, new object[] { item });
				if (item.category == category && CanUseMeme)
				{
					if (ideo.memes.Contains(item))
					{
						memesPlayerHave.Add(item);
                    }
                    else
                    {
						memesInCategory.Add(item);
					}
				}
			}
			
			// 가르침 선택지
			if(memesPlayerHave.Count >= val_maxMemeCount)
            {
				for (int i = 0; i < Mathf.Max(2, Mathf.FloorToInt(val_maxMemeCount * 0.25f)); i++)
				{
					meme = memesPlayerHave[Rand.RangeSeeded(0, memesPlayerHave.Count, seed)];
					memesPlayerHave.Remove(meme);
					finalSelectedMemes.Add(meme);
					if (memesPlayerHave.Count == 0) break;
				}
			}
            else
            {
				float removeCount = Mathf.Min(memesPlayerHave.Count * 0.25f, val_memeSelectCount * 0.25f);

				for (int i = 0; i < val_memeSelectCount; i++)
				{
					if(removeCount >= 1f)
                    {
						meme = memesPlayerHave[Rand.RangeSeeded(0, memesPlayerHave.Count, seed)];
						memesPlayerHave.Remove(meme);
						removeCount -= 1f;
					}
					else if (removeCount > 0f && Rand.ChanceSeeded(removeCount, seed))
                    {
						meme = memesPlayerHave[Rand.RangeSeeded(0, memesPlayerHave.Count, seed)];
						memesPlayerHave.Remove(meme);
						removeCount = 0f;
					}
                    else
                    {
						meme = memesInCategory[Rand.RangeSeeded(0, memesInCategory.Count, seed)];
						memesInCategory.Remove(meme);
					}
					
					finalSelectedMemes.Add(meme);
					if (memesInCategory.Count == 0) break;
				}
			}
			

			if (category == MemeCategory.Structure)
			{
				//DoStructureMemeSelector(viewRect, ref curY, memesInCategory);
				AccessTools.Method(typeof(Dialog_ChooseMemes), "DoStructureMemeSelector").Invoke(__instance, new object[] { viewRect, curY, finalSelectedMemes });
			}
			else
			{
				//DoNormalMemeSelector(viewRect, ref curY, memesInCategory);
				AccessTools.Method(typeof(Dialog_ChooseMemes), "DoNormalMemeSelector").Invoke(__instance, new object[] { viewRect, curY, finalSelectedMemes });
			}
		}


		public static bool CanAddRitualPattern(Ideo ideo, RitualPatternDef pattern, IdeoEditMode editMode)
		{
			if (editMode == IdeoEditMode.Dev)
			{
				return true;
			}
			if (!pattern.CanFactionUse(RimWorld.IdeoUIUtility.FactionForRandomization(ideo)))
			{
				return false;
			}
			return true;
		}





	}



    






}