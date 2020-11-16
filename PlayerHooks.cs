using RWCustom;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace PrimitiveArmory
{
    public class PlayerHooks
    {

        public static int swingTime = 10; // Time between regular swings.
        public static float comboTimeMultiplier = 4f; // Multiplier for how long after a player finishes a combo before they can swing again.
        private static int postComboCooldown = (int)(swingTime * comboTimeMultiplier); // Time after a successful combo until the player can swing again.
        public static int maxCombo = 2; // The maximum combo a player can attain.
        public static float comboCancelMultiplier = 2.5f;

        public struct ArmoryState
        {
            public int swingTimer;
            public int swingDelay;
            public int comboCount;
            public int comboCooldown;
            public int swingAnimTimer;
            public float clubSkill;
            public EquippedArmor headSlot;
            public EquippedArmor bodySlot;
            public EquippedArmor accessorySlot;
            public BackSlot backSlot;
            public bool attackIncrement;
            public int attackCounter;
        }

        public class EquippedArmor
        {
            public Player owner;
            public Armor armor;
            public bool increment;
            public int counter;
            public float flip;
            public bool interactionLocked;
            public AbstractArmorStick abstractStick;

            public EquippedArmor(Player owner)
            {
                this.owner = owner;
            }
        }

        public class BackSlot
        {
            public Player owner;
            public Weapon backItem;
            public bool increment;
            public int counter;
            public float flip;
            public bool interactionLocked;
            public AbstractArmorStick abstractStick;

            public BackSlot(Player owner)
            {
                this.owner = owner;
            }
        }

        public class AbstractArmorStick : AbstractPhysicalObject.AbstractObjectStick
        {
            public AbstractPhysicalObject Player
            {
                get
                {
                    return A;
                }
                set
                {
                    A = value;
                }
            }

            public AbstractPhysicalObject Armor
            {
                get
                {
                    return B;
                }
                set
                {
                    B = value;
                }
            }

            public AbstractArmorStick(AbstractPhysicalObject player, AbstractPhysicalObject mask)
                : base(player, mask)
            {
            }

            public override string SaveToString(int roomIndex)
            {
                return roomIndex + "<stkA>gripStk<stkA>" + A.ID.ToString() + "<stkA>" + B.ID.ToString() + "<stkA>" + "2" + "<stkA>" + "1";
            }
        }

        public class AbstractWeaponOnBackStick : AbstractPhysicalObject.AbstractObjectStick
        {
            public AbstractPhysicalObject Player
            {
                get
                {
                    return A;
                }
                set
                {
                    A = value;
                }
            }

            public AbstractPhysicalObject Armor
            {
                get
                {
                    return B;
                }
                set
                {
                    B = value;
                }
            }

            public AbstractWeaponOnBackStick(AbstractPhysicalObject player, AbstractPhysicalObject mask)
                : base(player, mask)
            {
            }

            public override string SaveToString(int roomIndex)
            {
                return roomIndex + "<stkA>gripStk<stkA>" + A.ID.ToString() + "<stkA>" + B.ID.ToString() + "<stkA>" + "2" + "<stkA>" + "1";
            }
        }

        public static ArmoryState[] stats;
        public static int totalPlayerNum = 4;

        public static void Patch()
        {
            stats = new ArmoryState[totalPlayerNum];
            Debug.Log("Patching Player Constructor");
            On.Player.ctor += (PlayerPatch);

            Debug.Log("Patching Player.Grabability");
            On.Player.Grabability += GrababilityPatch;
            Debug.Log("Patching Player.GrabUpdate");
            On.Player.GrabUpdate += GrabUpdatePatch;
            Debug.Log("Patching Player.Update");
            On.Player.Update += PlayerUpdatePatch;
            Debug.Log("Patching Player.ThrowObject");
            On.Player.ThrowObject += ThrowPatch;
            Debug.Log("Patching Player.GraphicsModuleUpdated");
            On.Player.GraphicsModuleUpdated += GraphicsModulePatch;
        }
        public static void PlayerPatch(On.Player.orig_ctor orig, Player player, AbstractCreature abstractCreature, World world)
        {
            orig(player, abstractCreature, world);

            int playerNumber = player.playerState.playerNumber;
            if (playerNumber >= totalPlayerNum)
            {
                Debug.Log("Extra slugcats detected: " + playerNumber);
                MoreSlugcat(playerNumber);
            }

            stats[playerNumber] = new ArmoryState
            {
                swingDelay = 0,
                swingTimer = 0,
                comboCount = 0,
                comboCooldown = 0,
                attackIncrement = false,
                attackCounter = 0,
                swingAnimTimer = 0,
                headSlot = null,
                bodySlot = null,
                accessorySlot = null,
                backSlot = null
            };
        }

        public static void PlayerUpdatePatch(On.Player.orig_Update orig, Player player, bool eu)
        {
            orig(player, eu);

            if (stats[player.playerState.playerNumber].swingTimer > 0)
            {
                stats[player.playerState.playerNumber].swingTimer--;
            }

            if (stats[player.playerState.playerNumber].swingDelay > 0)
            {
                stats[player.playerState.playerNumber].swingDelay--;
            }

            if (stats[player.playerState.playerNumber].comboCooldown > 0)
            {
                stats[player.playerState.playerNumber].comboCooldown--;
            }

            if (stats[player.playerState.playerNumber].swingAnimTimer > 0)
            {
                stats[player.playerState.playerNumber].swingAnimTimer--;
            }

            if (stats[player.playerState.playerNumber].comboCooldown == 1)
            {
                stats[player.playerState.playerNumber].comboCount = 0;
            }

        }
        // (float)stats[player.playerState.playerNumber].swingAnimTimer / (float)swingTime
        public static void GraphicsModulePatch(On.Player.orig_GraphicsModuleUpdated orig, Player player, bool actuallyViewed, bool eu)
        {

            orig(player, actuallyViewed, eu);

            for (int i = 0; i < 2; i++)
            {
                if (player.grasps[i] == null)
                {
                    continue;
                }

                if (actuallyViewed && player.grasps[i].grabbed is Club)
                {
                    player.grasps[i].grabbed.firstChunk.vel = (player.graphicsModule as PlayerGraphics).hands[i].vel;
                    player.grasps[i].grabbed.firstChunk.MoveFromOutsideMyUpdate(eu, (player.graphicsModule as PlayerGraphics).hands[i].pos);
                    if (player.grasps[i].grabbed is Weapon)
                    {
                        Vector2 vector = Custom.DirVec(player.mainBodyChunk.pos, player.grasps[i].grabbed.bodyChunks[0].pos) * ((i != 0) ? 1f : (-1f));
                        if (player.animation != Player.AnimationIndex.HangFromBeam)
                        {
                            vector = Custom.PerpendicularVector(vector);
                        }

                        if (player.animation != Player.AnimationIndex.ClimbOnBeam)
                        {
                            vector = Vector3.Slerp(vector, Custom.DegToVec((80f + Mathf.Cos((float)(player.animationFrame + ((!player.leftFoot) ? 3 : 9)) / 12f * 2f * (float)Math.PI) * 4f * (player.graphicsModule as PlayerGraphics).spearDir) * (player.graphicsModule as PlayerGraphics).spearDir), Mathf.Abs((player.graphicsModule as PlayerGraphics).spearDir));
                        }

                        if (stats[player.playerState.playerNumber].swingAnimTimer > 0)
                        {
                            float swingProgress = (float)stats[player.playerState.playerNumber].swingAnimTimer / (float)swingTime;
                            float swingVector = Mathf.Lerp(90, 0, swingProgress);

                            vector = Custom.DegToVec(swingVector);

                            (player.graphicsModule as PlayerGraphics).hands[i].reachingForObject = true;
                            player.grasps[i].grabbed.firstChunk.pos = player.mainBodyChunk.pos + vector * 15f;
                            (player.graphicsModule as PlayerGraphics).hands[i].absoluteHuntPos = player.grasps[i].grabbed.firstChunk.pos;
                        }

                        (player.grasps[i].grabbed as Weapon).setRotation = vector;
                        (player.grasps[i].grabbed as Weapon).rotationSpeed = 0f;
                    }
                }
            }
        }

        public static void MoreSlugcat(int slugcatNum)
        {
            List<ArmoryState> list = new List<ArmoryState>();
            for (int i = 0; i < stats.Length; i++)
            {
                list.Add(stats[i]);
            }
            totalPlayerNum = slugcatNum + 1;
            stats = new ArmoryState[totalPlayerNum];
            for (int j = 0; j < stats.Length; j++)
            {
                stats[j] = list[j];
            }
        }

        public static Player.ObjectGrabability GrababilityPatch(On.Player.orig_Grabability orig, Player player, PhysicalObject obj)
        {
            // code that runs before game code

            if (obj is Club)
            {
                return Player.ObjectGrabability.BigOneHand;
            }

            Player.ObjectGrabability result = orig.Invoke(player, obj);

            // code that runs after game code

            return result;
        }

        public static void GrabUpdatePatch(On.Player.orig_GrabUpdate orig, Player player, bool eu)
        {
            orig.Invoke(player, eu);
        }

        public static void ThrowPatch(On.Player.orig_ThrowObject orig, Player player, int grasp, bool eu)
        {
            PhysicalObject thrownObject = player.grasps[grasp].grabbed;
            AbstractPhysicalObject.AbstractObjectType thrownType = thrownObject.abstractPhysicalObject.type;
            RWCustom.IntVector2 throwDir = new RWCustom.IntVector2(player.ThrowDirection, 0);

            if (thrownType == EnumExt_NewItems.Club)
            {
                if (stats[player.playerState.playerNumber].swingDelay <= 0 && player.animation != Player.AnimationIndex.Flip && player.animation != Player.AnimationIndex.CrawlTurn && player.animation != Player.AnimationIndex.Roll)
                {

                    player.room.PlaySound(SoundID.Slugcat_Throw_Spear, player.firstChunk);
                    thrownObject.firstChunk.vel.x = thrownObject.firstChunk.vel.x + (float)throwDir.x * 30f;
                    player.room.AddObject(new ExplosionSpikes(player.room, thrownObject.firstChunk.pos + new Vector2((float)player.rollDirection * -40f, 0f), 6, 5.5f, 4f, 4.5f, 21f, new Color(1f, 1f, 1f, 0.25f)));

                    player.bodyChunks[0].vel += throwDir.ToVector2() * 4f;
                    player.bodyChunks[1].vel -= throwDir.ToVector2() * 3f;
                    // swingAnimTimer
                    stats[player.playerState.playerNumber].comboCooldown = 30;

                    stats[player.playerState.playerNumber].swingAnimTimer = swingTime;

                    if (stats[player.playerState.playerNumber].comboCount >= maxCombo)
                    {
                        stats[player.playerState.playerNumber].swingDelay = postComboCooldown;
                        player.room.AddObject(new ExplosionSpikes(player.room, thrownObject.firstChunk.pos + new Vector2((float)player.rollDirection * -40f, 0f), 15, 25f, 4f, 4.5f, 21f, new Color(1f, 0.5f, 0.75f, 0.5f)));
                        stats[player.playerState.playerNumber].comboCount = 0;
                    }
                    else
                    {
                        stats[player.playerState.playerNumber].swingDelay = swingTime;
                        stats[player.playerState.playerNumber].comboCount++;
                    }
                }
                return;
            }
            orig(player, grasp, eu);
        }

        public static void ThrownSpearPatch(On.Player.orig_ThrownSpear orig, Player player, Spear spear)
        {
            orig(player, spear);
        }
    }
}
