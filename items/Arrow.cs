using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;
using UnityEngine;

namespace PrimitiveArmory
{
    public class Arrow : Weapon, IDrawable
	{
		public bool spinning;

		public bool pivotAtTip;

		public bool lastPivotAtTip;

		public PhysicalObject stuckInObject;

		public int stuckInChunkIndex;

		public Appendage.Pos stuckInAppendage;

		public float stuckRotation;

		public Vector2? stuckInWall;

		public int stuckBodyPart;

		public bool alwaysStickInWalls;

		public int pinToWallCounter;

		public bool addPoles;

		public float arrowDamageBonus = 1f;

		public int stillCounter;

		public AbstractArrow abstractArrow => abstractPhysicalObject as AbstractArrow;

		public class AbstractArrow : AbstractPhysicalObject
        {
			public enum ArrowType
            {
				Normal,
				Fire,
				Explosive,
				Electric,
				Flash
            }

			public int stuckInWallCycles;

			public ArrowType arrowType;

			public bool stuckVertically;

			public bool stuckInWall => stuckInWallCycles != 0;

			public AbstractArrow(World world, Arrow realizedObject, WorldCoordinate pos, EntityID ID, int arrowType = 0)
				: base(world, EnumExt_NewItems.Arrow, realizedObject, pos, ID)
			{
                this.arrowType = arrowType switch
                {
                    0 => ArrowType.Normal,
                    1 => ArrowType.Fire,
                    2 => ArrowType.Explosive,
                    3 => ArrowType.Electric,
					4 => ArrowType.Flash,
                    _ => ArrowType.Normal,
                };
            }
			public void StuckInWallTick(int ticks)
			{
				if (stuckInWallCycles > 0)
				{
					stuckInWallCycles = System.Math.Max(0, stuckInWallCycles - ticks);
				}
				else if (stuckInWallCycles < 0)
				{
					stuckInWallCycles = System.Math.Min(0, stuckInWallCycles + ticks);
				}
			}

			public override string ToString()
			{
				return ID.ToString() + "<oA>" + type.ToString() + "<oA>" + pos.room + "." + pos.x + "." + pos.y + "." + pos.abstractNode + "<oA>" + stuckInWallCycles.ToString() + "<oA>" + arrowType.ToString();
			}
		}

		public BodyChunk stuckInChunk => stuckInObject.bodyChunks[stuckInChunkIndex];

		public override bool HeavyWeapon => false;

		public Arrow(AbstractPhysicalObject abstractPhysicalObject, World world)
			: base(abstractPhysicalObject, world)
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
			pivotAtTip = false;
			lastPivotAtTip = false;
			stuckBodyPart = -1;
			base.firstChunk.loudness = 7f;
			tailPos = base.firstChunk.pos;
			soundLoop = new ChunkDynamicSoundLoop(base.firstChunk);
		}

