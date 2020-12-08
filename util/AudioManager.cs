using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using RWCustom;
using System.Linq;
using System.Text.RegularExpressions;

namespace PrimitiveArmory
{
    public class AudioManager
	{
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
			"Slugcat_Bow_Pickup.wav",
			"Slugcat_Blunt_Pickup.wav",
			"Slugcat_Tool_Pickup.wav",
			"Armor_Heavy_Move_1.wav",
			"Armor_Heavy_Move_2.wav",
			"Armor_Heavy_Move_3.wav",
			"Armor_Heavy_Move_4.wav",
			"Armor_Heavy_Move_5.wav",
			"Armor_Heavy_Move_6.wav",
			"Armor_Heavy_Shuffle_1.wav",
			"Armor_Heavy_Shuffle_2.wav",
			"Armor_Heavy_Shuffle_3.wav",
			"Armor_Heavy_Shuffle_4.wav",
			"Armor_Light_Move_1.wav",
			"Armor_Light_Move_2.wav",
			"Armor_Light_Move_3.wav"
		};

		public static void Patch()
        {
			On.SoundLoader.LoadSounds += SoundLoaderPatch;
			
			PreLoadSounds();
		}

        private static void SoundLoaderPatch(On.SoundLoader.orig_LoadSounds orig, SoundLoader self)
        {
            orig(self);

			Stream manifestResourceStream;
			manifestResourceStream = typeof(Main).Assembly.GetManifestResourceStream("PrimitiveArmory.resources.sfx.Sounds.txt");

			byte[] array = new byte[manifestResourceStream.Length];

			manifestResourceStream.Read(array, 0, (int)manifestResourceStream.Length);

			string[] list = Regex.Split(System.Text.Encoding.Default.GetString(array, 0, (int)manifestResourceStream.Length), "\n");

			foreach (string str in list)
            {
				Debug.Log(str);
            }


		}

        private static void PreLoadSounds()
		{
			string path = Custom.RootFolderDirectory() + "Assets" + Path.DirectorySeparatorChar + "Futile" + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar + "LoadedSoundEffects";

			Directory.CreateDirectory(path);

			byte[] array;
			byte[] emptyByte = new byte[0]
			{
				 
			};

			Stream manifestResourceStream;

			for (int i = 0; i < sfx.Length; i++)
			{
				Debug.Log("Writing " + sfx[i] + " to LoadedSoundEffects folder");

				manifestResourceStream = typeof(Main).Assembly.GetManifestResourceStream("PrimitiveArmory.resources.sfx." + sfx[i]);
				array = new byte[manifestResourceStream.Length];
				manifestResourceStream.Read(array, 0, (int)manifestResourceStream.Length);
				

				System.IO.File.WriteAllBytes(path + Path.DirectorySeparatorChar + sfx[i], array);
				System.IO.File.WriteAllBytes(path + Path.DirectorySeparatorChar + sfx[i] + ".meta", emptyByte);
			}
		}
	}
}
