using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace IdeoReformLimited.Patches
{
	[HarmonyPatch(typeof(IdeoUIUtility), "DoPreceptsInt")]
	internal static class Patch_IdeoUIUtility_DoPreceptsInt
	{
		/// <summary>
		/// Inject PreceptLimiter call.
		/// Replace "RandomizePrecepts" button with the one that will hide itself if idelolgy is fluid
		/// </summary>
		/// <param name="instructions">Original method body</param>
		/// <returns>Modified method body</returns>
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			using IEnumerator<CodeInstruction> enumerator = instructions.GetEnumerator();

			foreach (CodeInstruction instruction in InjectPreceptLimiter(enumerator))
			{
				yield return instruction;
			}

			foreach (CodeInstruction instruction in TransplierHelpers.FindAndReplaceButton(enumerator, "RandomizePrecepts"))
			{
				yield return instruction;
			}

			while (enumerator.MoveNext())
			{
				yield return enumerator.Current;
			}
		}

		/// <summary>
		/// Place PreceptLimiter call right after tmpPrecepts is filled.
		/// Enumerator is left in the position right after the placed call
		/// </summary>
		/// <param name="enumerator">Code instructions to search through</param>
		/// <returns>Processed code instructions</returns>
		private static IEnumerable<CodeInstruction> InjectPreceptLimiter(IEnumerator<CodeInstruction> enumerator)
		{
			MethodInfo limitPrecepts = AccessTools.Method(typeof(Patch_IdeoUIUtility_DoPreceptsInt), nameof(LimitPrecepts));
			bool injected = false;
			CodeInstruction[] peeked = new CodeInstruction[6];

			while (enumerator.MoveNext())
			{
				yield return enumerator.Current;

				// Search for the last operation of the fill loop
				if (enumerator.Current.opcode != OpCodes.Blt_S)
				{
					continue;
				}

				// If found, cut 6 next operators to analize
				for (int i = 0; i < peeked.Length; i++)
				{
					if (!enumerator.MoveNext())
					{
						// Less then 6 ops before the method end. Fail
						foreach (CodeInstruction instruction in peeked)
						{
							// Don't forget to return all of the pulled operators
							yield return instruction;
						}
						Log.Error("[LimitedIdeoReform] Failed to transpile precept limiter into IdeoUIUtility.DoPreceptsInt");
						yield break;
					}

					peeked[i] = enumerator.Current;
				}

				// The next 6 ops corresponds to this lines
				// if (!mainPrecepts && preceptsListForReading.Count == 0)
				// {
				//     return;
				// }
				if (peeked[0].opcode == OpCodes.Ldarg_2 &&
					peeked[1].opcode == OpCodes.Brtrue_S &&
					peeked[2].opcode == OpCodes.Ldloc_1 &&
					peeked[3].opcode == OpCodes.Callvirt &&
					peeked[4].opcode == OpCodes.Brtrue_S &&
					peeked[5].opcode == OpCodes.Ret)
				{
					yield return new CodeInstruction(OpCodes.Ldarg_2); // Load mainPrecept argument
					yield return new CodeInstruction(OpCodes.Ldarg_S, 4); // Load editMode argument
					yield return CodeInstruction.LoadField(typeof(IdeoUIUtility), "tmpPrecepts");
					yield return new CodeInstruction(OpCodes.Call, limitPrecepts);
					injected = true;
				}

				// Again, don't forget to return all of the peeked operators
				foreach (CodeInstruction instruction in peeked)
				{
					yield return instruction;
				}

				if (injected)
				{
					yield break;
				}
			}

			Log.Error("[LimitedIdeoReform] Failed to transpile precept limiter into IdeoUIUtility.DoPreceptsInt");
		}

		/// <summary>
		/// Apply random filter to the static list
		/// </summary>
		/// <param name="mainPrecepts">Method id called for the main precepts. As opposed to closing/ritual/etc</param>
		/// <param name="editMode">What king of edit is happening</param>
		/// <param name="tmpPrecepts">Reference to a static field used by DoPreceptsInt to collect precepts for output</param>
		public static void LimitPrecepts(bool mainPrecepts, IdeoEditMode editMode, List<Precept> tmpPrecepts)
		{
			if (!(editMode == IdeoEditMode.Reform && mainPrecepts))
			{
				return;
			}

			HashSet<IssueDef> limitedIssues = Core.ReformIdeoDialogContext.limitedPreceptIssues;

			if (limitedIssues.Count != 0)
			{
				for(int i = tmpPrecepts.Count - 1; i >= 0; i--)
				{
					if (!limitedIssues.Contains(tmpPrecepts[i].def.issue)){
						tmpPrecepts.RemoveAt(i);
					}
				}
				return;
			}

			List<Precept> preceptPool = tmpPrecepts.OrderBy(a => a.def.defName).ToList();
			tmpPrecepts.Clear();

			while (tmpPrecepts.Count < Core.NumberOfPreceptsToChooseFromOnReform && preceptPool.Count > 0)
			{
				int randomIndex = Rand.RangeSeeded(0, preceptPool.Count, Core.Seed);
				Precept tmp = preceptPool[randomIndex];
				preceptPool.RemoveAt(randomIndex);
				if (!Core.SkipUneditablePrecepts || CanBeEdited(tmp))
				{
					tmpPrecepts.Add(tmp);
				}
			}

			for(int i = tmpPrecepts.Count - 1; i >= 0; i--)
			{
				IssueDef issueDef = tmpPrecepts[i].def.issue;
				if (!limitedIssues.Contains(issueDef)){
					limitedIssues.Add(issueDef);
				}
			}

			tmpPrecepts.SortByDescending(x => (int)x.def.impact);
		}

		/// <summary>
		/// This method attempts to repeat Precept.DrawPreceptBox logic for collecting FloatMenuOptions but in boolean
		/// </summary>
		private static bool CanBeEdited(Precept precept)
		{
			return CanBeRemoved(precept) || CanBeChanged(precept);
		}

		private static bool CanBeRemoved(Precept precept)
		{
			if (!precept.def.canRemoveInUI || precept.def.issue.HasDefaultPrecept)
			{
				return false;
			}

			if (precept.ideo.GetMemeThatRequiresPrecept(precept.def) != null)
			{
				return false;
			}

			return true;
		}

		private static bool CanBeChanged(Precept precept)
		{
			return DefDatabase<PreceptDef>.AllDefs.Any(x =>
				x.issue == precept.def.issue &&
				x != precept.def &&
				IdeoUIUtility.CanListPrecept(precept.ideo, x, IdeoEditMode.Reform));
		}
	}
}
