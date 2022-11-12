using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;

namespace IdeoReformLimited
{
	public static class TransplierHelpers
	{
		/// <summary>
		/// Pulls code instructions until button with a given label found. Button call is replaced with a given method from the <see cref="ModWidgets"/> class.
		/// Enumerator is left in the position right after the replaced call
		/// </summary>
		/// <param name="enumerator">Code instructions to search through</param>
		/// <param name="labelKey">Replaced button label's translation key</param>
		/// <param name="detourMethodName">Name of the replacement method from <see cref="ModWidgets"/> class</param>
		/// <returns>Processed code instructions</returns>
		public static IEnumerable<CodeInstruction> FindAndReplaceButton(IEnumerator<CodeInstruction> enumerator, string labelKey, string detourMethodName = nameof(ModWidgets.ButtonTextSkipIfFluid))
		{
			MethodInfo buttonText = AccessTools.Method(typeof(Widgets), nameof(Widgets.ButtonText), new Type[] { typeof(Rect), typeof(string), typeof(bool), typeof(bool), typeof(bool), typeof(TextAnchor?) });
			MethodInfo buttonReplacement = AccessTools.Method(typeof(ModWidgets), detourMethodName);
			bool foundLabel = false;

			while (enumerator.MoveNext())
			{
				if (!foundLabel)
				{
					// Find operation that puts labelKey string onto the stack
					if (enumerator.Current.opcode == OpCodes.Ldstr && enumerator.Current.operand as string == labelKey)
					{
						foundLabel = true;
					}
				}
				else if (enumerator.Current.opcode == OpCodes.Call && enumerator.Current.operand as MethodInfo == buttonText)
				{
					// Replace next button call with the given method
					enumerator.Current.operand = buttonReplacement;
					yield return enumerator.Current;
					yield break;
				}

				yield return enumerator.Current;
			}
		}
	}
}