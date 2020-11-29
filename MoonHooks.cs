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
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += MoonConversation_AddEvents;
        }

        private static SLOracleBehaviorHasMark.MiscItemType MoonText(On.SLOracleBehaviorHasMark.orig_TypeOfMiscItem orig, SLOracleBehaviorHasMark self, PhysicalObject testItem)
        {

            if (testItem.abstractPhysicalObject.type == EnumExt_NewItems.Club)
            {
                return EnumExt_NewItems.ClubDialogue;
            }

            if (testItem.abstractPhysicalObject.type == EnumExt_NewItems.Bow)
            {
                return EnumExt_NewItems.BowDialogue;
            }

            if (testItem.abstractPhysicalObject.type == EnumExt_NewItems.Arrow)
            {
                return EnumExt_NewItems.ArrowDialogue;
            }

            return orig(self, testItem);
        }

        private static void MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
        {
            orig(self);
			if (self.id == Conversation.ID.Moon_Misc_Item)
            {
                if (self.describeItem == EnumExt_NewItems.ClubDialogue)
                {
                    self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(""), 0));
                }
                if (self.describeItem == EnumExt_NewItems.BowDialogue)
                {
                    self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("It's a long, curved piece of some composite material,<LINE>with a thread wrapped around the ends.<LINE>Some form of spider silk, perhaps?"), 0));
                    self.events.Add(new Conversation.TextEvent(self, 10, self.Translate("I remember some scavengers that passed by carrying a few of these on their back...<LINE> did they give you this, <PlayerName>?"), 0));
                }
                if (self.describeItem == EnumExt_NewItems.ArrowDialogue)
                {
                    self.events.Add(new Conversation.TextEvent(self, 10, self.Translate(""), 0));
                }
			}
		}
    }
}
