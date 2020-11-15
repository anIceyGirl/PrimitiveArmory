using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using RWCustom;

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

		public static void Patch()
		{
			PreLoad();

			On.RainWorld.LoadResources += Load;

			On.RainWorld.LoadResources += LoadResourcesPatch;
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
			Assembly executingAssembly = Assembly.GetExecutingAssembly();

			string path = Custom.RootFolderDirectory() + "Assets" + Path.DirectorySeparatorChar + "Futile" + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar + "Atlases";
			Directory.CreateDirectory(path);

			string[] resourceNames = executingAssembly.GetManifestResourceNames();
			byte[] array;

			Stream manifestResourceStream;

			for (int i = 0; i < resources.Length; i++)
            {
				manifestResourceStream = typeof(Main).Assembly.GetManifestResourceStream("PrimitiveArmory.resources." + resources[i]);
				array = new byte[manifestResourceStream.Length];
				manifestResourceStream.Read(array, 0, (int)manifestResourceStream.Length);

				System.IO.File.WriteAllBytes(path + Path.DirectorySeparatorChar + resources[i], array);
			}
		}

		public static Texture2D ReadPNG(string pngName)
		{
			Debug.Log("Creating Texture based on " + pngName + ".png");
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipmap: false)
            {
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0,
                filterMode = FilterMode.Point
            };
            Stream manifestResourceStream = typeof(Main).Assembly.GetManifestResourceStream("PrimitiveArmory.resources." + pngName + ".png");
			byte[] array = new byte[manifestResourceStream.Length];
			manifestResourceStream.Read(array, 0, (int)manifestResourceStream.Length);
			texture2D.LoadImage(array);

			return texture2D;
		}

		public static string ReadTXT(string txtName)
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string name = "PrimitiveArmory.resources." + txtName + ".txt";
			using Stream stream = executingAssembly.GetManifestResourceStream(name);
			using StreamReader streamReader = new StreamReader(stream);
			return streamReader.ReadToEnd();
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
