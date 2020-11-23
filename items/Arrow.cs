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
				Fire,
				Explosive,
				Electric
            }

			public int stuckInWallCycles;

			public ArrowType arrowType;

			public bool stuckVertically;

			public bool stuckInWall => stuckInWallCycles != 0;

			public AbstractArrow(World world, Spear realizedObject, WorldCoordinate pos, EntityID ID, ArrowType arrowType)
				: base(world, EnumExt_NewItems.Arrow, realizedObject, pos, ID)
			{
				this.arrowType = arrowType;
			}
		}

		public BodyChunk stuckInChunk => stuckInObject.bodyChunks[stuckInChunkIndex];

		public override bool HeavyWeapon => true;

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

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Arrow")
			{
				scale = 1.5f
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
