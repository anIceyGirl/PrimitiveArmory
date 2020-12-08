using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimitiveArmory
{
    public class MoonHooks
    {
        public static void Patch()
        {
            On.SLOracleBehaviorHasMark.TypeOfMiscItem += MoonText;
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += MoonConvoExtended;
        }

        private static SLOracleBehaviorHasMark.MiscItemType MoonText(On.SLOracleBehaviorHasMark.orig_TypeOfMiscItem orig, SLOracleBehaviorHasMark moonConvo, PhysicalObject testItem)
        {
            AbstractPhysicalObject.AbstractObjectType itemType = testItem.abstractPhysicalObject.type;

            if (itemType == EnumExt_NewItems.Club)
            {
                return EnumExt_NewItems.ClubDialogue;
            }

            if (itemType == EnumExt_NewItems.Bow)
            {
                return EnumExt_NewItems.BowDialogue;
            }

            if (itemType == EnumExt_NewItems.Arrow)
            {
                Arrow arrow = testItem as Arrow;

                return EnumExt_NewItems.ArrowDialogue;
            }

            return orig(moonConvo, testItem);
        }

        private static void MoonConvoExtended(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation moonConvo)
        {
            orig(moonConvo);
			if (moonConvo.id == Conversation.ID.Moon_Misc_Item)
            {
                if (moonConvo.describeItem == EnumExt_NewItems.ClubDialogue)
                {
                    moonConvo.events.Add(new Conversation.TextEvent(moonConvo, 10, moonConvo.Translate("It's the bone of a large creature, carved into the shape of a club."), 0));
                    moonConvo.events.Add(new Conversation.TextEvent(moonConvo, 10, moonConvo.Translate("I can imagine using this could cause quite a bit of havoc,<LINE>so please don't swing it around me."), 0));
                }
                if (moonConvo.describeItem == EnumExt_NewItems.BowDialogue)
                {
                    moonConvo.events.Add(new Conversation.TextEvent(moonConvo, 10, moonConvo.Translate("It's a long, curved piece of some composite material,<LINE>with a thread wrapped around the ends.<LINE>Some form of spider silk, perhaps?"), 0));
                    moonConvo.events.Add(new Conversation.TextEvent(moonConvo, 10, moonConvo.Translate("I remember some scavengers that passed by carrying a few of these on their back...<LINE>did they give you this, <PlayerName>?"), 0));
                }
                if (moonConvo.describeItem == EnumExt_NewItems.ArrowDialogue)
                {
                    moonConvo.events.Add(new Conversation.TextEvent(moonConvo, 10, moonConvo.Translate("This is a large needle with feathers attached at one end<LINE>and a piece of sharpened rock attached to ther other."), 0));
                }
			}
		}
    }
}
