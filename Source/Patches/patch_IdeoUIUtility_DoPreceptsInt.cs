using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace IdeoReformLimited.Patches
{
	[HarmonyPatch(typeof(IdeoUIUtility), "DoPreceptsInt")]
	internal static class Patch_IdeoUIUtility_DoPreceptsInt
	{
		public static bool DoLimit { get; private set; }
		public static List<Precept> TmpPrecepts { get; private set; }

		/// <summary>
		/// Capture some parameters here to bypass adding stack manipulations in the transpiler
		/// </summary>
		/// <param name="mainPrecepts">Method id called for the main precepts. As opposed to closing/ritual/etc</param>
		/// <param name="editMode">What king of edit is happening</param>
		/// <param name="___tmpPrecepts">Reference to a static field used by DoPreceptsInt to collect precepts for output</param>
		public static void Prefix(bool mainPrecepts, IdeoEditMode editMode, ref List<Precept> ___tmpPrecepts)
		{
			DoLimit = (editMode == IdeoEditMode.Reform && mainPrecepts);
			TmpPrecepts = ___tmpPrecepts;
		}

		/// <summary>
		/// Inject PreceptLimiter call.
		/// Replace "RandomizePrecepts" button with the one that will hide itself if idelolgy is fluid
		/// </summary>
		/// <param name="instructions">Original method body</param>
		/// <returns>Modified method body</returns>
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			IEnumerator<CodeInstruction> enumerator = instructions.GetEnumerator();

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
			MethodInfo newMethod = AccessTools.Method(typeof(Patch_IdeoUIUtility_DoPreceptsInt), nameof(Patch_IdeoUIUtility_DoPreceptsInt.LimitPrecepts));
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
					injected = true;
					yield return new CodeInstruction(OpCodes.Call, newMethod);
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
		public static void LimitPrecepts()
		{
			if (!DoLimit)
			{
				return;
			}

			List<Precept> preceptPool = TmpPrecepts.OrderBy(a => a.def.defName).ToList();
			TmpPrecepts.Clear();

			int count = Mathf.Min(preceptPool.Count, Core.NumberOfPreceptsToChooseFromOnReform);

			for (int i = 0; i < count; i++)
			{
				Precept tmp = preceptPool[Rand.RangeSeeded(0, preceptPool.Count, Core.Seed)];
				TmpPrecepts.Add(tmp);
				preceptPool.Remove(tmp);
				if (preceptPool.Count == 0)
				{
					break;
				}
			}
		}
	}
}
