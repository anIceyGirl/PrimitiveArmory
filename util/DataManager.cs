using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using RWCustom;
using System.Linq;

namespace PrimitiveArmory
{
    public static class DataManager
	{
		public static string[] resources = new string[]
		{
			"arenaIconAtlas.png",
			"arenaIconAtlas.txt",
			"objectAtlas.png",
			"objectAtlas.txt"
		};

		public static string[] sfx = new string[]
		{
			"Bottle_Slosh_1.wav",
			"Bottle_Slosh_2.wav",
			"Bottle_Slosh_3.wav",
			"Bottle_Slosh_4.wav",
			"Bottle_Smash_1.wav",
			"Bottle_Smash_2.wav",
			"Bottle_Smash_3.wav",
			"Bottle_Smash_4.wav",
			"Bow_Draw.wav",
			"Bow_Fire.wav",
			"Bow_Fire_Dry.wav",
			"Slugcat_Bow_Pickup.wav"
		};

		public static void Patch()
		{
			PreLoad();

			On.RainWorld.LoadResources += Load;

			On.RainWorld.LoadResources += LoadResourcesPatch;
			
			// PreLoadSounds();
		}

        private static void LoadResourcesPatch(On.RainWorld.orig_LoadResources orig, RainWorld self)
		{
			orig(self);

			Futile.atlasManager.LoadAtlas("Atlases" + Path.DirectorySeparatorChar + "arenaIconAtlas");
			Futile.atlasManager.LoadAtlas("Atlases" + Path.DirectorySeparatorChar + "objectAtlas");
		}

		private static void Load(On.RainWorld.orig_LoadResources orig, RainWorld self)
		{
			orig(self);
		}

		private static void PreLoad()
		{
			string path = Custom.RootFolderDirectory() + "Assets" + Path.DirectorySeparatorChar + "Futile" + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar + "Atlases";
			Directory.CreateDirectory(path);

			byte[] array;

			Stream manifestResourceStream;

			for (int i = 0; i < resources.Length; i++)
            {
				Debug.Log("Writing " + resources[i] + " to Atlases folder");

				manifestResourceStream = typeof(Main).Assembly.GetManifestResourceStream("PrimitiveArmory.resources." + resources[i]);
				array = new byte[manifestResourceStream.Length];
				manifestResourceStream.Read(array, 0, (int)manifestResourceStream.Length);

				System.IO.File.WriteAllBytes(path + Path.DirectorySeparatorChar + resources[i], array);
			}
		}

		private static void PreLoadSounds()
        {
			string path = Custom.RootFolderDirectory() + "Assets" + Path.DirectorySeparatorChar + "Futile" + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar + "LoadedSoundEffects";

			Directory.CreateDirectory(path);

			byte[] array;

			Stream manifestResourceStream;

			for (int i = 0; i < sfx.Length; i++)
			{
				Debug.Log("Writing " + sfx[i] + " to LoadedSoundEffects folder");

				manifestResourceStream = typeof(Main).Assembly.GetManifestResourceStream("PrimitiveArmory.resources.sfx." + sfx[i]);
				array = new byte[manifestResourceStream.Length];
				manifestResourceStream.Read(array, 0, (int)manifestResourceStream.Length);

				System.IO.File.WriteAllBytes(path + Path.DirectorySeparatorChar + sfx[i], array);
				System.IO.File.WriteAllBytes(path + Path.DirectorySeparatorChar + sfx[i] + ".meta", new byte[0]);
			}
		}

		public static void LogAllResources()
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			Debug.Log("Log All Resources: ");
			string[] array = manifestResourceNames;
			foreach (string message in array)
			{
				Debug.Log(message);
			}
		}
	}
}
