using UnityEngine;
using Verse;

namespace IdeoReformLimited
{
	public class SettingHandle<T>(string xmlLabel, string label, string description, T defaultValue, int intMinValue = 0)
	{
		public T Value { get; set; } = defaultValue;

		private readonly T defaultValue = defaultValue;
		private readonly string xmlLabel = xmlLabel;
		private readonly string label = label;
		private readonly string description = description;
		private readonly int intMinValue = intMinValue;
		private string? editBuffer;

		public void Scribe()
		{
			T val = Value;
			Scribe_Values.Look(ref val, xmlLabel, defaultValue);
			Value = val;
		}

		public void DoSetting(Listing_Standard listing)
		{
			if (Value is int intVal)
			{
				DoIntSetting(listing, ref intVal);
				Value = (T)(object)intVal;
			}
			else if (Value is bool boolVal)
			{
				listing.CheckboxLabeled(label.Translate(), ref boolVal, description.Translate());
				Value = (T)(object)boolVal;
			}
		}

		public void DoIntSetting(Listing_Standard listing, ref int intVal)
		{
			Rect lineRect = listing.GetRect(24f);
			lineRect.SplitVertically(lineRect.width * 2f / 3f, out Rect labelRect, out Rect controlRect);
			Widgets.Label(labelRect, label.Translate());
			controlRect.SplitVertically(controlRect.width / 3f, out Rect defaultRect, out Rect entryRect);

			if (Widgets.ButtonText(defaultRect, "Default".Translate() + ": " + defaultValue?.ToString()))
			{
				intVal = defaultValue as int? ?? 0;
				editBuffer = intVal.ToString();
			}

			Widgets.IntEntry(entryRect, ref intVal, ref editBuffer);
			if (intVal < intMinValue)
			{
				intVal = intMinValue;
				editBuffer = intVal.ToString();
			}
			if (Mouse.IsOver(lineRect))
			{
				Widgets.DrawHighlight(lineRect);
			}
			TooltipHandler.TipRegion(lineRect, description.Translate());
			listing.Gap(listing.verticalSpacing);
		}

		public static implicit operator T(SettingHandle<T> settingHandle)
		{
			return settingHandle.Value;
		}
	}
}