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
        private bool startupHooks;

        public static bool enabled;
        public static bool customResources;

        public static bool Enabled => EnumExt && enabled;

        public static bool EnumExt => EnumExt_NewItems.Club > AbstractPhysicalObject.AbstractObjectType.OverseerCarcass;

        public static bool CustomResources => EnumExt && enabled && customResources;

        public static bool jollyCoop = false;

        public Main()
        {
            instance = this;
            this.ModID = "Primitive Armory";
            this.Version = "v0.1.3";
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

            On.RainWorld.Update += StartupHooks;
            On.AbstractPhysicalObject.Realize += RealizePatch;
            On.MultiplayerUnlocks.SymbolDataForSandboxUnlock += SandboxIconPatch;
            On.ItemSymbol.SpriteNameForItem += SpriteNameForItemPatch;
            On.ItemSymbol.ColorForItem += ItemSymbol_ColorForItem;
            On.ItemSymbol.SymbolDataFromItem += ItemSymbol_SymbolDataFromItem;

            Debug.Log("Hooking Savestate (we'll probably crash here if we're running more than two patches without BepinEx)");
            On.SaveState.AbstractPhysicalObjectFromString += AbstractFromStringPatch;
            Debug.Log("We haven't crashed yet? Sick.");

            On.RegionState.AdaptWorldToRegionState += CustomRegionLoad;
            On.ArenaGameSession.SpawnItem += ArenaSpawnItemPatch;
            On.SandboxGameSession.SpawnEntity += SpawnEntityPatch;

            On.RainWorld.Start += RainWorld_Start;

            PlayerHooks.Patch();
            DataManager.Patch();
            ItemSpawner.Patch();
            MoonHooks.Patch();

            Debug.Log("PrimitiveArmory Hooking Complete!");
        }

        private string SpriteNameForItemPatch(On.ItemSymbol.orig_SpriteNameForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
        {
            if (itemType == EnumExt_NewItems.Club)
            {
                return "Symbol_Club";
            }
            
            if (itemType == EnumExt_NewItems.Bow)
            {
                return "Symbol_Bow";
            }

            if (itemType == EnumExt_NewItems.Arrow)
            {
                return intData switch
                {
                    0 => "Symbol_Arrow",
                    1 => "Symbol_FireArrow",
                    2 => "Symbol_ExplosiveArrow",
                    _ => "Symbol_Arrow"
                };
            }

            return orig.Invoke(itemType, intData);
        }

        private Color ItemSymbol_ColorForItem(On.ItemSymbol.orig_ColorForItem orig, AbstractPhysicalObject.AbstractObjectType itemType, int intData)
        {


            return orig(itemType, intData);
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

            if (self.type == EnumExt_NewItems.Bow)
            {
                self.realizedObject = new Bow(self, self.world);

                goto StuckRealize;
            }

            if (self.type == EnumExt_NewItems.Arrow)
            {
                self.realizedObject = new Arrow(self, self.world);

                goto StuckRealize;
            }

            orig(self);
            return;

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

            return;
        }

        private void ArenaSpawnItemPatch(On.ArenaGameSession.orig_SpawnItem orig, ArenaGameSession self, Room room, PlacedObject placedObj)
        {
            AbstractPhysicalObject.AbstractObjectType abstractObjectType = AbstractPhysicalObject.AbstractObjectType.Rock;
            int arrowType = 0;
            if ((placedObj.data as PlacedObject.MultiplayerItemData).type == EnumExt_NewItems.ArrowData)
            {
                abstractObjectType = EnumExt_NewItems.Arrow;
                arrowType = 0;
            }
            if ((placedObj.data as PlacedObject.MultiplayerItemData).type == EnumExt_NewItems.FireArrowData)
            {
                abstractObjectType = EnumExt_NewItems.Arrow;
                arrowType = 1;
            }
            if ((placedObj.data as PlacedObject.MultiplayerItemData).type == EnumExt_NewItems.ExplosiveArrowData)
            {
                abstractObjectType = EnumExt_NewItems.Arrow;
                arrowType = 2;
            }
            if ((placedObj.data as PlacedObject.MultiplayerItemData).type == EnumExt_NewItems.ElectricArrowData)
            {
                abstractObjectType = EnumExt_NewItems.Arrow;
                arrowType = 3;
            }
            if ((placedObj.data as PlacedObject.MultiplayerItemData).type == EnumExt_NewItems.FlashArrowData)
            {
                abstractObjectType = EnumExt_NewItems.Arrow;
                arrowType = 4;
            }

            if (abstractObjectType == EnumExt_NewItems.Arrow)
            {
                Arrow.AbstractArrow item = new Arrow.AbstractArrow(room.world, null, room.GetWorldCoordinate(placedObj.pos), self.game.GetNewID(), arrowType);
                room.abstractRoom.entities.Add(item);
                return;
            }

            orig(self, room, placedObj);
        }

        private void SpawnEntityPatch(On.SandboxGameSession.orig_SpawnEntity orig, SandboxGameSession gameSession, ArenaBehaviors.SandboxEditor.PlacedIconData placedIconData)
        {
            IconSymbol.IconSymbolData data = placedIconData.data;
            WorldCoordinate pos = new WorldCoordinate(0, -1, -1, -1);
            pos.x = Mathf.RoundToInt(placedIconData.pos.x / 20f);
            pos.y = Mathf.RoundToInt(placedIconData.pos.y / 20f);
            EntityID entityID = (!gameSession.GameTypeSetup.saveCreatures) ? gameSession.game.GetNewID() : placedIconData.ID;

            if (data.itemType == EnumExt_NewItems.Arrow)
            {
                gameSession.game.world.GetAbstractRoom(0).AddEntity(new Arrow.AbstractArrow(gameSession.game.world, null, pos, entityID, data.intData));

                return;
            }

            orig(gameSession, placedIconData);
        }

        private static IconSymbol.IconSymbolData SandboxIconPatch(On.MultiplayerUnlocks.orig_SymbolDataForSandboxUnlock orig, MultiplayerUnlocks.SandboxUnlockID unlockID)
        {
            if (unlockID == EnumExt_NewItems.ClubUnlock)
            {
                return new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, EnumExt_NewItems.Club, 0);
            }

            if (unlockID == EnumExt_NewItems.BowUnlock)
            {
                return new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, EnumExt_NewItems.Bow, 0);
            }

            if (unlockID == EnumExt_NewItems.ArrowUnlock)
            {
                return new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, EnumExt_NewItems.Arrow, 0);
            }

            return orig(unlockID);
        }

        private IconSymbol.IconSymbolData? ItemSymbol_SymbolDataFromItem(On.ItemSymbol.orig_SymbolDataFromItem orig, AbstractPhysicalObject item)
        {
            AbstractPhysicalObject.AbstractObjectType type = item.type;

            if (type == EnumExt_NewItems.Arrow)
            {
                if ((item as Arrow.AbstractArrow).stuckInWall)
                {
                    return null;
                }
                return new IconSymbol.IconSymbolData?(new IconSymbol.IconSymbolData(CreatureTemplate.Type.StandardGroundCreature, type, (int)(item as Arrow.AbstractArrow).arrowType));
            }

            return orig(item);
        }

        private AbstractPhysicalObject AbstractFromStringPatch(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
        {
            Debug.Log(objString);

            AbstractPhysicalObject result = LoadCustomItem(world, objString);

            if (result != null)
            {
                return result;
            }
            else
            {
                return orig(world, objString);
            }
        }

        public void StartupHooks(On.RainWorld.orig_Update orig, RainWorld self)
        {
            orig(self);

            if (!startupHooks)
            {
                startupHooks = true;
                AddItems();
                DynamicHooks.SearchAndAdd();
            }
        }

        private void AddItems()
        {
            SandboxUnlockCore.Main.items.Add(EnumExt_NewItems.ClubUnlock);
            SandboxUnlockCore.Main.items.Add(EnumExt_NewItems.BowUnlock);
            SandboxUnlockCore.Main.items.Add(EnumExt_NewItems.ArrowUnlock);
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
            Debug.Log(objString);
            try
            {
                string[] objectData = Regex.Split(objString, "<oA>");
                EntityID ID = EntityID.FromString(objectData[0]);
                AbstractPhysicalObject.AbstractObjectType abstractObjectType = Custom.ParseEnum<AbstractPhysicalObject.AbstractObjectType>(objectData[1]);
                WorldCoordinate pos = new WorldCoordinate(int.Parse(objectData[2].Split('.')[0]), int.Parse(objectData[2].Split('.')[1]), int.Parse(objectData[2].Split('.')[2]), int.Parse(objectData[2].Split('.')[3]));

                if (abstractObjectType == EnumExt_NewItems.Arrow)
                {
                    Vector2 rotation = new Vector2(int.Parse(objectData[5].Split('.')[0]), int.Parse(objectData[5].Split('.')[1]));
                    Arrow.AbstractArrow abstractArrow = new Arrow.AbstractArrow(world, null, pos, ID, int.Parse(objectData[4]));
                    abstractArrow.rotation = rotation;
                    abstractArrow.stuckInWallCycles = int.Parse(objectData[3]);
                    return abstractArrow;
                }
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