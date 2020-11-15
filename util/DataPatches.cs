using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using RWCustom;

namespace PrimitiveArmory
{
    public class DataPatches
    {
        public static void Patch()
        {
            On.RainWorld.LoadResources += LoadResourcesPatch;
            On.RainWorld.LoadResources += Load;
        }

        private static void LoadResourcesPatch(On.RainWorld.orig_LoadResources orig, RainWorld self)
        {
            orig(self);
            string path = Custom.RootFolderDirectory() + "ModConfigs" + Path.DirectorySeparatorChar + Main.instance.ModID;
            Directory.CreateDirectory(path);
            string[] files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                Debug.Log(i);
                Debug.Log(File.ReadAllText(files[i]));
                Futile.atlasManager.LoadAtlas("Atlases/" + File.ReadAllText(files[i]));
            }

        }

        private static void Load(On.RainWorld.orig_LoadResources orig, RainWorld self)
        {
            orig(self);
        }
    }
}
