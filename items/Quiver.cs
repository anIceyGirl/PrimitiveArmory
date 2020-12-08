using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;

namespace PrimitiveArmory
{
    public class Quiver : Armor
    {
        public Color fadeColor;

        public class AbstractQuiver : AbstractPhysicalObject
        {
            public int[] quiverContents = new int[6];

            public AbstractQuiver(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID) : base(world, EnumExt_NewItems.Quiver, realizedObject, pos, ID)
            {

            }
        }

        public Quiver(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
            base.bodyChunks = new BodyChunk[1];
            base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
            bodyChunkConnections = new BodyChunkConnection[0];
            base.airFriction = 0.999f;
            base.gravity = 0.9f;
            bounce = 0.4f;
            surfaceFriction = 0.4f;
            collisionLayer = 2;
            base.waterFriction = 0.98f;
            base.buoyancy = 0.4f;
            base.firstChunk.loudness = 7f;
            tailPos = base.firstChunk.pos;
            soundLoop = new ChunkDynamicSoundLoop(base.firstChunk);
            base.armorMode = ArmorMode.Free;
            base.armorSlot = ArmorSlot.Accessory;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];

            sLeaser.sprites[1] = new FSprite("Quiver")
            {
                scale = 1.25f
            };
            sLeaser.sprites[0] = new FSprite("QuiverFade")
            {
                scale = 1.25f
            };

            AddToContainer(sLeaser, rCam, null);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
            if (vibrate > 0)
            {
                vector += Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f * UnityEngine.Random.value;
            }
            Vector3 v = Vector3.Slerp(lastRotation, rotation, timeStacker);
            for (int i = 1; i >= 0; i--)
            {
                sLeaser.sprites[i].x = vector.x - camPos.x;
                sLeaser.sprites[i].y = vector.y - camPos.y;
                sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), v);
            }

            if (blink > 0 && UnityEngine.Random.value < 0.5f)
            {
                sLeaser.sprites[1].color = base.blinkColor;
                sLeaser.sprites[0].color = base.blinkColor;
            }
            else
            {
                sLeaser.sprites[1].color = color;
                sLeaser.sprites[0].color = fadeColor;
            }

            if (base.slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            color = palette.blackColor;
            fadeColor = Color.Lerp(new Color(1f, 0.05f, 0.04f), palette.blackColor, 0.1f + 0.8f * palette.darkness);
            sLeaser.sprites[1].color = color;
            sLeaser.sprites[0].color = fadeColor;
        }
    }
}
