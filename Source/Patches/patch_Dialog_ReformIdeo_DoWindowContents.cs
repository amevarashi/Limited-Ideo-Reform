using HarmonyLib;
using RimWorld;
using System.Collections.Generic;

namespace IdeoReformLimited.Patches
{
	// Remove "Randomize" button in fluid ideology
	[HarmonyPatch(typeof(Dialog_ReformIdeo), nameof(Dialog_ReformIdeo.DoWindowContents))]
	internal static class Patch_Dialog_ReformIdeo_DoWindowContents
	{
		/// <summary>
		/// Replace "Randomize" button with the one that will hide itself if idelolgy is fluid
		/// </summary>
		/// <param name="instructions">Original method body</param>
		/// <returns>Modified method body</returns>
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			IEnumerator<CodeInstruction> enumerator = instructions.GetEnumerator();

			foreach (CodeInstruction instruction in TransplierHelpers.FindAndReplaceButton(enumerator, "Randomize"))
			{
				yield return instruction;
			}

			while (enumerator.MoveNext())
			{
				yield return enumerator.Current;
			}
		}
	}
}
