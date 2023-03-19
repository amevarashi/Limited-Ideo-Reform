using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace IdeoReformLimited.Patches
{
	[HarmonyPatch(typeof(Dialog_ChooseMemes))]
	public static class Patch_Dialog_ChooseMemes
	{
		private static List<MemeDef> limitedMemesCache;
		public static MemeCategory CurrentMemeCategory { get; private set; }

		/// <summary>
		/// Drop cache when new window created. Can't find a method to patch on close, this window doesn't override any [Pre/Post]Close()
		/// </summary>
		[HarmonyPatch(MethodType.Constructor, new Type[] { typeof(Ideo), typeof(MemeCategory), typeof(bool), typeof(Action), typeof(List<MemeDef>), typeof(bool) })]
		[HarmonyPostfix]
		public static void ConstructorPostfix()
		{
			limitedMemesCache = null;
		}

		/// <summary>
		/// Capture meme category for <see cref="ModWidgets.RerollButton(Rect, string, bool, bool, bool, TextAnchor?)"/>
		/// </summary>
		/// <param name="___memeCategory">Private instance attribute memeCategory, provided by Harmony</param>
		[HarmonyPatch(nameof(Dialog_ChooseMemes.DoWindowContents))]
		[HarmonyPrefix]
		public static void DoWindowContentsPrefix(MemeCategory ___memeCategory)
		{
			CurrentMemeCategory = ___memeCategory;
		}

		/// <summary>
		/// Replace "Randomize" button with a "Reroll" one
		/// </summary>
		/// <param name="instructions">Original method body</param>
		/// <returns>Modified method body</returns>
		[HarmonyPatch(nameof(Dialog_ChooseMemes.DoWindowContents))]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			IEnumerator<CodeInstruction> enumerator = instructions.GetEnumerator();

			foreach (CodeInstruction instruction in TransplierHelpers.FindAndReplaceButton(enumerator, "Randomize", nameof(ModWidgets.RerollButton)))
			{
				yield return instruction;
			}

			while (enumerator.MoveNext())
			{
				yield return enumerator.Current;
			}
		}

		/// <summary>
		/// Limit available memes
		/// </summary>
		/// <param name="memes">Memes for user to choose from</param>
		/// <param name="___ideo">Currently displayed ideology</param>
		/// <param name="___reformingIdeo">Is currently reforming ideology</param>
		[HarmonyPatch("DoNormalMemeSelector")]
		[HarmonyPrefix]
		public static void DoNormalMemeSelectorPrefix(ref List<MemeDef> memes, Ideo ___ideo, bool ___reformingIdeo)
		{
			if (!___ideo.Fluid || !___reformingIdeo)
			{
				return;
			}

			if (limitedMemesCache != null)
			{
				// Game forces pause when this dialog is open, so this should be ok
				memes.Clear();
				memes.AddRange(limitedMemesCache);
				return;
			}

			List<MemeDef> memesPlayerHave = new List<MemeDef>();
			List<MemeDef> memesCanBeAdded = new List<MemeDef>();
			HashSet<string> exclusionTags = ___ideo.memes.SelectMany(x => x.exclusionTags).Distinct().ToHashSet();

			foreach (MemeDef meme in memes)
			{
				if (___ideo.memes.Contains(meme))
				{
					memesPlayerHave.Add(meme);
				}
				else if (!meme.exclusionTags.Any(tag => exclusionTags.Contains(tag)))
				{
					memesCanBeAdded.Add(meme);
				}
			}

			List<MemeDef> finalSelectedMemes = new List<MemeDef>();

			if (memesPlayerHave.Count >= Core.MaxMemeCount)
			{
				// If at max possible memes, only show memes to remove
				for (int i = 0; i < Mathf.Max(2, Mathf.FloorToInt(Core.MaxMemeCount * 0.25f)); i++)
				{
					MemeDef meme = memesPlayerHave[Rand.RangeSeeded(0, memesPlayerHave.Count, Core.Seed)];
					memesPlayerHave.Remove(meme);
					finalSelectedMemes.Add(meme);
					if (memesPlayerHave.Count == 0)
						break;
				}
			}
			else
			{
				float removeCount = Mathf.Min(memesPlayerHave.Count * 0.25f, Core.NumberOfMemesToChooseFromOnReform * 0.25f);

				for (int i = 0; i < Core.NumberOfMemesToChooseFromOnReform; i++)
				{
					MemeDef meme;
					if (removeCount >= 1f)
					{
						meme = memesPlayerHave[Rand.RangeSeeded(0, memesPlayerHave.Count, Core.Seed)];
						memesPlayerHave.Remove(meme);
						removeCount--;
					}
					else if (removeCount > 0f && Rand.ChanceSeeded(removeCount, Core.Seed))
					{
						meme = memesPlayerHave[Rand.RangeSeeded(0, memesPlayerHave.Count, Core.Seed)];
						memesPlayerHave.Remove(meme);
						removeCount = 0f;
					}
					else
					{
						meme = memesCanBeAdded[Rand.RangeSeeded(0, memesCanBeAdded.Count, Core.Seed)];
						memesCanBeAdded.Remove(meme);
					}

					finalSelectedMemes.Add(meme);
					if (memesCanBeAdded.Count == 0)
						break;
				}
			}

			memes = finalSelectedMemes;
			limitedMemesCache = finalSelectedMemes;
		}

		/// <summary>
		/// The closest I can find to a Close() method. Doesn't run on exit without reform
		/// </summary>
		[HarmonyPatch("DoAcceptChanges")]
		[HarmonyPostfix]
		public static void PostClosePostfix()
		{
			limitedMemesCache = null;
		}
	}
}
