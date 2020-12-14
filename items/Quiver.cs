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

        public Vector2[,] strap;

        public float conRad = 7f;

        public class AbstractQuiver : AbstractPhysicalObject
        {
            public int[] quiverContents;

            public static int quiverSize = 5;

            public AbstractQuiver(World world, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int[] quiverContents = null) : base(world, EnumExt_NewItems.Quiver, realizedObject, pos, ID)
            {
                if (quiverContents == null)
                {
                    this.quiverContents = new int[quiverSize];

                    foreach (int i in this.quiverContents)
                    {
                        this.quiverContents[i] = -1;
                    }
                }
                else
                {
                    this.quiverContents = quiverContents;
                }
            }
        }

        public void ResetStrap()
        {
            Vector2 vector = StrapAttachPos(1f);
            for (int i = 0; i < strap.GetLength(0); i++)
            {
                strap[i, 0] = vector;
                strap[i, 1] = vector;
                strap[i, 2] *= 0f;
            }
        }

        public Vector2 StrapAttachPos(float timeStacker)
        {
            return Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker) + (Vector2)Vector3.Slerp(lastRotation, rotation, timeStacker) * 15f;
        }

        public Quiver(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
            base.bodyChunks = new BodyChunk[1];
            base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 2.5f, 0.05f);
            bodyChunkConnections = new BodyChunkConnection[0];
            base.airFriction = 0.999f;
            base.gravity = 0.9f;
            bounce = 0.4f;
            surfaceFriction = 0.4f;
            collisionLayer = 2;
            base.waterFriction = 0.98f;
            base.buoyancy = 1.0f;
            base.firstChunk.loudness = 7f;
            tailPos = base.firstChunk.pos;
            soundLoop = new ChunkDynamicSoundLoop(base.firstChunk);
            base.armorMode = ArmorMode.Free;
            base.armorSlot = ArmorSlot.Accessory;
            strap = new Vector2[UnityEngine.Random.Range(4, UnityEngine.Random.Range(4, 10)), 6];
        }

        public override void Update(bool eu)
        {
            base.Update(eu);

            switch (base.mode)
            {
                case Mode.Free:
                    if (spinning)
                    {
                        if (Custom.DistLess(base.firstChunk.pos, base.firstChunk.lastPos, 4f * room.gravity))
                        {
                            stillCounter++;
                        }
                        else
                        {
                            stillCounter = 0;
                        }
                        if (base.firstChunk.ContactPoint.y < -10.0f || stillCounter > 20)
                        {
                            spinning = false;
                            rotationSpeed = 0f;
                            rotation = Custom.DegToVec(Mathf.Lerp(-90, 90f, Random.value) + 180f);
                            base.firstChunk.vel *= 0f;
                            room.PlaySound(SoundID.Spear_Stick_In_Ground, base.firstChunk);
                        }
                    }
                    else if (!Custom.DistLess(base.firstChunk.lastPos, base.firstChunk.pos, 6f))
                    {
                        SetRandomSpin();
                    }
                    break;
                case Mode.Thrown:
                    SetRandomSpin();
                    soundLoop.sound = SoundID.Spear_Spinning_Through_Air_LOOP;
                    soundLoop.Volume = Mathf.InverseLerp(5f, 15f, base.firstChunk.vel.magnitude);
                    break;
            }
            for (int i = 0; i < strap.GetLength(0); i++)
            {
                float t = (float)i / (float)(strap.GetLength(0) - 1);
                strap[i, 1] = strap[i, 0];
                strap[i, 0] += strap[i, 2];
                strap[i, 2] -= rotation * Mathf.InverseLerp(1f, 0f, i) * 0.8f;
                strap[i, 4] = strap[i, 3];
                strap[i, 3] = (strap[i, 3] + strap[i, 5] * Custom.LerpMap(Vector2.Distance(strap[i, 0], strap[i, 1]), 1f, 18f, 0.05f, 0.3f)).normalized;
                strap[i, 5] = (strap[i, 5] + Custom.RNV() * UnityEngine.Random.value * Mathf.Pow(Mathf.InverseLerp(1f, 18f, Vector2.Distance(strap[i, 0], strap[i, 1])), 0.3f)).normalized;
                if (room.PointSubmerged(strap[i, 0]))
                {
                    strap[i, 2] *= Custom.LerpMap(strap[i, 2].magnitude, 1f, 10f, 1f, 0.5f, Mathf.Lerp(1.4f, 0.4f, t));
                    strap[i, 2].y += 0.05f;
                    strap[i, 2] += Custom.RNV() * 0.1f;
                    continue;
                }
                strap[i, 2] *= Custom.LerpMap(Vector2.Distance(strap[i, 0], strap[i, 1]), 1f, 6f, 0.999f, 0.7f, Mathf.Lerp(1.5f, 0.5f, t));
                strap[i, 2].y -= room.gravity * Custom.LerpMap(Vector2.Distance(strap[i, 0], strap[i, 1]), 1f, 6f, 0.6f, 0f);
                if (i % 3 == 2 || i == strap.GetLength(0) - 1)
                {
                    SharedPhysics.TerrainCollisionData cd = new SharedPhysics.TerrainCollisionData(strap[i, 0], strap[i, 1], strap[i, 2], 1f, new IntVector2(0, 0), goThroughFloors: false);
                    cd = SharedPhysics.HorizontalCollision(room, cd);
                    cd = SharedPhysics.VerticalCollision(room, cd);
                    cd = SharedPhysics.SlopesVertically(room, cd);
                    strap[i, 0] = cd.pos;
                    strap[i, 2] = cd.vel;
                    if (cd.contactPoint.x != 0)
                    {
                        strap[i, 2].y *= 0.6f;
                    }
                    if (cd.contactPoint.y != 0)
                    {
                        strap[i, 2].x *= 0.6f;
                    }
                }
            }
            for (int j = 0; j < strap.GetLength(0); j++)
            {
                if (j > 0)
                {
                    Vector2 normalized = (strap[j, 0] - strap[j - 1, 0]).normalized;
                    float num = Vector2.Distance(strap[j, 0], strap[j - 1, 0]);
                    float d = ((!(num > conRad)) ? 0.25f : 0.5f);
                    strap[j, 0] += normalized * (conRad - num) * d;
                    strap[j, 2] += normalized * (conRad - num) * d;
                    strap[j - 1, 0] -= normalized * (conRad - num) * d;
                    strap[j - 1, 2] -= normalized * (conRad - num) * d;
                    if (j > 1)
                    {
                        normalized = (strap[j, 0] - strap[j - 2, 0]).normalized;
                        strap[j, 2] += normalized * 0.2f;
                        strap[j - 2, 2] -= normalized * 0.2f;
                    }
                    if (j < strap.GetLength(0) - 1)
                    {
                        strap[j, 3] = Vector3.Slerp(strap[j, 3], (strap[j - 1, 3] * 2f + strap[j + 1, 3]) / 3f, 0.1f);
                        strap[j, 5] = Vector3.Slerp(strap[j, 5], (strap[j - 1, 5] * 2f + strap[j + 1, 5]) / 3f, Custom.LerpMap(Vector2.Distance(strap[j, 1], strap[j, 0]), 1f, 8f, 0.05f, 0.5f));
                    }
                }
                else
                {
                    strap[j, 0] = StrapAttachPos(1f);
                    strap[j, 2] *= 0f;
                }
            }
        }

        public override void SetRandomSpin()
        {
            if (room != null)
            {
                rotationSpeed = ((!(Random.value < 0.5f)) ? 1f : (-1f)) * Mathf.Lerp(50f, 150f, Random.value) * Mathf.Lerp(0.05f, 1f, room.gravity);
            }
            spinning = true;
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[3];

            sLeaser.sprites[1] = new FSprite("Quiver")
            {
                scale = 1.25f
            };
            sLeaser.sprites[0] = new FSprite("QuiverFade")
            {
                scale = 1.25f
            };
            sLeaser.sprites[2] = TriangleMesh.MakeLongMesh(strap.GetLength(0), false, false);
            sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["JaggedSquare"];
            sLeaser.sprites[2].alpha = rCam.game.SeededRandom(abstractPhysicalObject.ID.RandomSeed);

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

            sLeaser.sprites[2].color = color;
            
            float num = 0f;
            Vector2 strapAttachPos = StrapAttachPos(timeStacker);
            for (int i = 0; i < strap.GetLength(0); i++)
            {
                float f = (float)i / (float)(strap.GetLength(0) - 1);
                Vector2 strapFollow = Vector2.Lerp(strap[i, 1], strap[i, 0], timeStacker);
                float num2 = (2f + 2f * Mathf.Sin(Mathf.Pow(f, 2f) * (float)System.Math.PI)) * Vector3.Slerp(strap[i, 4], strap[i, 3], timeStacker).x;
                Vector2 normalizedStrap = (strapAttachPos - strapFollow).normalized;
                Vector2 perpStrap = Custom.PerpendicularVector(normalizedStrap);
                float diff = Vector2.Distance(strapAttachPos, strapFollow) / 5f;
                (sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4, strapAttachPos - normalizedStrap * diff - perpStrap * (num2 + num) * 0.5f - camPos);
                (sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 1, strapAttachPos - normalizedStrap * diff + perpStrap * (num2 + num) * 0.5f - camPos);
                (sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 2, strapFollow + normalizedStrap * diff - perpStrap * num2 - camPos);
                (sLeaser.sprites[2] as TriangleMesh).MoveVertice(i * 4 + 3, strapFollow + normalizedStrap * diff + perpStrap * num2 - camPos);
                strapAttachPos = vector;
                num = num2;
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
