using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Partiality;
using Partiality.Modloader;
using UnityEngine;

namespace PrimitiveArmory
{
    public class DynamicHooks
	{
		// PLACEHOLDER FOR LATER
		// TODO: IMPLEMENT JOLLY CO-OP COMPATABILITY
		public static void SearchAndAdd()
		{
			foreach (PartialityMod loadedMod in PartialityManager.Instance.modManager.loadedMods)
			{
				if (loadedMod.ModID == "Jolly Co-op Mod" || loadedMod.ModID == "MSC Jolly Co-op Mod")
				{
					Main.jollyCoop = true;
				}
			}
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			Assembly[] array = assemblies;
			foreach (Assembly assembly in array)
			{
				try
				{
					if (assembly.GetName().Name == "MonoMod.Utils")
					{
						continue;
					}
					Type[] types = assembly.GetTypes();
					foreach (Type type in types)
					{
						if (typeof(PlayerGraphics).IsAssignableFrom(type))
						{
							GenerateHooks(type);
						}
					}
				}
				catch (Exception message)
				{
					Debug.LogError("Failed to generate hooks for assembly: " + assembly.FullName);
					Debug.LogError(message);
				}
			}
		}


		private static void GenerateHooks(Type pgType)
		{

		}
	}
}
