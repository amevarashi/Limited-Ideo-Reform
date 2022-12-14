using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace IdeoReformLimited.Patches
{
	[HarmonyPatch(typeof(Dialog_ChooseMemes))]
	public static class patch_Dialog_ChooseMemes
	{
		public static MemeCategory CurrentMemeCategory { get; private set; }

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

			List<MemeDef> memesPlayerHave = new List<MemeDef>();
			List<MemeDef> memesCanBeAdded = new List<MemeDef>();
			List<MemeDef> finalSelectedMemes = new List<MemeDef>();

			foreach (MemeDef meme in memes)
			{
				if (___ideo.memes.Contains(meme))
				{
					memesPlayerHave.Add(meme);
				}
				else
				{
					memesCanBeAdded.Add(meme);
				}
			}

			if (memesPlayerHave.Count >= Core.val_maxMemeCount)
			{
				// If at max possible memes, only show memes to remove
				for (int i = 0; i < Mathf.Max(2, Mathf.FloorToInt(Core.val_maxMemeCount * 0.25f)); i++)
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
				float removeCount = Mathf.Min(memesPlayerHave.Count * 0.25f, Core.val_memeSelectCount * 0.25f);

				for (int i = 0; i < Core.val_memeSelectCount; i++)
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
		}
	}
}
