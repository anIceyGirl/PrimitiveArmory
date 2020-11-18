
using OptionalUI;
using Partiality;
using Partiality.Modloader;
using RWCustom;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PrimitiveArmory
{
    public class Main : PartialityMod
    {
        private bool made;

        public static bool enabled;
        public static bool customResources;

        public static bool Enabled => EnumExt && enabled;

        public static bool EnumExt => EnumExt_NewItems.Club > AbstractPhysicalObject.AbstractObjectType.OverseerCarcass;

        public static bool CustomResources => EnumExt && enabled && customResources;

        public Main()
        {
            instance = this;
            this.ModID = "Primitive Armory";
            this.Version = "v0.0.2";
            this.author = "Icey";
        }

        public static OptionInterface LoadOI()
        {
            return new PrimitiveArmoryConfig();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            // DataManager.EncodeExternalPNG();
            
            enabled = true;
            customResources = true;

            Debug.Log("Ready for hooking for PrimitiveArmory...");

            On.AbstractPhysicalObject.Realize += RealizePatch;
            On.MultiplayerUnlocks.SymbolDataForSandboxUnlock += SandboxIconPatch;
            On.RainWorld.Update += MakeMe;
            On.ItemSymbol.SpriteNameForItem += ItemSymbol_SpriteNameForItem;

            Debug.Log("Hooking Savestate (we'll probably crash here if we're running more than two patches without BepinEx)");
            On.SaveState.AbstractPhysicalObjectFromString += AbstractFromStringPatch;
            Debug.Log("We haven't crashed yet? Sick.");

            On.RegionState.AdaptWorldToRegionState += CustomRegionLoad;

            On.RainWorld.Start += RainWorld_Start;

            // IceyDebug.DebugHook();
            PlayerHooks.Patch();
            DataManager.Patch();

            Futile.atlasManager.LogAllElementNames();

            Debug.Log("PrimitiveArmory Hooking Complete!");
        }

        private string ItemSymbol_SpriteNameForItem(On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
        {
            if (itemType == EnumExt_NewItems.Club)
            {
                return "Symbol_Club";
            }

            return orig.Invoke(itemType, intData);
        }

        private void RainWorld_Start(On.RainWorld.orig_Start orig, RainWorld self)
        {
            orig(self);

            DataManager.LogAllResources();
        }

        public static Main instance;

        public static void RealizePatch(On.AbstractPhysicalObject.orig_Realize orig, AbstractPhysicalObject self)
        {

            if (self.realizedObject != null)
            {
                return;
            }

            if (self.type == EnumExt_NewItems.Club)
            {
                self.realizedObject = new Club(self, self.world);

                goto StuckRealize;
            }

            orig(self);


        StuckRealize:
            for (int i = 0; i < self.stuckObjects.Count; i++)
            {
                if (self.stuckObjects[i].A.realizedObject == null && self.stuckObjects[i].A != self)
                {
                    self.stuckObjects[i].A.Realize();
                }
                if (self.stuckObjects[i].B.realizedObject == null && self.stuckObjects[i].B != self)
                {
                    self.stuckObjects[i].B.Realize();
                }
            }


        }

        private static IconSymbol.IconSymbolData SandboxIconPatch(On.MultiplayerUnlocks.orig_SymbolDataForSandboxUnlock orig, MultiplayerUnlocks.SandboxUnlockID unlockID)
        {
            if (unlockID == EnumExt_NewItems.ClubUnlock)
            {
                return new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, EnumExt_NewItems.Club, 0);
            }

            return orig(unlockID);
        }

        private AbstractPhysicalObject AbstractFromStringPatch(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
        {
            AbstractPhysicalObject result = orig(world, objString);
            try
            {
                string[] array = Regex.Split(objString, "<oA>");
                EntityID ID = EntityID.FromString(array[0]);
                AbstractPhysicalObject.AbstractObjectType abstractObjectType = Custom.ParseEnum<AbstractPhysicalObject.AbstractObjectType>(array[1]);
                WorldCoordinate pos = new WorldCoordinate(int.Parse(array[2].Split('.')[0]), int.Parse(array[2].Split('.')[1]), int.Parse(array[2].Split('.')[2]), int.Parse(array[2].Split('.')[3]));

                if (result == null)
                {
                    return null;
                }

                if (result.type == EnumExt_NewItems.Club)
                {

                }

                return result;
            }
            catch
            {
                return null;
            }
        }

        public void MakeMe(On.RainWorld.orig_Update orig, RainWorld self)
        {
            orig(self);

            if (!made)
            {
                made = true;
                AddItems();
            }
        }

        private void AddItems()
        {
            SandboxUnlockCore.Main.items.Add(EnumExt_NewItems.ClubUnlock);
        }

        private static void CustomRegionLoad(On.RegionState.orig_AdaptWorldToRegionState orig, RegionState self)
        {
            for (int i = 0; i < self.savedObjects.Count; i++)
            {
                AbstractPhysicalObject abstractPhysicalObject = LoadCustomItem(self.world, self.savedObjects[i]);
                if (abstractPhysicalObject != null)
                {
                    self.savedObjects[i] = null;
                    if (abstractPhysicalObject.pos.room >= self.world.firstRoomIndex && abstractPhysicalObject.pos.room < self.world.firstRoomIndex + self.world.NumberOfRooms)
                    {
                        self.world.GetAbstractRoom(abstractPhysicalObject.pos).AddEntity(abstractPhysicalObject);
                        continue;
                    }
                    Debug.Log("trying to respawn object in room outside of world. " + abstractPhysicalObject.type.ToString() + " " + abstractPhysicalObject.pos.room + "(" + self.world.firstRoomIndex + "-" + (self.world.firstRoomIndex + self.world.NumberOfRooms) + ")");
                }
            }
            orig(self);
        }

        private static AbstractPhysicalObject LoadCustomItem(World world, string objString)
        {
            AbstractPhysicalObject result = null;
            try
            {
                string[] array = Regex.Split(objString, "<oA>");
                EntityID iD = EntityID.FromString(array[0]);
                AbstractPhysicalObject.AbstractObjectType abstractObjectType = Custom.ParseEnum<AbstractPhysicalObject.AbstractObjectType>(array[1]);
                WorldCoordinate pos = new WorldCoordinate(int.Parse(array[2].Split('.')[0]), int.Parse(array[2].Split('.')[1]), int.Parse(array[2].Split('.')[2]), int.Parse(array[2].Split('.')[3]));
            }
            catch
            {
                result = null;
            }
            return result;
        }

        public string Translate(string text)
        {
            return text;
        }
    }
}