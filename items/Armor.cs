using UnityEngine;
using RWCustom;

namespace PrimitiveArmory
{
    public abstract class Armor : PlayerCarryableItem, IDrawable
    {
        public enum ArmorSlot
        {
            Head,
            Body,
            Accessory
        }

        public enum Mode
        {
            Equipped,
            Carried,
            Free
        }

        public Creature equippedBy;

        public Vector2 rotation;
        public Vector2 lastRotation;
        public float rotationSpeed;

        public DynamicSoundLoop soundLoop;

        public Mode mode
        {
            get;
            set;
        }

        public ArmorSlot armorSlot
        {
            get;
            set;
        }

        public bool isEquipped;

        public int inFrontOfObjects;

        public Armor(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject)
        {
            mode = Mode.Free;
            isEquipped = false;
            rotation = Custom.DegToVec(Random.value * 360f);
            lastRotation = rotation;
            inFrontOfObjects = -1;
            equippedBy = null;
            rotationSpeed = 0f;
            inFrontOfObjects = -1;
        }

        public override void NewRoom(Room newRoom)
        {
            base.NewRoom(newRoom);
            inFrontOfObjects = -1;
        }
        public override void Grabbed(Creature.Grasp grasp)
        {
            ChangeMode(Mode.Carried);
            base.Grabbed(grasp);
        }

        public void ChangeOverlap(bool newOverlap)
        {
            if (inFrontOfObjects != (newOverlap ? 1 : 0) && room != null)
            {
                for (int i = 0; i < room.game.cameras.Length; i++)
                {
                    room.game.cameras[i].MoveObjectToContainer(this, room.game.cameras[i].ReturnFContainer((!newOverlap) ? "Background" : "Items"));
                }
                inFrontOfObjects = (newOverlap ? 1 : 0);
            }
        }

        public virtual void ChangeMode(Mode newMode)
        {
            if (newMode != mode)
            {
                base.firstChunk.collideWithObjects = newMode != Mode.Carried;
                base.firstChunk.collideWithTerrain = newMode == Mode.Free;
                base.firstChunk.goThroughFloors = true;

                mode = newMode;
            }
        }

        #region drawLogic
        public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
        }

        public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
        }

        public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }

        public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("Items");
            }
            for (int i = sLeaser.sprites.Length - 1; i >= 0; i--)
            {
                sLeaser.sprites[i].RemoveFromContainer();
                newContatiner.AddChild(sLeaser.sprites[i]);
            }
        }
        #endregion
    }
}