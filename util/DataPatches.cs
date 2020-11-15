using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using RWCustom;

namespace PrimitiveArmory
{
    public class DataPatches
    {
        public static string[] objectTextureList = new string[]
        {
             "BlowGun",
             "Symbol_BlowGun",

             "Arrow",
             "Bow",
             "Symbol_Arrow",
             "Symbol_Bow",

             "Club",
             "Symbol_Club",

             "Quiver",
             "QuiverFade",
             "Symbol_Quiver",

             "Satchel1A",
             "Satchel1B",
             "Satchel1C",
             "Satchel1Fade",
             "Satchel2A",
             "Satchel2B",
             "Satchel2C",
             "Satchel2Fade",
             "Symbol_Satchel"
        };

        public static Texture2D[] objectTextures = new Texture2D[objectTextureList.Length];

        public static void Patch()
        {
            On.RainWorld.LoadResources += LoadResourcesPatch;
        }

        public static void LoadResourcesPatch(On.RainWorld.orig_LoadResources orig, RainWorld self)
        {
            orig(self);

            for (int i = 0; i < objectTextureList.Length; i++)
            {
                objectTextures[i] = DataManager.ReadPNG(objectTextureList[i]);

                Futile.atlasManager.LoadAtlasFromTexture(objectTextureList[i], objectTextures[i]);
            }

        }
    }
}
