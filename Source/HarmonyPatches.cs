using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.Sound;
using System.Linq;

namespace IdeoReformLimited
{


	public class HarmonyPatches : Mod
	{

		public HarmonyPatches(ModContentPack content) : base(content)
		{
			var harmony = new Harmony("com.yayo.ideoReformLimited");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

		}

	}

	[HarmonyPatch(typeof(IdeoDevelopmentTracker))]
	[HarmonyPatch("NextReformationDevelopmentPoints", MethodType.Getter)]
	internal class patch_IdeoDevelopmentTracker_NextReformationDevelopmentPoints
	{
		

		[HarmonyPrefix]
		private static bool Prefix(IdeoDevelopmentTracker __instance, ref int __result)
		{

			__result = core.val_needPointReform + __instance.reformCount * core.val_needPointReformStep;
			return false;
		}
	}




	// 가르침

	[HarmonyPatch(typeof(Dialog_ChooseMemes))]
	[HarmonyPatch("DoWindowContents")]
	internal class patch_Dialog_ChooseMemes_DoWindowContents
	{
		[HarmonyPrefix]
		private static bool Prefix(Dialog_ChooseMemes __instance, Rect rect, ref List<MemeDef> ___newMemes, Ideo ___ideo, bool ___initialSelection, ref Vector2 ___scrollPos, float ___viewHeight, Vector2 ___ButSize, MemeCategory ___memeCategory)
		{
			bool ReformingFluidIdeo = (bool)AccessTools.Property(typeof(Dialog_ChooseMemes), "ReformingFluidIdeo").GetValue(__instance);
			if (!ReformingFluidIdeo) return true;

			bool ConfiguringNewFluidIdeo = (bool)AccessTools.Property(typeof(Dialog_ChooseMemes), "ConfiguringNewFluidIdeo").GetValue(__instance);
			IntRange MemeCountRangeAbsolute = (IntRange)AccessTools.Property(typeof(Dialog_ChooseMemes), "MemeCountRangeAbsolute").GetValue(__instance);

			Rect outRect = rect;
			outRect.height -= ___ButSize.y;
			string label = ((___memeCategory == MemeCategory.Structure) ? ((string)"ChooseStructure".Translate()) : ((!ConfiguringNewFluidIdeo) ? ((string)"ChooseMemes".Translate()) : ((string)"ChooseStartingMeme".Translate())));
			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(outRect.x, outRect.y, rect.width, 30f), label);
			Text.Font = GameFont.Small;
			outRect.yMin += 30f;
			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, ___viewHeight);
			Widgets.BeginScrollView(outRect, ref ___scrollPos, viewRect);
			float curY = 0f;
			if (___memeCategory == MemeCategory.Structure)
			{
				//DoMemeSelector(viewRect, MemeCategory.Structure, ref curY);
				//AccessTools.Method(typeof(Dialog_ChooseMemes), "DoMemeSelector").Invoke(__instance, new object[] { viewRect, MemeCategory.Structure, curY });
				core.DoMemeSelector(__instance, ___ideo, viewRect, MemeCategory.Structure, ref curY);
			}
			else if (___memeCategory == MemeCategory.Normal)
			{
				//DoMemeSelector(viewRect, MemeCategory.Normal, ref curY);
				//AccessTools.Method(typeof(Dialog_ChooseMemes), "DoMemeSelector").Invoke(__instance, new object[] { viewRect, MemeCategory.Normal, curY });
				core.DoMemeSelector(__instance, ___ideo, viewRect, MemeCategory.Normal, ref curY);
			}
			___viewHeight = Mathf.Max(___viewHeight, curY);
			Widgets.EndScrollView();
			if (Widgets.ButtonText(new Rect(0f, rect.height - ___ButSize.y, ___ButSize.x, ___ButSize.y), "Back".Translate()))
			{
				__instance.Close();
				if (___memeCategory == MemeCategory.Structure)
				{
					//NotifyConfigureIdeoPage();
					AccessTools.Method(typeof(Dialog_ChooseMemes), "NotifyConfigureIdeoPage").Invoke(__instance, new object[] { });
				}
				else if (___memeCategory == MemeCategory.Normal)
				{
					if (___initialSelection)
					{
						Find.WindowStack.Add(new Dialog_ChooseMemes(___ideo, MemeCategory.Structure, ___initialSelection));
					}
					else
					{
						//NotifyConfigureIdeoPage();
						AccessTools.Method(typeof(Dialog_ChooseMemes), "NotifyConfigureIdeoPage").Invoke(__instance, new object[] { });
					}
				}
			}
			//if (Widgets.ButtonText(new Rect((int)(rect.width - ___ButSize.x) / 2, rect.height - ___ButSize.y, ___ButSize.x, ___ButSize.y), "Randomize".Translate()))
			//{
			//    SoundDefOf.Tick_High.PlayOneShotOnCamera();
			//    FactionDef forFaction = IdeoUIUtility.FactionForRandomization(___ideo);
			//    if (___memeCategory == MemeCategory.Normal)
			//    {
			//        if (ReformingFluidIdeo)
			//        {
			//            ___newMemes = IdeoUtility.RandomizeNormalMemesForReforming(MemeCountRangeAbsolute.max, ___ideo.memes, forFaction);
			//        }
			//        else
			//        {
			//            int RandomGeneratedMemeCount = (int)AccessTools.Method(typeof(Dialog_ChooseMemes), "RandomGeneratedMemeCount").Invoke(__instance, new object[] {});
			//            ___newMemes = IdeoUtility.RandomizeNormalMemes(RandomGeneratedMemeCount, ___newMemes, forFaction, ConfiguringNewFluidIdeo);
			//        }
			//    }
			//    else if (___memeCategory == MemeCategory.Structure)
			//    {
			//        ___newMemes = IdeoUtility.RandomizeStructureMeme(___newMemes, forFaction);
			//    }
			//}
			Rect rect2 = new Rect(rect.width - ___ButSize.x, rect.height - ___ButSize.y, ___ButSize.x, ___ButSize.y);
			if (Widgets.ButtonText(rect2, "DoneButton".Translate()))
			{
				//TryAccept();
				AccessTools.Method(typeof(Dialog_ChooseMemes), "TryAccept").Invoke(__instance, new object[] { });
			}
			string text = null;
			//Pair<MemeDef, MemeDef> firstIncompatibleMemePair = GetFirstIncompatibleMemePair();
			Pair<MemeDef, MemeDef> firstIncompatibleMemePair = (Pair<MemeDef, MemeDef>)AccessTools.Method(typeof(Dialog_ChooseMemes), "GetFirstIncompatibleMemePair").Invoke(__instance, new object[] { });
			int GetMemeCount1 = (int)AccessTools.Method(typeof(Dialog_ChooseMemes), "GetMemeCount").Invoke(__instance, new object[] { MemeCategory.Structure });
			int GetMemeCount2 = (int)AccessTools.Method(typeof(Dialog_ChooseMemes), "GetMemeCount").Invoke(__instance, new object[] { MemeCategory.Normal });
			if (GetMemeCount1 < 1 || (___memeCategory == MemeCategory.Normal && (GetMemeCount2 < MemeCountRangeAbsolute.min || GetMemeCount2 > MemeCountRangeAbsolute.max)) || firstIncompatibleMemePair != default(Pair<MemeDef, MemeDef>))
			{
				Text.Font = GameFont.Tiny;
				Text.Anchor = TextAnchor.MiddleRight;
				GUI.color = Color.red;
				text = ((firstIncompatibleMemePair != default(Pair<MemeDef, MemeDef>)) ? ((string)"IncompatibleMemes".Translate(firstIncompatibleMemePair.First, firstIncompatibleMemePair.Second).CapitalizeFirst()) : ((___memeCategory != 0) ? ((string)"ChooseStructureMeme".Translate()) : ((GetMemeCount2 >= MemeCountRangeAbsolute.min) ? ((string)"TooManyMemes".Translate(MemeCountRangeAbsolute.max)) : ((string)(ConfiguringNewFluidIdeo ? "NotEnoughMemesFluidIdeo".Translate(MemeCountRangeAbsolute.min) : "NotEnoughMemes".Translate(MemeCountRangeAbsolute.min))))));
			}
			Rect rect3 = new Rect(rect.xMax - ___ButSize.x - 240f - 6f, rect2.y, 240f, ___ButSize.y);
			if (text != null)
			{
				Widgets.Label(rect3, text);
				GUI.color = Color.white;
				Text.Anchor = TextAnchor.UpperLeft;
			}
			else if (___memeCategory == MemeCategory.Normal)
			{
				IdeoUIUtility.DrawImpactInfo(rect3, ___newMemes);
			}
			return false;
		}
	}





	// 규율

	[HarmonyPatch(typeof(IdeoUIUtility))]
	[HarmonyPatch("DoPreceptsInt")]
	internal class patch_IdeoUIUtility_DoPreceptsInt
	{
		//      private static List<Precept> tmpPrecepts => AccessTools.StaticFieldRefAccess<List<Precept>>(AccessTools.Field(typeof(IdeoUIUtility), "tmpPrecepts")).Invoke();
		//private static List<ThingDef> tmpUsedThingDefs => AccessTools.StaticFieldRefAccess<List<ThingDef>>(AccessTools.Field(typeof(IdeoUIUtility), "tmpUsedThingDefs")).Invoke();

		static List<Precept> ar_tmp = new List<Precept>();
		static Precept tmp;
		static int count;

		[HarmonyPrefix]
		private static bool Prefix(ref PreceptDef ___tmpMouseOverPrecept, ref MemeDef ___tmpMouseOverMeme, ref List<ThingDef> ___tmpAllThingDefs, ref List<ThingDef> ___tmpAllowedThingDefs, ref List<RitualPatternDef> ___addedPatternDefsTmp, ref Vector2 ___AddPreceptButtonSize, ref List<ThingDef> ___tmpUsedThingDefs, ref List<Precept> ___tmpPrecepts, string categoryLabel, string addPreceptLabel, bool mainPrecepts, Ideo ideo, IdeoEditMode editMode, ref float curY, float width, Func<PreceptDef, bool> filter, bool sortFloatMenuOptionsByLabel = false)
		{
			if (editMode != IdeoEditMode.Reform || !mainPrecepts) return true;

			___tmpPrecepts.Clear();
			ar_tmp.Clear();
			List<Precept> preceptsListForReading = ideo.PreceptsListForReading;

			for (int i = 0; i < preceptsListForReading.Count; i++)
			{
                if (preceptsListForReading[i].def.visible && filter(preceptsListForReading[i].def))
                {
					ar_tmp.Add(preceptsListForReading[i]);
				}
			}
			ar_tmp = ar_tmp.OrderBy(a => a.def.defName).ToList();


			count = Mathf.Min(ar_tmp.Count, core.val_preceptSelectCount);


			for (int i = 0; i < count; i++)
			{
				tmp = ar_tmp[Rand.RangeSeeded(0, ar_tmp.Count, core.seed)];
				___tmpPrecepts.Add(tmp);
				ar_tmp.Remove(tmp);
				if (ar_tmp.Count == 0) break;
			}


			if (!mainPrecepts && preceptsListForReading.Count == 0)
			{
				return false;
			}
			___tmpUsedThingDefs.Clear();
			foreach (Precept item in ideo.PreceptsListForReading)
			{
				Precept_ThingDef precept_ThingDef;
				if ((precept_ThingDef = item as Precept_ThingDef) != null)
				{
					___tmpUsedThingDefs.Add(precept_ThingDef.ThingDef);
				}
			}
			curY += 4f;
			float num = (width - IdeoUIUtility.PreceptBoxSize.x * 3f - 16f) / 2f;
			Widgets.Label(num, ref curY, width, categoryLabel);

			List<FloatMenuOption> opts;
			AcceptanceReport acceptance;
			if (editMode != 0)
			{
				float num2 = width - (width - IdeoUIUtility.PreceptBoxSize.x * 3f - 16f) / 2f;
				Rect rect = new Rect(num2 - ___AddPreceptButtonSize.x, curY - ___AddPreceptButtonSize.y, ___AddPreceptButtonSize.x, ___AddPreceptButtonSize.y);
				if (!mainPrecepts && Widgets.ButtonText(rect, "AddPrecept".Translate(addPreceptLabel).CapitalizeFirst() + "...") && IdeoUIUtility.TutorAllowsInteraction(editMode))
				{
					opts = new List<FloatMenuOption>();
					List<PreceptDef> list = DefDatabase<PreceptDef>.AllDefs.Where((PreceptDef x) => filter(x)).ToList();
					bool flag = list.Any((PreceptDef p) => p.preceptClass == typeof(Precept_Ritual) && p.visible && (bool)IdeoUIUtility.CanListPrecept(ideo, p, editMode));
					int num3 = ideo.PreceptsListForReading.Count((Precept p) => p is Precept_Ritual && p.def.visible);
					___addedPatternDefsTmp.Clear();
					foreach (PreceptDef p2 in list)
					{
						acceptance = IdeoUIUtility.CanListPrecept(ideo, p2, editMode);
						if (!acceptance && string.IsNullOrWhiteSpace(acceptance.Reason))
						{
							continue;
						}
						int preceptCountOfDef = ideo.GetPreceptCountOfDef(p2);
						int num4 = p2.maxCount;
						if (p2.preceptInstanceCountCurve != null)
						{
							num4 = Mathf.Max(num4, Mathf.RoundToInt(p2.preceptInstanceCountCurve.Last().y));
						}
						if (preceptCountOfDef < num4 || p2.ignoreLimitsInEditMode)
						{
							if (!p2.useChoicesFromBuildingDefs || p2.Worker.ThingDefsForIdeo(ideo, null).EnumerableNullOrEmpty())
							{
								if (p2.preceptClass == typeof(Precept_Weapon))
								{
									AddWeaponPreceptOption(p2);
								}
								else
								{
									AccessTools.Method(typeof(IdeoUIUtility), "AddPreceptOption").Invoke(null, new object[] { ideo, p2, editMode, opts, null });
								}
							}
							else
							{
								___tmpAllowedThingDefs.Clear();
								___tmpAllThingDefs.Clear();
								___tmpAllowedThingDefs.AddRange(from td in p2.Worker.ThingDefsForIdeo(ideo, null)
																select td.def);
								___tmpAllThingDefs.AddRange(from bd in (from bDef in p2.Worker.ThingDefs
																		orderby p2.Worker.GetThingOrder(bDef), bDef.chance descending
																		select bDef).ThenBy((Func<PreceptThingChance, string>)((PreceptThingChance bDef) => bDef.def.LabelCap))
															select bd.def);
								if (p2.preceptClass == typeof(Precept_Building))
								{
									foreach (MemeDef meme in ideo.memes)
									{
										if (meme.consumableBuildings.NullOrEmpty())
										{
											continue;
										}
										foreach (ThingDef consumableBuilding in meme.consumableBuildings)
										{
											if (!___tmpAllowedThingDefs.Contains(consumableBuilding))
											{
												___tmpAllowedThingDefs.Add(consumableBuilding);
											}
											if (!___tmpAllThingDefs.Contains(consumableBuilding))
											{
												___tmpAllThingDefs.Add(consumableBuilding);
											}
										}
									}
								}
								foreach (ThingDef b in ___tmpAllThingDefs)
								{
									TaggedString labelCap = b.LabelCap;
									if (p2.preceptClass == typeof(Precept_Apparel))
									{
										labelCap += ": " + p2.LabelCap;
									}
									FloatMenuOption floatMenuOption = null;
									if ((!p2.canUseAlreadyUsedThingDef && ___tmpUsedThingDefs.Contains(b)) || p2.Worker.ShouldSkipThing(ideo, b) || !___tmpAllowedThingDefs.Contains(b))
									{
										if (!p2.canUseAlreadyUsedThingDef && ___tmpUsedThingDefs.Contains(b))
										{
											floatMenuOption = null;
										}
										else
										{
											AcceptanceReport acceptanceReport = p2.Worker.CanUse(b, ideo, null);
											if (!acceptanceReport)
											{
												floatMenuOption = new FloatMenuOption(string.IsNullOrWhiteSpace(acceptanceReport.Reason) ? labelCap : (labelCap + " (" + acceptanceReport.Reason + ")"), null, b);
												floatMenuOption.thingStyle = ideo.GetStyleFor(b);
											}
										}
									}
									else
									{
										floatMenuOption = new FloatMenuOption(labelCap, delegate
										{
											Precept precept = PreceptMaker.MakePrecept(p2);
											Precept_Apparel precept_Apparel;
											if ((precept_Apparel = precept as Precept_Apparel) != null)
											{
												precept_Apparel.apparelDef = b;
											}
											ideo.AddPrecept(precept, init: true);
											Precept_ThingDef precept_ThingDef2;
											if ((precept_ThingDef2 = precept as Precept_ThingDef) != null)
											{
												precept_ThingDef2.ThingDef = b;
												precept_ThingDef2.RegenerateName();
											}
										}, b);
										floatMenuOption.thingStyle = ideo.GetStyleFor(b);
										if (p2.preceptClass == typeof(Precept_Apparel))
										{
											floatMenuOption.forceThingColor = ideo.ApparelColor;
										}
									}
									if (floatMenuOption != null)
									{
										opts.Add(PostProcessOption(floatMenuOption));
									}
								}
							}
							string groupTag = p2.ritualPatternBase?.patternGroupTag;
							if (groupTag.NullOrEmpty())
							{
								continue;
							}
							foreach (RitualPatternDef item2 in DefDatabase<RitualPatternDef>.AllDefs.Where((RitualPatternDef d) => d.patternGroupTag == groupTag && d != p2.ritualPatternBase))
							{
								AccessTools.Method(typeof(IdeoUIUtility), "AddPreceptOption").Invoke(null, new object[] { ideo, p2, editMode, opts, item2 });
							}
						}
						else if (p2.preceptClass == typeof(Precept_Relic))
						{
							opts.Add(new FloatMenuOption("MaxRelicCount".Translate(num4), null));
						}
						else if (preceptCountOfDef >= num4 && p2.issue.allowMultiplePrecepts)
						{
							opts.Add(new FloatMenuOption(p2.LabelCap + " (" + "MaxPreceptCount".Translate(num4) + ")", null, p2.Icon ?? ideo.Icon, ideo.Color));
						}

					}
					if (sortFloatMenuOptionsByLabel)
					{
						opts.SortBy((FloatMenuOption x) => x.Label);
					}
					if (num3 < 6)
					{
						foreach (MemeDef meme2 in ideo.memes)
						{
							if (meme2.replacementPatterns.NullOrEmpty())
							{
								continue;
							}
							foreach (PreceptDef item3 in DefDatabase<PreceptDef>.AllDefs.Where(filter))
							{
								if (item3.ritualPatternBase == null || item3.ritualPatternBase.tags.NullOrEmpty() || !meme2.replaceRitualsWithTags.Any(item3.ritualPatternBase.tags.Contains))
								{
									continue;
								}
								foreach (RitualPatternDef replacementPattern in meme2.replacementPatterns)
								{
									if (core.CanAddRitualPattern(ideo, replacementPattern, editMode))
									{
										AccessTools.Method(typeof(IdeoUIUtility), "AddPreceptOption").Invoke(null, new object[] { ideo, item3, editMode, opts, replacementPattern });
									}
								}
							}
						}
					}
					else if (flag)
					{
						opts.Clear();
						opts.Add(new FloatMenuOption("MaxRitualCount".Translate(6), null));
					}
					if (!opts.Any())
					{
						opts.Add(new FloatMenuOption("NoChoicesAvailable".Translate(), null));
					}
					Find.WindowStack.Add(new FloatMenu(opts));
				}
				//if (mainPrecepts)
				//{
				//	Rect rect2 = rect;
				//	rect2.x = rect.xMin - rect.width - 10f;
				//	if (Widgets.ButtonText(rect2, "RandomizePrecepts".Translate()) && IdeoUIUtility.TutorAllowsInteraction(editMode))
				//	{
				//		Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("ChangesRandomizePrecepts".Translate("RandomizePrecepts".Translate()), delegate
				//		{
				//			ideo.foundation.RandomizePrecepts(init: true, new IdeoGenerationParms(IdeoUIUtility.FactionForRandomization(ideo)));
				//			ideo.RegenerateDescription();
				//			SoundDefOf.Tick_High.PlayOneShotOnCamera();
				//		}));
				//	}
				//}
			}
			curY += 4f;
			PreceptImpact preceptImpact = (___tmpPrecepts.Any() ? ___tmpPrecepts[0].def.impact : PreceptImpact.Low);
			float num5 = (width - 3f * IdeoUIUtility.PreceptBoxSize.x - 16f) / 2f;
			int num6 = 0;
			int num7 = 0;
			if (!___tmpPrecepts.Any())
			{
				GUI.color = Color.grey;
				Widgets.Label(new Rect(num + 36f, curY + 10f, 999f, Text.LineHeight), "(" + "NoneLower".Translate() + ")");
				GUI.color = Color.white;
			}
			for (int j = 0; j < ___tmpPrecepts.Count; j++)
			{
				if (preceptImpact != ___tmpPrecepts[j].def.impact)
				{
					preceptImpact = ___tmpPrecepts[j].def.impact;
					num7 = 0;
					num6++;
				}
				else if (num7 >= 2)
				{
					num7 = 0;
					num6++;
				}
				else if (j > 0)
				{
					num7++;
				}
				Rect rect3 = new Rect(num5 + (float)num7 * IdeoUIUtility.PreceptBoxSize.x + (float)(num7 * 8), curY + (float)num6 * IdeoUIUtility.PreceptBoxSize.y + (float)(num6 * 8), IdeoUIUtility.PreceptBoxSize.x, IdeoUIUtility.PreceptBoxSize.y);
				if (mainPrecepts && editMode == IdeoEditMode.GameStart)
				{
					UIHighlighter.HighlightOpportunity(rect3, "PreceptBox");
				}
				___tmpPrecepts[j].DrawPreceptBox(rect3, editMode, ___tmpMouseOverMeme != null && IdeoUIUtility.IsPreceptRelatedForUI(___tmpMouseOverMeme, ___tmpPrecepts[j].def));
				if (Mouse.IsOver(rect3))
				{
					___tmpMouseOverPrecept = ___tmpPrecepts[j].def;
				}
				GUI.color = Color.white;
			}
			curY += (float)(num6 + 1) * IdeoUIUtility.PreceptBoxSize.y + (float)(num6 * 8);
			curY += 17f;
			___tmpPrecepts.Clear();
			void AddWeaponPreceptOption(PreceptDef pr)
			{
				foreach (WeaponClassPairDef w in DefDatabase<WeaponClassPairDef>.AllDefs)
				{
					Precept_Weapon precept_Weapon2;
					if (!ideo.PreceptsListForReading.Any((Precept x) => (precept_Weapon2 = x as Precept_Weapon) != null && ((precept_Weapon2.noble == w.first && precept_Weapon2.despised == w.second) || (precept_Weapon2.noble == w.second && precept_Weapon2.despised == w.first))))
					{
						_ = w;
						opts.Add(PostProcessOption(new FloatMenuOption(w.first.LabelCap + " / " + w.second.LabelCap, delegate
						{
							Precept_Weapon precept_Weapon = (Precept_Weapon)PreceptMaker.MakePrecept(pr);
							precept_Weapon.noble = w.first;
							precept_Weapon.despised = w.second;
							ideo.AddPrecept(precept_Weapon);
							precept_Weapon.Init(ideo);
						}, pr.Icon, ideo.Color)));
					}
				}
			}
			FloatMenuOption PostProcessOption(FloatMenuOption option)
			{
				if (!acceptance)
				{
					option.action = null;
					option.Label = option.Label + " (" + acceptance.Reason + ")";
				}
				return option;
			}

			return false;
		}
	}





	// 규율 - 하단 랜덤 버튼 제거

	[HarmonyPatch(typeof(Dialog_ReformIdeo))]
	[HarmonyPatch("DoWindowContents")]
	internal class patch_Dialog_ReformIdeo_DoWindowContents
	{
		static Color DisabledColor => AccessTools.StaticFieldRefAccess<Color>(AccessTools.Field(typeof(Dialog_ReformIdeo), "DisabledColor")).Invoke();

		[HarmonyPrefix]
		private static bool Prefix(Dialog_ReformIdeo __instance, Rect inRect, List<MemeDef> ___tmpPreSelectedMemes, Ideo ___ideo, ref List<MemeDef> ___tmpNormalMemes, Ideo ___newIdeo, ref float ___scrollViewHeight, ref Vector2 ___scrollPosition, ref IdeoReformStage ___stage, ref Vector2 ___MemeBoxSize)
		{
			//AnyChooseOneChanges = (bool)AccessTools.Property(typeof(Dialog_ReformIdeo), "AnyChooseOneChanges").GetValue(__instance);

			Text.Font = GameFont.Medium;
			Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 40f), "ReformIdeoligion".Translate());
			Text.Font = GameFont.Small;
			Widgets.Label(new Rect(inRect.x, inRect.y + 40f, inRect.width, 50f), "ReformIdeoligionDesc".Translate());
			Rect outRect = new Rect(inRect.x, inRect.y + 40f + 50f, inRect.width, inRect.height - 55f - 40f - 50f);
			Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, ___scrollViewHeight);
			Widgets.BeginScrollView(outRect, ref ___scrollPosition, viewRect);
			float num = 0f;
			if (___stage == IdeoReformStage.MemesAndStyles)
			{
				Rect rect = new Rect(0f, num, viewRect.width - 16f, Text.LineHeight);
				Widgets.Label(rect, "ReformIdeoChooseOneChange".Translate());
				GUI.color = new Color(1f, 1f, 1f, 0.5f);
				Widgets.DrawLineHorizontal(0f, rect.y + Text.LineHeight, viewRect.width - 16f);
				GUI.color = Color.white;
				num += 2f * Text.LineHeight;
				Widgets.Label(new Rect(viewRect.x, num, viewRect.width, Text.LineHeight), "ReformIdeoChangeStructure".Translate());
				num += Text.LineHeight + 10f;
				_ = viewRect.x;
				bool flag = __instance.AnyChooseOneChanges && !__instance.StructureMemeChanged;
				Rect rect2 = new Rect(viewRect.x + viewRect.width / 2f - ___MemeBoxSize.x / 2f, num, ___MemeBoxSize.x, ___MemeBoxSize.y);
				IdeoUIUtility.DoMeme(rect2, ___newIdeo.StructureMeme, ___newIdeo, (!flag) ? IdeoEditMode.Reform : IdeoEditMode.None);
				if (flag)
				{
					Widgets.DrawRectFast(rect2, DisabledColor);
					if (Widgets.ButtonInvisible(rect2))
					{
						Messages.Message("MessageFluidIdeoOneChangeAllowed".Translate(), MessageTypeDefOf.RejectInput, historical: false);
					}
				}
				num += ___MemeBoxSize.y + 17f;
				___tmpNormalMemes.Clear();
				for (int i = 0; i < ___newIdeo.memes.Count; i++)
				{
					if (___newIdeo.memes[i].category == MemeCategory.Normal)
					{
						___tmpNormalMemes.Add(___newIdeo.memes[i]);
					}
				}
				Widgets.Label(new Rect(viewRect.x, num, viewRect.width, Text.LineHeight), (___ideo.memes.Where((MemeDef m) => m.category == MemeCategory.Normal).Count() <= 1) ? "ReformIdeoAddMeme".Translate() : "ReformIdeoAddOrRemoveMeme".Translate());
				num += Text.LineHeight + 10f;
				int num2 = Mathf.CeilToInt((float)___tmpNormalMemes.Count / 5f);
				bool flag2 = __instance.AnyChooseOneChanges && !__instance.NormalMemesChanged;
				for (int j = 0; j < num2; j++)
				{
					num += (float)j * ___MemeBoxSize.y + (float)((j > 0) ? 10 : 0);
					int num3 = j * 5;
					int num4 = Mathf.Min(5, ___tmpNormalMemes.Count - j * 5);
					float num5 = (float)num4 * (___MemeBoxSize.x + 10f);
					float num6 = (viewRect.width - num5) / 2f;
					for (int k = num3; k < num3 + num4; k++)
					{
						Rect rect3 = new Rect(num6, num, ___MemeBoxSize.x, ___MemeBoxSize.y);
						IdeoUIUtility.DoMeme(rect3, ___tmpNormalMemes[k], ___newIdeo, (!flag2) ? IdeoEditMode.Reform : IdeoEditMode.None, drawHighlight: true, delegate
						{
							___tmpPreSelectedMemes.Clear();
							___tmpPreSelectedMemes.AddRange(___newIdeo.memes.Where((MemeDef m) => !___ideo.memes.Contains(m)));
							___ideo.CopyTo(___newIdeo);
							Find.WindowStack.Add(new Dialog_ChooseMemes(___newIdeo, MemeCategory.Normal, initialSelection: false, null, ___tmpPreSelectedMemes, reformingIdeo: true));
						});
						if (flag2)
						{
							Widgets.DrawRectFast(rect3, DisabledColor);
							if (Widgets.ButtonInvisible(rect3))
							{
								Messages.Message("MessageFluidIdeoOneChangeAllowed".Translate(), MessageTypeDefOf.RejectInput, historical: false);
							}
						}
						num6 += ___MemeBoxSize.x + 10f;
					}
				}
				___tmpNormalMemes.Clear();
				num += ___MemeBoxSize.y + 17f;
				Widgets.Label(new Rect(viewRect.x, num, viewRect.width, Text.LineHeight), "ReformIdeoChangeStyles".Translate());
				num += Text.LineHeight + 10f;
				float curX = viewRect.x;
				Rect position = new Rect(curX, num, 0f, 0f);
				bool flag3 = __instance.AnyChooseOneChanges && !__instance.StylesChanged;
				IdeoUIUtility.DoStyles(ref num, ref curX, viewRect.width, ___newIdeo, (!flag3) ? IdeoEditMode.Reform : IdeoEditMode.None, 50);
				if (flag3)
				{
					position.width = curX - position.x;
					position.height = num - position.y;
					Widgets.DrawRectFast(position, DisabledColor);
				}
				num += 67f;
			}
			else
			{
				Rect rect4 = new Rect(0f, num, viewRect.width - 16f, Text.LineHeight);
				Widgets.Label(rect4, "ReformIdeoChangeAny".Translate());
				GUI.color = new Color(1f, 1f, 1f, 0.5f);
				Widgets.DrawLineHorizontal(0f, rect4.y + Text.LineHeight, viewRect.width - 16f);
				GUI.color = Color.white;
				num += 2f * Text.LineHeight;
				float width = viewRect.width - 16f;
				IdeoUIUtility.DoNameAndSymbol(ref num, width, ___newIdeo, IdeoEditMode.Reform);
				num += 10f;
				IdeoUIUtility.DoDescription(ref num, width, ___newIdeo, IdeoEditMode.Reform);
				num += 10f;
				if (___newIdeo.foundation != null)
				{
					IdeoUIUtility.DoFoundationInfo(ref num, width, ___newIdeo, IdeoEditMode.Reform);
					num += 10f;
				}
				IdeoUIUtility.DoPrecepts(ref num, width, ___newIdeo, IdeoEditMode.Reform);
				num += 10f;
				IdeoUIUtility.DoAppearanceItems(___newIdeo, IdeoEditMode.Reform, ref num, width);
			}
			if (Event.current.type == EventType.Layout)
			{
				___scrollViewHeight = num;
			}
			Widgets.EndScrollView();
			Rect rect5 = new Rect(inRect.xMax - Window.CloseButSize.x, inRect.height - Window.CloseButSize.y, Window.CloseButSize.x, Window.CloseButSize.y);
			if (___stage == IdeoReformStage.MemesAndStyles)
			{
				Rect rect6 = rect5;
				rect6.x = inRect.x;
				if (Widgets.ButtonText(rect6, "Cancel".Translate()))
				{
					__instance.Close();
				}
				if (__instance.AnyChooseOneChanges)
				{
					if (Widgets.ButtonText(new Rect(inRect.x + (inRect.width - Window.CloseButSize.x) / 2f, inRect.height - Window.CloseButSize.y, Window.CloseButSize.x, Window.CloseButSize.y), "ReformIdeoResetChanges".Translate()))
					{
						SoundDefOf.Tick_Low.PlayOneShotOnCamera();
						//ResetAllChooseOneChanges();
						AccessTools.Method(typeof(Dialog_ReformIdeo), "ResetAllChooseOneChanges").Invoke(__instance, new object[] { });
					}
					num += 47f;
				}
				if (Widgets.ButtonText(rect5, "Next".Translate()))
				{
					___stage = IdeoReformStage.PreceptsNarrativeAndDeities;
				}
				return false;
			}
			Rect rect7 = rect5;
			rect7.x = inRect.x;
			if (Widgets.ButtonText(rect7, "Back".Translate()))
			{
				___stage = IdeoReformStage.MemesAndStyles;
			}
			Rect rect8 = rect5;
			rect8.x = inRect.x + (inRect.width - rect8.width) / 2f;
			//if (Widgets.ButtonText(rect8, "Randomize".Translate()))
			//{
			//	RandomizeNewIdeo();
			//}
			if (Widgets.ButtonText(rect5, "DoneButton".Translate()))
			{
				IdeoDevelopmentUtility.ConfirmChangesToIdeo(___ideo, ___newIdeo, delegate
				{
					IdeoDevelopmentUtility.ApplyChangesToIdeo(___ideo, ___newIdeo);
					__instance.Close();
				});
			}
			return false;
		}
	}





	//[HarmonyPatch(typeof(Window))]
 //   [HarmonyPatch("Close")]
	//internal class patch_Dialog_ChooseMemes_Close
	//{
	//	[HarmonyPrefix]
	//	private static bool Prefix(Window __instance, bool doCloseSound)
	//	{
	//		if (__instance is Dialog_ReformIdeo && Find.FactionManager.OfPlayer.ideos.PrimaryIdeo.development.points > 0)
	//		{
	//			return false;
	//		}
	//		if (__instance is Dialog_ChooseMemes) return false;
			
	//		return true;

	//	}
	//}





}
