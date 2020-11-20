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
            public float rangedSkill;
            public float meleeSkill;
            public EquippedArmor headSlot;
            public EquippedArmor bodySlot;
            public EquippedArmor accessorySlot;
            public BackSlot backSlot;
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
            public AbstractWeaponOnBackStick abstractStick;

            public bool HasAWeapon => backItem != null;

            public BackSlot(Player owner)
            {
                this.owner = owner;
            }

            public void Update(bool eu)
            {
                if (!interactionLocked && increment)
                {
                    counter++;
                    if (backItem != null && counter > 20)
                    {
                        WeaponToHand(eu);
                        counter = 0;
                    }
                    else if (backItem == null && counter > 20)
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            if (owner.grasps[i] != null && owner.grasps[i].grabbed is Weapon)
                            {
                                owner.bodyChunks[0].pos += Custom.DirVec(owner.grasps[i].grabbed.firstChunk.pos, owner.bodyChunks[0].pos) * 2f;
                                WeaponToBack(owner.grasps[i].grabbed as Weapon);
                                counter = 0;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    counter = 0;
                }
                if (!owner.input[0].pckp)
                {
                    interactionLocked = false;
                }
                increment = false;
            }

            public void GraphicsModuleUpdated(bool actuallyViewed, bool eu)
            {
                if (backItem == null)
                {
                    return;
                }
                if (backItem.slatedForDeletetion || backItem.grabbedBy.Count > 0)
                {
                    if (abstractStick != null)
                    {
                        abstractStick.Deactivate();
                    }
                    backItem = null;
                    return;
                }
                Vector2 vector = owner.mainBodyChunk.pos;
                Vector2 vector2 = owner.bodyChunks[1].pos;
                if (owner.graphicsModule != null)
                {
                    vector = Vector2.Lerp((owner.graphicsModule as PlayerGraphics).drawPositions[0, 0], (owner.graphicsModule as PlayerGraphics).head.pos, 0.2f);
                    vector2 = (owner.graphicsModule as PlayerGraphics).drawPositions[1, 0];
                }
                Vector2 vector3 = Custom.DirVec(vector2, vector);
                if (owner.Consious && owner.bodyMode != Player.BodyModeIndex.ZeroG && owner.room.gravity > 0f)
                {
                    if (owner.bodyMode == Player.BodyModeIndex.Default && owner.animation == Player.AnimationIndex.None && owner.standing && owner.bodyChunks[1].pos.y < owner.bodyChunks[0].pos.y - 6f)
                    {
                        flip = Custom.LerpAndTick(flip, (float)owner.input[0].x * 0.3f, 0.05f, 0.02f);
                    }
                    else if (owner.bodyMode == Player.BodyModeIndex.Stand && owner.input[0].x != 0)
                    {
                        flip = Custom.LerpAndTick(flip, owner.input[0].x, 0.02f, 0.1f);
                    }
                    else
                    {
                        flip = Custom.LerpAndTick(flip, (float)owner.flipDirection * Mathf.Abs(vector3.x), 0.15f, 355f / (678f * (float)Math.PI));
                    }
                    if (counter > 12 && !interactionLocked && owner.input[0].x != 0 && owner.standing)
                    {
                        float num = 0f;
                        for (int i = 0; i < owner.grasps.Length; i++)
                        {
                            if (owner.grasps[i] == null)
                            {
                                num = ((i != 0) ? 1f : (-1f));
                                break;
                            }
                        }
                        backItem.setRotation = Custom.DegToVec(Custom.AimFromOneVectorToAnother(vector2, vector) + Custom.LerpMap(counter, 12f, 20f, 0f, 360f * num));
                    }
                    else
                    {
                        backItem.setRotation = (vector3 - Custom.PerpendicularVector(vector3) * 0.9f * (1f - Mathf.Abs(flip))).normalized;
                    }
                    backItem.ChangeOverlap(vector3.y < -0.1f && owner.bodyMode != Player.BodyModeIndex.ClimbingOnBeam);
                }
                else
                {
                    flip = Custom.LerpAndTick(flip, 0f, 0.15f, 0.142857149f);
                    backItem.setRotation = vector3 - Custom.PerpendicularVector(vector3) * 0.9f;
                    backItem.ChangeOverlap(newOverlap: false);
                }
                backItem.firstChunk.MoveFromOutsideMyUpdate(eu, Vector2.Lerp(vector2, vector, 0.6f) - Custom.PerpendicularVector(vector2, vector) * 7.5f * flip);
                backItem.firstChunk.vel = owner.mainBodyChunk.vel;
                backItem.rotationSpeed = 0f;
            }

            public void WeaponToHand(bool eu)
            {
                if (backItem == null)
                {
                    return;
                }
                for (int i = 0; i < 2; i++)
                {
                    if (owner.grasps[i] != null && owner.Grabability(owner.grasps[i].grabbed) >= Player.ObjectGrabability.BigOneHand)
                    {
                        return;
                    }
                }
                int targetHand = -1;
                for (int j = 0; j < 2; j++)
                {
                    if (targetHand != -1)
                    {
                        break;
                    }
                    if (owner.grasps[j] == null)
                    {
                        targetHand = j;
                    }
                }
                if (targetHand != -1)
                {
                    if (owner.graphicsModule != null)
                    {
                        backItem.firstChunk.MoveFromOutsideMyUpdate(eu, (owner.graphicsModule as PlayerGraphics).hands[targetHand].pos);
                    }
                    owner.SlugcatGrab(backItem, targetHand);
                    backItem = null;
                    interactionLocked = true;
                    owner.noPickUpOnRelease = 20;
                    owner.room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, owner.mainBodyChunk);
                    if (abstractStick != null)
                    {
                        abstractStick.Deactivate();
                        abstractStick = null;
                    }
                }
            }

            public void WeaponToBack(Weapon backItem)
            {
                if (this.backItem != null)
                {
                    return;
                }
                for (int i = 0; i < 2; i++)
                {
                    if (owner.grasps[i] != null && owner.grasps[i].grabbed == backItem)
                    {
                        owner.ReleaseGrasp(i);
                        break;
                    }
                }
                this.backItem = backItem;
                this.backItem.ChangeMode(Weapon.Mode.OnBack);
                interactionLocked = true;
                owner.noPickUpOnRelease = 20;
                owner.room.PlaySound(SoundID.Slugcat_Stash_Spear_On_Back, owner.mainBodyChunk);
                if (abstractStick != null)
                {
                    abstractStick.Deactivate();
                }
                abstractStick = new AbstractWeaponOnBackStick(owner.abstractPhysicalObject, this.backItem.abstractPhysicalObject);
            }

            public void DropItem()
            {
                if (backItem != null)
                {
                    backItem.firstChunk.vel = owner.mainBodyChunk.vel + Custom.RNV() * 3f * UnityEngine.Random.value;
                    backItem.ChangeMode(Weapon.Mode.Free);
                    backItem = null;
                    if (abstractStick != null)
                    {
                        abstractStick.Deactivate();
                        abstractStick = null;
                    }
                }
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

            public AbstractPhysicalObject Weapon
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

            public AbstractWeaponOnBackStick(AbstractPhysicalObject player, AbstractPhysicalObject weapon) : base(player, weapon)
            {

            }

            public override string SaveToString(int roomIndex)
            {
                return roomIndex + "<stkA>wepOnBackStick<stkA>" + A.ID.ToString() + "<stkA>" + B.ID.ToString() + "<stkA>" + "2" + "<stkA>" + "1";
            }
        }

        public static bool CanPutWeaponToBack(Player player, Weapon weapon)
        {
            int playerNumber = player.playerState.playerNumber;
            if (player.spearOnBack.HasASpear)
            {
                return false;
            }

            if(weapon is Club)
            {
                return !stats[playerNumber].backSlot.HasAWeapon && (player.grasps[0]?.grabbed is Club || player.grasps[1]?.grabbed is Club) && !player.spearOnBack.HasASpear;
            }

            return false;
        }

        public static bool CanRetrieveWeaponFromBack(Player player)
        {
            int playerNumber = player.playerState.playerNumber;
            int activeHand = -1;
            for (int i = 0; i < 2; i++)
            {
                if (player.grasps[i] == null)
                {
                    activeHand = i;
                    continue;
                }
                if (player.grasps[i]?.grabbed is IPlayerEdible || player.grasps[i].grabbed is Spear)
                {
                    return false;
                }
                if (player.Grabability(player.grasps[i].grabbed) >= Player.ObjectGrabability.TwoHands)
                {
                    return false;
                }
            }
            if (player.spearOnBack != null && player.spearOnBack.HasASpear)
            {
                return false;
            }

            return stats[playerNumber].backSlot.HasAWeapon && activeHand > -1;
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

            On.Player.Die += DeathPatch;

            On.Player.SpearOnBack.SpearToBack += SpearOnBack_SpearToBack;
        }

        private static void SpearOnBack_SpearToBack(On.Player.SpearOnBack.orig_SpearToBack orig, Player.SpearOnBack spear, Spear spr)
        {
            Player owner = spear.owner;
            int playerNumber = owner.playerState.playerNumber;

            if (stats[playerNumber].backSlot.backItem is Weapon)
            {
                return;
            }

            orig(spear, spr);
        }

        private static void DeathPatch(On.Player.orig_Die orig, Player player)
        {
            int playerNumber = player.playerState.playerNumber;

            if (stats[playerNumber].backSlot != null && stats[playerNumber].backSlot.backItem != null)
            {
                stats[playerNumber].backSlot.DropItem();
            }

            orig(player);
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
                swingAnimTimer = 0,
                headSlot = null,
                bodySlot = null,
                accessorySlot = null,
                backSlot = null
            };

            switch (player.slugcatStats.name)
            {
                case SlugcatStats.Name.White:
                    stats[playerNumber].meleeSkill = 1f;
                    stats[playerNumber].rangedSkill = 1f;
                    break;
                case SlugcatStats.Name.Red:
                    stats[playerNumber].meleeSkill = 1.25f;
                    stats[playerNumber].rangedSkill = 0.75f;
                    break;
                case SlugcatStats.Name.Yellow:
                    stats[playerNumber].meleeSkill = 0.75f;
                    stats[playerNumber].rangedSkill = 1.25f;
                    break;

                default:
                    stats[playerNumber].meleeSkill = 1f;
                    stats[playerNumber].rangedSkill = 1f;
                    break;
            }
        }

        public static void PlayerUpdatePatch(On.Player.orig_Update orig, Player player, bool eu)
        {
            orig(player, eu);

            int playerNumber = player.playerState.playerNumber;

            if (stats[playerNumber].backSlot == null)
            {
                stats[playerNumber].backSlot = new BackSlot(player);
            }

            if (player.input[0].pckp && !stats[playerNumber].backSlot.interactionLocked && ((CanPutWeaponToBack(player, (player.grasps[0]?.grabbed as Weapon)) || CanPutWeaponToBack(player, (player.grasps[1]?.grabbed as Weapon))) || CanRetrieveWeaponFromBack(player)) && player.CanPutSpearToBack)
            {
                stats[playerNumber].backSlot.increment = true;
            }
            else
            {
                stats[playerNumber].backSlot.increment = false;
            }

            if (player.input[0].pckp && player.grasps[0] != null && player.grasps[0].grabbed is Creature && player.CanEatMeat(player.grasps[0].grabbed as Creature) && (player.grasps[0].grabbed as Creature).Template.meatPoints > 0)
            {
                stats[playerNumber].backSlot.increment = false;
                stats[playerNumber].backSlot.interactionLocked = true;
            }
            else if (player.swallowAndRegurgitateCounter > 90)
            {
                stats[playerNumber].backSlot.increment = false;
                stats[playerNumber].backSlot.interactionLocked = true;
            }

            stats[playerNumber].backSlot.Update(eu);
            
            if (stats[playerNumber].backSlot.HasAWeapon && player.spearOnBack.increment)
            {
                player.spearOnBack.increment = false;
            }

            if (stats[playerNumber].swingTimer > 0)
            {
                stats[playerNumber].swingTimer--;
            }

            if (stats[playerNumber].swingDelay > 0)
            {
                stats[playerNumber].swingDelay--;
            }

            if (stats[playerNumber].comboCooldown > 0)
            {
                stats[playerNumber].comboCooldown--;
            }

            if (stats[playerNumber].swingAnimTimer > 0)
            {
                stats[playerNumber].swingAnimTimer--;
            }

            if (stats[playerNumber].comboCooldown == 1)
            {
                stats[playerNumber].comboCount = 0;
            }
        }

        public static void GraphicsModulePatch(On.Player.orig_GraphicsModuleUpdated orig, Player player, bool actuallyViewed, bool eu)
        {

            orig(player, actuallyViewed, eu);

            int playerNumber = player.playerState.playerNumber;

            if (stats[playerNumber].backSlot != null)
            {
                stats[playerNumber].backSlot.GraphicsModuleUpdated(actuallyViewed, eu);
            }

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
                            float swingAngle = Mathf.Lerp(110f, -20f, swingProgress * swingProgress);
                            vector = Custom.DegToVec(swingAngle);

                            (player.grasps[i].grabbed as Club).isSwinging = true;


                            if (player.ThrowDirection < 0)
                            {
                                vector = new Vector2(-vector.x, vector.y);
                            }

                            (player.graphicsModule as PlayerGraphics).hands[i].reachingForObject = true;
                            player.grasps[i].grabbed.firstChunk.pos = player.mainBodyChunk.pos + vector * 25f;
                            (player.graphicsModule as PlayerGraphics).hands[i].absoluteHuntPos = player.grasps[i].grabbed.firstChunk.pos;
                        }
                        else
                        {
                            (player.grasps[i].grabbed as Club).isSwinging = false;
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
                if ((obj as Weapon).mode == Weapon.Mode.OnBack) {
                    return Player.ObjectGrabability.CantGrab;
                }

                return Player.ObjectGrabability.BigOneHand;
            }

            Player.ObjectGrabability result = orig.Invoke(player, obj);

            // code that runs after game code

            return result;
        }

        public static void GrabUpdatePatch(On.Player.orig_GrabUpdate orig, Player player, bool eu)
        {
            orig.Invoke(player, eu);

            if (player.pickUpCandidate is Weapon && CanPutWeaponToBack(player, stats[player.playerState.playerNumber].backSlot.backItem as Weapon) && ((player.grasps[0] != null && player.Grabability(player.grasps[0].grabbed) >= Player.ObjectGrabability.BigOneHand) || (player.grasps[1] != null && player.Grabability(player.grasps[1].grabbed) >= Player.ObjectGrabability.BigOneHand) || (player.grasps[0] != null && player.grasps[1] != null)))
            {
                Debug.Log("spear straight to back");
                player.room.PlaySound(SoundID.Slugcat_Switch_Hands_Init, player.mainBodyChunk);
                stats[player.playerState.playerNumber].backSlot.WeaponToBack(player.pickUpCandidate as Weapon);
            }
        }

        public static void ThrowPatch(On.Player.orig_ThrowObject orig, Player player, int grasp, bool eu)
        {
            PhysicalObject thrownObject = player.grasps[grasp].grabbed;
            AbstractPhysicalObject.AbstractObjectType thrownType = thrownObject.abstractPhysicalObject.type;
            RWCustom.IntVector2 throwDir = new RWCustom.IntVector2(player.ThrowDirection, 0);
            int playerNumber = player.playerState.playerNumber;

            if (thrownType == EnumExt_NewItems.Club)
            {
                if (stats[playerNumber].swingDelay <= 0 && player.animation != Player.AnimationIndex.Flip && player.animation != Player.AnimationIndex.CrawlTurn && player.animation != Player.AnimationIndex.Roll)
                {

                    player.room.PlaySound(SoundID.Slugcat_Throw_Spear, player.firstChunk);
                    thrownObject.firstChunk.vel.x = thrownObject.firstChunk.vel.x + (float)throwDir.x * 30f;
                    player.room.AddObject(new ExplosionSpikes(player.room, thrownObject.firstChunk.pos + new Vector2((float)player.rollDirection * -40f, 0f), 6, 5.5f, 4f, 4.5f, 21f, new Color(1f, 1f, 1f, 0.25f)));

                    player.bodyChunks[0].vel += throwDir.ToVector2() * 4f;
                    player.bodyChunks[1].vel -= throwDir.ToVector2() * 3f;
                    // swingAnimTimer
                    stats[playerNumber].comboCooldown = 30;

                    stats[playerNumber].swingAnimTimer = swingTime;

                    if (stats[playerNumber].comboCount >= maxCombo)
                    {
                        stats[playerNumber].swingDelay = postComboCooldown;
                        player.room.AddObject(new ExplosionSpikes(player.room, thrownObject.firstChunk.pos + new Vector2((float)player.rollDirection * -40f, 0f), 15, 25f, 4f, 4.5f, 21f, new Color(1f, 0.5f, 0.75f, 0.5f)));
                        stats[playerNumber].comboCount = 0;
                    }
                    else
                    {
                        stats[playerNumber].swingDelay = swingTime;
                        stats[playerNumber].comboCount++;
                    }

                    Vector2 clubTip = (thrownObject.firstChunk.pos + (thrownObject as Weapon).rotation * 50f);

                    SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks((thrownObject as SharedPhysics.IProjectileTracer), player.room, thrownObject.firstChunk.pos, ref clubTip, 10f, player.collisionLayer, player, true);
                    
                    if (collisionResult.obj != null)
                    { 
                        bool flag = false;
                        if (thrownObject.abstractPhysicalObject.world.game.IsArenaSession && thrownObject.abstractPhysicalObject.world.game.GetArenaGameSession.GameTypeSetup.spearHitScore != 0 && player != null && collisionResult.obj is Creature)
                        {
                            flag = true;
                            if ((collisionResult.obj as Creature).State is HealthState && ((collisionResult.obj as Creature).State as HealthState).health <= 0f)
                            {
                                flag = false;
                            }
                            else if (!((collisionResult.obj as Creature).State is HealthState) && (collisionResult.obj as Creature).State.dead)
                            {
                                flag = false;
                            }
                        }

                        if (collisionResult.obj is Creature)
                        {
                            player.room.PlaySound(SoundID.Rock_Hit_Creature, collisionResult.chunk);

                            (collisionResult.obj as Creature).Violence(thrownObject.firstChunk, (thrownObject as Weapon).rotation * thrownObject.firstChunk.mass * 2f, collisionResult.chunk, collisionResult.onAppendagePos, Creature.DamageType.Blunt, stats[playerNumber].meleeSkill* 0.6f, 20f);

                            if (flag)
                            {
                                thrownObject.abstractPhysicalObject.world.game.GetArenaGameSession.PlayerLandSpear(player, collisionResult.obj as Creature);
                            }
                        }
                        else
                        {
                            player.room.PlaySound(SoundID.Rock_Hit_Wall, collisionResult.chunk);
                        }
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