        public override void Update(bool eu)
        {
            base.Update(eu);
			soundLoop.sound = SoundID.None;
			if (base.firstChunk.vel.magnitude > 5f)
			{
				if (base.mode == Mode.Thrown)
				{
					soundLoop.sound = SoundID.Spear_Thrown_Through_Air_LOOP;
				}
				else if (base.mode == Mode.Free)
				{
					soundLoop.sound = SoundID.Spear_Spinning_Through_Air_LOOP;
				}
				soundLoop.Volume = Mathf.InverseLerp(5f, 15f, base.firstChunk.vel.magnitude);
			}
			soundLoop.Update();
			lastPivotAtTip = pivotAtTip;
			pivotAtTip = base.mode == Mode.Thrown || base.mode == Mode.StuckInCreature;
			if (addPoles && room.readyForAI)
			{
				if (abstractArrow.stuckInWallCycles >= 0)
				{
					room.GetTile(stuckInWall.Value).horizontalBeam = true;
					for (int i = -1; i < 2; i += 2)
					{
						if (!room.GetTile(stuckInWall.Value + new Vector2(20f * (float)i, 0f)).Solid)
						{
							room.GetTile(stuckInWall.Value + new Vector2(20f * (float)i, 0f)).horizontalBeam = true;
						}
					}
				}
				else
				{
					room.GetTile(stuckInWall.Value).verticalBeam = true;
					for (int j = -1; j < 2; j += 2)
					{
						if (!room.GetTile(stuckInWall.Value + new Vector2(0f, 20f * (float)j)).Solid)
						{
							room.GetTile(stuckInWall.Value + new Vector2(0f, 20f * (float)j)).verticalBeam = true;
						}
					}
				}
				addPoles = false;
			}

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
					{
						base.firstChunk.vel.y += 0.45f;
						if (!Custom.DistLess(thrownPos, base.firstChunk.pos, 560f * Mathf.Max(1f, arrowDamageBonus)) || !(base.firstChunk.ContactPoint == throwDir) || room.GetTile(base.firstChunk.pos).Terrain != 0 || room.GetTile(base.firstChunk.pos + throwDir.ToVector2() * 20f).Terrain != Room.Tile.TerrainType.Solid || ((!Custom.DistLess(thrownPos, base.firstChunk.pos, 140f) && !alwaysStickInWalls))) 
						{
							break;
						}
						bool flag = true;
						foreach (AbstractWorldEntity entity in room.abstractRoom.entities)
						{
							if (entity is AbstractArrow && (entity as AbstractArrow).realizedObject != null && ((entity as AbstractArrow).realizedObject as Weapon).mode == Mode.StuckInWall && entity.pos.Tile == abstractPhysicalObject.pos.Tile)
							{
								flag = false;
								break;
							}
						}
						if (flag)
						{
							for (int k = 0; k < room.roomSettings.placedObjects.Count; k++)
							{
								if (room.roomSettings.placedObjects[k].type == PlacedObject.Type.NoSpearStickZone && Custom.DistLess(room.MiddleOfTile(base.firstChunk.pos), room.roomSettings.placedObjects[k].pos, (room.roomSettings.placedObjects[k].data as PlacedObject.ResizableObjectData).Rad))
								{
									flag = false;
									break;
								}
							}
						}
						if (flag)
						{
							stuckInWall = room.MiddleOfTile(base.firstChunk.pos);
							vibrate = 10;
							ChangeMode(Mode.StuckInWall);
							room.PlaySound(SoundID.Spear_Stick_In_Wall, base.firstChunk);
							base.firstChunk.collideWithTerrain = false;
						}
						break;
					}
				case Mode.StuckInCreature:
					if (!stuckInWall.HasValue)
					{
						if (stuckInAppendage != null)
						{
							setRotation = Custom.DegToVec(stuckRotation + Custom.VecToDeg(stuckInAppendage.appendage.OnAppendageDirection(stuckInAppendage)));
							base.firstChunk.pos = stuckInAppendage.appendage.OnAppendagePosition(stuckInAppendage);
						}
						else
						{
							base.firstChunk.vel = stuckInChunk.vel;
							if (stuckBodyPart == -1 || !room.BeingViewed || (stuckInChunk.owner as Creature).BodyPartByIndex(stuckBodyPart) == null)
							{
								setRotation = Custom.DegToVec(stuckRotation + Custom.VecToDeg(stuckInChunk.Rotation));
								base.firstChunk.MoveWithOtherObject(eu, stuckInChunk, new Vector2(0f, 0f));
							}
							else
							{
								setRotation = Custom.DegToVec(stuckRotation + Custom.AimFromOneVectorToAnother(stuckInChunk.pos, (stuckInChunk.owner as Creature).BodyPartByIndex(stuckBodyPart).pos));
								base.firstChunk.MoveWithOtherObject(eu, stuckInChunk, Vector2.Lerp(stuckInChunk.pos, (stuckInChunk.owner as Creature).BodyPartByIndex(stuckBodyPart).pos, 0.5f) - stuckInChunk.pos);
							}
						}
					}
					else
					{
						if (pinToWallCounter > 0)
						{
							pinToWallCounter--;
						}
						if (stuckInChunk.vel.magnitude * stuckInChunk.mass > Custom.LerpMap(pinToWallCounter, 160f, 0f, 7f, 2f))
						{
							setRotation = (Custom.DegToVec(stuckRotation) + Vector2.ClampMagnitude(stuckInChunk.vel * stuckInChunk.mass * 0.005f, 0.1f)).normalized;
						}
						else
						{
							setRotation = Custom.DegToVec(stuckRotation);
						}
						base.firstChunk.vel *= 0f;
						base.firstChunk.pos = stuckInWall.Value;
						if ((stuckInChunk.owner is Creature && (stuckInChunk.owner as Creature).enteringShortCut.HasValue) || (pinToWallCounter < 160 && Random.value < 0.025f && stuckInChunk.vel.magnitude > Custom.LerpMap(pinToWallCounter, 160f, 0f, 140f, 30f / (1f + stuckInChunk.owner.TotalMass * 0.2f))))
						{
							stuckRotation = Custom.Angle(setRotation.Value, stuckInChunk.Rotation);
							stuckInWall = null;
						}
						else
						{
							stuckInChunk.MoveFromOutsideMyUpdate(eu, stuckInWall.Value);
							stuckInChunk.vel *= 0f;
						}
					}
					if (stuckInChunk.owner.slatedForDeletetion)
					{
						ChangeMode(Mode.Free);
					}
					break;
				case Mode.StuckInWall:
					base.firstChunk.pos = stuckInWall.Value;
					base.firstChunk.vel *= 0f;
					break;

			}

