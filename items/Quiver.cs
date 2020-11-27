using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PrimitiveArmory
{
    public class Quiver : Armor
    {
        public class AbstractQuiver : AbstractPhysicalObject
        {
            public int quiverCapacity = 6;

            public Arrow.AbstractArrow[] quiverContents;

            public AbstractQuiver(World world,  PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID) : base(world, EnumExt_NewItems.Quiver, realizedObject, pos, ID)
            {
                quiverContents = new Arrow.AbstractArrow[quiverCapacity];
            }
        }

        public AbstractQuiver abstractQuiver => abstractPhysicalObject as AbstractQuiver;

        public Quiver(AbstractPhysicalObject abstractPhysicalObject, World world) : base(abstractPhysicalObject, world)
        {
            armorSlot = ArmorSlot.Accessory;
        }

        public bool CanPutThisIntoQuiver(PhysicalObject arrow)
        {
            if (arrow.abstractPhysicalObject.type == EnumExt_NewItems.Arrow)
            {
                foreach (Arrow.AbstractArrow abstractArrow in abstractQuiver.quiverContents)
                {
                    if (abstractArrow == null)
                    {
                        return true;
                    }
                }
            }


            return false;
        }

        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            base.AddToContainer(sLeaser, rCam, newContatiner);
        }

        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
        }

        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
        }

        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            base.InitiateSprites(sLeaser, rCam);
        }
    }
}
