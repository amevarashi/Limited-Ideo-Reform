using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace IdeoReformLimited.Patches
{
	[HarmonyPatch(typeof(Dialog_ReformIdeo))]
	public static class Patch_Dialog_ReformIdeo
	{
		/// <summary>
		/// Drop cache when new window created. Can't find a method to patch on close, this window doesn't override any [Pre/Post]Close()
		/// </summary>
		[HarmonyPatch(MethodType.Constructor, new Type[] { typeof(Ideo) })]
		[HarmonyPostfix]
		public static void ConstructorPostfix()
		{
			Core.ReformIdeoDialogContext = new ReformIdeoDialogContext();
		}
		
		/// <summary>
		/// Replace "Randomize" button with the one that will hide itself if ideology is fluid
		/// Inject PreClose call
		/// </summary>
		/// <param name="instructions">Original method body</param>
		/// <returns>Modified method body</returns>
		[HarmonyPatch(nameof(Dialog_ReformIdeo.DoWindowContents))]
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			using IEnumerator<CodeInstruction> enumerator = instructions.GetEnumerator();

			MethodInfo close = AccessTools.Method(typeof(Window), nameof(Window.Close));

			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Calls(close))
				{
					yield return CodeInstruction.Call(typeof(Patch_Dialog_ReformIdeo), nameof(PreClose));
					yield return enumerator.Current;
					break;
				}
				yield return enumerator.Current;
			}

			foreach (CodeInstruction instruction in TransplierHelpers.FindAndReplaceButton(enumerator, "Randomize"))
			{
				yield return instruction;
			}

			while (enumerator.MoveNext())
			{
				yield return enumerator.Current;
			}
		}

		public static void PreClose()
		{
			Core.ReformIdeoDialogContext = null;
		}
	}
}
