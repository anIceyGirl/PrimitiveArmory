using RWCustom;
using UnityEngine;

namespace PrimitiveArmory
{
    public class Bow : Weapon, IDrawable
    {

        public float arrowDrawn;

        public bool spinning;

        public int stillCounter;

        public float drawProgress;

        public override bool HeavyWeapon => true;

        public Bow(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
            base.bodyChunks = new BodyChunk[1];
            base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
            bodyChunkConnections = new BodyChunkConnection[0];
            base.airFriction = 0.999f;
            base.gravity = 0.9f;
            bounce = 0.4f;
            drawProgress = 0.0f;
            surfaceFriction = 0.4f;
            collisionLayer = 2;
            base.waterFriction = 0.98f;
            base.buoyancy = 0.4f;
            spinning = false;
            base.firstChunk.loudness = 7f;
            soundLoop = new ChunkDynamicSoundLoop(base.firstChunk);
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
                        if (base.firstChunk.ContactPoint.y < 0 || stillCounter > 20)
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
        }

        public Vector2 StringAttachPos(float timeStacker)
        {
            return Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker) + (Vector2)Vector3.Slerp(lastRotation, rotation, timeStacker) * 15f;
        }

        public override void SetRandomSpin()
        {
            if (room != null)
            {
                rotationSpeed = ((!(Random.value < 0.5f)) ? 1f : (-1f)) * Mathf.Lerp(50f, 150f, Random.value) * Mathf.Lerp(0.05f, 1f, room.gravity);
            }
            spinning = true;
        }

        #region drawLogic
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[2];

            sLeaser.sprites[0] = new FSprite("Bow")
            {
                scale = 1.1f,
            };
            sLeaser.sprites[1] = new FSprite("BowStringA1")
            {
                scale = 1.0f,
                anchorY = -0.55f
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

            int drawState = Mathf.RoundToInt(Mathf.Lerp(1, 8, drawProgress));
            
            sLeaser.sprites[1].element = Futile.atlasManager.GetElementWithName("BowStringA" + drawState.ToString());

            if (mode != Weapon.Mode.OnBack)
            {
                sLeaser.sprites[0].anchorY = 0.1f;
                sLeaser.sprites[1].anchorY = -0.55f;
            }
            else
            {
                sLeaser.sprites[0].anchorY = 0.5f;
                sLeaser.sprites[1].anchorY = -0.15f;
            }

            if (blink > 0 && UnityEngine.Random.value < 0.5f)
            {
                sLeaser.sprites[0].color = base.blinkColor;
            }
            else
            {
                sLeaser.sprites[0].color = color;
            }

            if (base.slatedForDeletetion || room != rCam.room)
            {
                sLeaser.CleanSpritesAndRemove();
            }
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            color = palette.blackColor;
            sLeaser.sprites[0].color = color;
            sLeaser.sprites[1].color = color;
        }
        #endregion
    }
}
