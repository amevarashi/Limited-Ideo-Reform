using System.Reflection;
using HarmonyLib;
using Verse;

namespace IdeoReformLimited
{
	[StaticConstructorOnStartup]
	internal class HarmonyBootstrap
	{
		static HarmonyBootstrap()
		{
			new Harmony("IdeoReformLimited").PatchAll(Assembly.GetExecutingAssembly());
		}
	}
}