﻿using RWCustom;
using UnityEngine;

namespace PrimitiveArmory
{
    public class Club : Weapon, IDrawable
    {
        public bool pivotAtTip;

        public bool lastPivotAtTip;

        public bool spinning;

        public int stillCounter;

        public bool isSwinging = false;

        public float clubDamageBonus = 1f;

        public override bool HeavyWeapon => true;

        public Club(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
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
            base.buoyancy = 1.1f;
            spinning = false;
            pivotAtTip = false;
            lastPivotAtTip = false;
            base.firstChunk.loudness = 7f;
            tailPos = base.firstChunk.pos;
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

        public override void SetRandomSpin()
        {
            if (room != null)
            {
                rotationSpeed = ((!(Random.value < 0.5f)) ? 1f : (-1f)) * Mathf.Lerp(50f, 150f, Random.value) * Mathf.Lerp(0.05f, 1f, room.gravity);
            }
            spinning = true;
        }

        public override void PickedUp(Creature upPicker)
        {
            base.PickedUp(upPicker);

            if (upPicker is Player)
            {
                rotation = new Vector2(-0.4f, 0.9f);
            }

            room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, base.firstChunk);
        }

        public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
        {
            base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);

            room.PlaySound(SoundID.Slugcat_Throw_Spear, base.firstChunk);
        }

        #region drawLogic
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[1];

            sLeaser.sprites[0] = new FSprite("Club");

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
            for (int i = 0; i >= 0; i--)
            {
                sLeaser.sprites[i].x = vector.x - camPos.x;
                sLeaser.sprites[i].y = vector.y - camPos.y;
                sLeaser.sprites[i].anchorY = Mathf.Lerp((!lastPivotAtTip) ? 0.5f : 0.85f, (!pivotAtTip) ? 0.5f : 0.85f, timeStacker);
                sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), v);
            }

            if (this.mode != Mode.OnBack && !isSwinging)
            {
                sLeaser.sprites[0].anchorY = 0.275f;
            }
            else if (isSwinging)
            {
                sLeaser.sprites[0].anchorY = 0.1f;
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
        }
        #endregion
    }
}
