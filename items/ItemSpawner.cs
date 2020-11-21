using OptionalUI;
using Partiality;
using Partiality.Modloader;
using RWCustom;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PrimitiveArmory
{
    public class ItemSpawner
    {
        public static float clubReplaceChance = 0.1f;

        public struct RoomStates
        {

        }

        public static void Patch()
        {
            On.Room.ctor += RoomConstructorPatch;
            On.Room.Loaded += RoomLoadedPatch;
        }

        private static void RoomConstructorPatch(On.Room.orig_ctor orig, Room room, RainWorldGame game, World world, AbstractRoom abstractRoom)
        {
            orig(room, game, world, abstractRoom);
        }

        private static void RoomLoadedPatch(On.Room.orig_Loaded orig, Room room)
        {
            if (room.abstractRoom.firstTimeRealized)
			{
				if (!room.abstractRoom.shelter && !room.abstractRoom.gate && room.game != null && (!room.game.IsArenaSession || room.game.GetArenaGameSession.GameTypeSetup.levelItems))
				{
					for (int i = (int)((float)room.TileWidth * (float)room.TileHeight * Mathf.Pow(room.roomSettings.RandomItemDensity * room.roomSettings.RandomItemSpearChance * clubReplaceChance, 2f) / 5f); i >= 0; i--)
					{
						IntVector2 spawnTile = room.RandomTile();
						if (!room.GetTile(spawnTile).Solid)
						{
                            bool canSpawnHere = true;
                            for (int j = -1; j < 2; j++)
                            {
                                if (!room.GetTile(spawnTile + new IntVector2(j, -1)).Solid)
                                {
                                    canSpawnHere = false;
                                    break;
                                }
                            }

                            if (canSpawnHere)
                            {
                                EntityID newID = room.game.GetNewID(-room.abstractRoom.index);
                                AbstractPhysicalObject entity = new AbstractPhysicalObject(room.world, EnumExt_NewItems.Club, null, new WorldCoordinate(room.abstractRoom.index, spawnTile.x, spawnTile.y, -1), newID);
                                room.abstractRoom.AddEntity(entity);
                            }
                        }
					}
				}
			}

            orig(room);
        }
    }
}