			for (int num = abstractPhysicalObject.stuckObjects.Count - 1; num >= 0; num--)
			{
				if (abstractPhysicalObject.stuckObjects[num] is AbstractPhysicalObject.ImpaledOnSpearStick)
				{
					if (abstractPhysicalObject.stuckObjects[num].B.realizedObject != null && (abstractPhysicalObject.stuckObjects[num].B.realizedObject.slatedForDeletetion || abstractPhysicalObject.stuckObjects[num].B.realizedObject.grabbedBy.Count > 0))
					{
						abstractPhysicalObject.stuckObjects[num].Deactivate();
					}
					else if (abstractPhysicalObject.stuckObjects[num].B.realizedObject != null && abstractPhysicalObject.stuckObjects[num].B.realizedObject.room == room)
					{
						abstractPhysicalObject.stuckObjects[num].B.realizedObject.firstChunk.MoveFromOutsideMyUpdate(eu, base.firstChunk.pos + rotation * Custom.LerpMap((abstractPhysicalObject.stuckObjects[num] as AbstractPhysicalObject.ImpaledOnSpearStick).onSpearPosition, 0f, 4f, 15f, -15f));
						abstractPhysicalObject.stuckObjects[num].B.realizedObject.firstChunk.vel *= 0f;
					}
				}
			}
		}

		public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
		{
			if (result.obj == null)
			{
				return false;
			}
			bool flag = false;
			if (abstractPhysicalObject.world.game.IsArenaSession && abstractPhysicalObject.world.game.GetArenaGameSession.GameTypeSetup.spearHitScore != 0 && thrownBy != null && thrownBy is Player && result.obj is Creature)
			{
				flag = true;
				if ((result.obj as Creature).State is HealthState && ((result.obj as Creature).State as HealthState).health <= 0f)
				{
					flag = false;
				}
				else if (!((result.obj as Creature).State is HealthState) && (result.obj as Creature).State.dead)
				{
					flag = false;
				}
			}
			if (result.obj is Creature)
			{
				(result.obj as Creature).Violence(base.firstChunk, base.firstChunk.vel * base.firstChunk.mass * 2f, result.chunk, result.onAppendagePos, Creature.DamageType.Stab, arrowDamageBonus, 20f);
			}
			else if (result.chunk != null)
			{
				result.chunk.vel += base.firstChunk.vel * base.firstChunk.mass / result.chunk.mass;
			}
			else if (result.onAppendagePos != null)
			{
				(result.obj as IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, base.firstChunk.vel * base.firstChunk.mass);
			}
			if (result.obj is Creature && (result.obj as Creature).SpearStick(this, Mathf.Lerp(0.55f, 0.62f, Random.value), result.chunk, result.onAppendagePos, base.firstChunk.vel))
			{
				room.PlaySound(SoundID.Spear_Stick_In_Creature, base.firstChunk);
				LodgeInCreature(result, eu);
				if (flag)
				{
					abstractPhysicalObject.world.game.GetArenaGameSession.PlayerLandSpear(thrownBy as Player, stuckInObject as Creature);
				}
				return true;
			}
			room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, base.firstChunk);
			vibrate = 20;
			ChangeMode(Mode.Free);
			base.firstChunk.vel = base.firstChunk.vel * -0.5f + Custom.DegToVec(Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, Random.value) * base.firstChunk.vel.magnitude;
			SetRandomSpin();
			return false;
		}

		public override void RecreateSticksFromAbstract()
		{
			for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
			{
				if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).Spear == abstractPhysicalObject && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).LodgedIn.realizedObject != null)
				{
					AbstractPhysicalObject.AbstractSpearStick abstractSpearStick = abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick;
					stuckInObject = abstractSpearStick.LodgedIn.realizedObject;
					stuckInChunkIndex = abstractSpearStick.chunk;
					stuckBodyPart = abstractSpearStick.bodyPart;
					stuckRotation = abstractSpearStick.angle;
					ChangeMode(Mode.StuckInCreature);
				}
				else if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearAppendageStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).Spear == abstractPhysicalObject && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).LodgedIn.realizedObject != null)
				{
					AbstractPhysicalObject.AbstractSpearAppendageStick abstractSpearAppendageStick = abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick;
					stuckInObject = abstractSpearAppendageStick.LodgedIn.realizedObject;
					stuckInAppendage = new Appendage.Pos(stuckInObject.appendages[abstractSpearAppendageStick.appendage], abstractSpearAppendageStick.prevSeg, abstractSpearAppendageStick.distanceToNext);
					stuckRotation = abstractSpearAppendageStick.angle;
					ChangeMode(Mode.StuckInCreature);
				}
			}
		}

		public void LodgeInCreature(SharedPhysics.CollisionResult result, bool eu)
		{
			stuckInObject = result.obj;
			ChangeMode(Mode.StuckInCreature);
			if (result.chunk != null)
			{
				stuckInChunkIndex = result.chunk.index;
				if (arrowDamageBonus > 0.9f && room.GetTile(room.GetTilePosition(stuckInChunk.pos) + throwDir).Terrain == Room.Tile.TerrainType.Solid && room.GetTile(stuckInChunk.pos).Terrain == Room.Tile.TerrainType.Air)
				{
					stuckInWall = room.MiddleOfTile(stuckInChunk.pos) + throwDir.ToVector2() * (10f - stuckInChunk.rad);
					stuckInChunk.MoveFromOutsideMyUpdate(eu, stuckInWall.Value);
					stuckRotation = Custom.VecToDeg(rotation);
					stuckBodyPart = -1;
					pinToWallCounter = 300;
				}
				else if (stuckBodyPart == -1)
				{
					stuckRotation = Custom.Angle(throwDir.ToVector2(), stuckInChunk.Rotation);
				}
				base.firstChunk.MoveWithOtherObject(eu, stuckInChunk, new Vector2(0f, 0f));
				Debug.Log("Add spear to creature chunk " + stuckInChunk.index);
				new AbstractPhysicalObject.AbstractSpearStick(abstractPhysicalObject, (result.obj as Creature).abstractCreature, stuckInChunkIndex, stuckBodyPart, stuckRotation);
			}
			else if (result.onAppendagePos != null)
			{
				stuckInChunkIndex = 0;
				stuckInAppendage = result.onAppendagePos;
				stuckRotation = Custom.VecToDeg(rotation) - Custom.VecToDeg(stuckInAppendage.appendage.OnAppendageDirection(stuckInAppendage));
				Debug.Log("Add spear to creature Appendage");
				new AbstractPhysicalObject.AbstractSpearAppendageStick(abstractPhysicalObject, (result.obj as Creature).abstractCreature, result.onAppendagePos.appendage.appIndex, result.onAppendagePos.prevSegment, result.onAppendagePos.distanceToNext, stuckRotation);
			}
			if (room.BeingViewed)
			{
				for (int i = 0; i < 8; i++)
				{
					room.AddObject(new WaterDrip(result.collisionPoint, -base.firstChunk.vel * Random.value * 0.5f + Custom.DegToVec(360f * Random.value) * base.firstChunk.vel.magnitude * Random.value * 0.5f, waterColor: false));
				}
			}
		}

		public virtual void TryImpaleSmallCreature(Creature smallCrit)
		{
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
			{
				if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.ImpaledOnSpearStick)
				{
					if ((abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.ImpaledOnSpearStick).onSpearPosition == num2)
					{
						num2++;
					}
					num++;
				}
			}
			if (num <= 5 && num2 < 5)
			{
				new AbstractPhysicalObject.ImpaledOnSpearStick(abstractPhysicalObject, smallCrit.abstractCreature, 0, num2);
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Arrow")
			{
				scale = 1.1f
			};
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
			if (vibrate > 0)
			{
				vector += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
			}
			Vector3 v = Vector3.Slerp(lastRotation, rotation, timeStacker);
			for (int i = 0; i >= 0; i--)
			{
				sLeaser.sprites[i].x = vector.x - camPos.x;
				sLeaser.sprites[i].y = vector.y - camPos.y;
				sLeaser.sprites[i].anchorY = Mathf.Lerp((!lastPivotAtTip) ? 0.5f : 0.85f, (!pivotAtTip) ? 0.5f : 0.85f, timeStacker);
				sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), v);
			}
			if (blink > 0 && Random.value < 0.5f)
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
	}
}
