using UnityEngine;

namespace PrimitiveArmory
{
    public abstract class Armor : PlayerCarryableItem, IDrawable
    {
        public enum ArmorSlot
        {
            Head,
            Body,
            Accessory,
            None
        }

        public enum Mode
        {
            Equipped,
            Carried,
            Free
        }

        public Creature equippedBy;

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
            inFrontOfObjects = -1;
            armorSlot = ArmorSlot.None;
            equippedBy = null;
        }

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
            for (int num = sLeaser.sprites.Length - 1; num >= 0; num--)
            {
                sLeaser.sprites[num].RemoveFromContainer();
                newContatiner.AddChild(sLeaser.sprites[num]);
            }
        }
    }
}