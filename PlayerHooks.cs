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

        public static float maxDrawTime = 35.0f;

        public struct ClubState
        {
            public int swingTimer;
            public int swingDelay;
            public int comboCount;
            public int comboCooldown;
        }

        public struct BowState
        {
            public float drawSpeed;
            public float drawTime;
            public bool isDrawing;
            public bool released;
            public Vector2 aimDir;
            public Vector2 lastAimDir;
        }

        public struct GlobalState
        {
            public float rangedSkill;
            public float meleeSkill;
            public int animTimer;
            public EquippedArmor headSlot;
            public EquippedArmor bodySlot;
            public EquippedArmor accessorySlot;
            public BackSlot backSlot;
        }

        public class EquippedArmor
        {
            public Player owner;
            public Armor armor;
            public Armor.ArmorSlot slot;
            public bool increment;
            public int counter;
            public float flip;
            public bool interactionLocked;
            public AbstractArmorStick abstractStick;

            public EquippedArmor(Player owner, Armor.ArmorSlot slot)
            {
                this.owner = owner;
                this.slot = slot;
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
                            if (owner.grasps[i] != null && owner.grasps[i].grabbed is Weapon && CanIStashThis(owner.grasps[i].grabbed as Weapon))
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
                for (int i = 0; i < 2; i++)
                {
                    if (targetHand != -1)
                    {
                        break;
                    }
                    if (owner.grasps[i] == null)
                    {
                        targetHand = i;
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
            if (player.spearOnBack != null)
            {
                int playerNumber = player.playerState.playerNumber;

                if (player.spearOnBack.HasASpear)
                {
                    return false;
                }

                switch (weapon)
                {
                    case Club club:
                        return !globalStats[playerNumber].backSlot.HasAWeapon && (player.grasps[0]?.grabbed is Club || player.grasps[1]?.grabbed is Club) && !player.spearOnBack.HasASpear;
                    case Bow bow:
                        return !globalStats[playerNumber].backSlot.HasAWeapon && (player.grasps[0]?.grabbed is Bow || player.grasps[1]?.grabbed is Bow) && !player.spearOnBack.HasASpear;
                    default:
                        break;
                }
            }

            return false;
        }

        public static ClubState[] clubStats;
        public static GlobalState[] globalStats;
        public static Player.InputPackage[] playerInput;
        public static BowState[] bowStats;

        public static int totalPlayerNum = 4;

        public static void Patch()
        {
            Debug.Log("Patching Player Constructor");
            On.Player.ctor += (PlayerPatch);
            Debug.Log("Patching Player.Grabability");
            On.Player.Grabability += GrababilityPatch;
            Debug.Log("Patching Player.Update");
            On.Player.Update += PlayerUpdatePatch;
            Debug.Log("Patching Player.ThrowObject");
            On.Player.ThrowObject += ThrowPatch;
            Debug.Log("Patching Player.GraphicsModuleUpdated");
            On.Player.GraphicsModuleUpdated += GraphicsModulePatch;
            Debug.Log("Patching Player.Die");
            On.Player.Die += DeathPatch;
            Debug.Log("Patching Player.SpearOnBack.SpearToBack");
            On.Player.SpearOnBack.SpearToBack += SpearToBackPatch;
            Debug.Log("Patching Player.checkInput");
            On.Player.checkInput += CheckInputPatch;

            clubStats = new ClubState[totalPlayerNum];
            bowStats = new BowState[totalPlayerNum];
            globalStats = new GlobalState[totalPlayerNum];
            playerInput = new Player.InputPackage[totalPlayerNum];
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

            clubStats[playerNumber] = new ClubState
            {
                swingDelay = 0,
                swingTimer = 0,
                comboCount = 0,
                comboCooldown = 0
            };

            bowStats[playerNumber] = new BowState
            {
                drawTime = 0.0f,
                aimDir = new Vector2(0, 0),
                lastAimDir = bowStats[playerNumber].aimDir,
                isDrawing = false,
                released = false
            };

            globalStats[playerNumber] = new GlobalState
            {
                headSlot = null,
                bodySlot = null,
                accessorySlot = null,
                backSlot = null
            };

            switch (player.slugcatStats.name)
            {
                case SlugcatStats.Name.White:
                    globalStats[playerNumber].meleeSkill = 1f;
                    globalStats[playerNumber].rangedSkill = 1f;
                    break;
                case SlugcatStats.Name.Red:
                    globalStats[playerNumber].meleeSkill = 1.25f;
                    globalStats[playerNumber].rangedSkill = 0.8f;
                    break;
                case SlugcatStats.Name.Yellow:
                    globalStats[playerNumber].meleeSkill = 0.75f;
                    globalStats[playerNumber].rangedSkill = 1.45f;
                    break;
                default:
                    globalStats[playerNumber].meleeSkill = 1f;
                    globalStats[playerNumber].rangedSkill = 1f;
                    break;
            }

            bowStats[playerNumber].drawSpeed = globalStats[playerNumber].rangedSkill * 1f;
        }

        public static void PlayerUpdatePatch(On.Player.orig_Update orig, Player player, bool eu)
        {
            int playerNumber = player.playerState.playerNumber;

            bowStats[playerNumber].aimDir = GetAimDir(player).normalized;

            if (bowStats[playerNumber].aimDir.magnitude > 0.25f)
            {
                bowStats[playerNumber].lastAimDir = bowStats[playerNumber].aimDir;
            }

            orig(player, eu);

            if (globalStats[playerNumber].backSlot == null)
            {
                globalStats[playerNumber].backSlot = new BackSlot(player);
            }

            if (player.input[0].pckp && !globalStats[playerNumber].backSlot.interactionLocked && ((CanPutWeaponToBack(player, (player.grasps[0]?.grabbed as Weapon)) || CanPutWeaponToBack(player, (player.grasps[1]?.grabbed as Weapon))) || CanRetrieveWeaponFromBack(player)) && player.CanPutSpearToBack)
            {
                globalStats[playerNumber].backSlot.increment = true;
            }
            else
            {
                globalStats[playerNumber].backSlot.increment = false;
            }

            if (player.input[0].pckp && player.grasps[0] != null && player.grasps[0].grabbed is Creature && player.CanEatMeat(player.grasps[0].grabbed as Creature) && (player.grasps[0].grabbed as Creature).Template.meatPoints > 0)
            {
                globalStats[playerNumber].backSlot.increment = false;
                globalStats[playerNumber].backSlot.interactionLocked = true;
            }
            else if (player.swallowAndRegurgitateCounter > 90)
            {
                globalStats[playerNumber].backSlot.increment = false;
                globalStats[playerNumber].backSlot.interactionLocked = true;
            }

            globalStats[playerNumber].backSlot.Update(eu);
            
            if (globalStats[playerNumber].backSlot.HasAWeapon && player.spearOnBack.increment)
            {
                player.spearOnBack.increment = false;
            }

            if (clubStats[playerNumber].swingTimer > 0)
            {
                clubStats[playerNumber].swingTimer--;
            }

            if (clubStats[playerNumber].swingDelay > 0)
            {
                clubStats[playerNumber].swingDelay--;
            }

            if (clubStats[playerNumber].comboCooldown > 0)
            {
                clubStats[playerNumber].comboCooldown--;
            }

            if (bowStats[playerNumber].isDrawing)
            {
                bowStats[playerNumber].drawTime = Mathf.Clamp(bowStats[playerNumber].drawTime + bowStats[playerNumber].drawSpeed, 0.0f, maxDrawTime);
            }
            else
            {
                bowStats[playerNumber].drawTime = 0.0f;
            }

            if (globalStats[playerNumber].animTimer > 0)
            {
                globalStats[playerNumber].animTimer--;
            }

            if (clubStats[playerNumber].comboCooldown == 1)
            {
                clubStats[playerNumber].comboCount = 0;
            }
        }

        public static void MoreSlugcat(int slugcatNum)
        {
            List<ClubState> clubState = new List<ClubState>();
            List<BowState> bowState = new List<BowState>();
            List<GlobalState> globalState = new List<GlobalState>();
            List<Player.InputPackage> inputList = new List<Player.InputPackage>();
            for (int i = 0; i < clubStats.Length; i++)
            {
                clubState.Add(clubStats[i]);
                globalState.Add(globalStats[i]);
                bowState.Add(bowStats[i]);
                inputList.Add(inputList[i]);
            }
            totalPlayerNum = slugcatNum + 1;
            clubStats = new ClubState[totalPlayerNum];
            bowStats = new BowState[totalPlayerNum];
            globalStats = new GlobalState[totalPlayerNum];
            playerInput = new Player.InputPackage[totalPlayerNum];

            for (int j = 0; j < clubStats.Length; j++)
            {
                clubStats[j] = clubState[j];
                bowStats[j] = bowState[j];
                globalStats[j] = globalState[j];
                playerInput[j] = inputList[j];
            }
        }

        public static bool CanIStashThis(Weapon weapon)
        {
            switch (weapon)
            {
                case Bow bow:
                case Club club:
                    return true;
                default:
                    return false;
            }
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

            return globalStats[playerNumber].backSlot.HasAWeapon && activeHand > -1;
        }

        private static void CheckInputPatch(On.Player.orig_checkInput orig, Player player)
        {
            int playerNumber = player.playerState.playerNumber;
            orig(player);
            if (player.stun == 0 && !player.dead)
            {
                PhysicalObject objectChecked;

                try
                {
                    objectChecked = player.grasps[0].grabbed;
                }
                catch
                {
                    objectChecked = null;
                }

                if (player.input[playerNumber].thrw && objectChecked != null && objectChecked.abstractPhysicalObject.type == EnumExt_NewItems.Bow)
                {
                    playerInput[playerNumber] = player.input[0];
                    player.input[0].x = 0;
                    player.input[0].y = 0;
                    Player.InputPackage[] input = player.input;
                    int x = 0;
                    player.input[x].analogueDir = input[x].analogueDir * 0f;

                    bowStats[playerNumber].isDrawing = true;
                }
                else
                {
                    bowStats[playerNumber].isDrawing = false;
                }
            }
        }

        private static void SpearToBackPatch(On.Player.SpearOnBack.orig_SpearToBack orig, Player.SpearOnBack spear, Spear spr)
        {
            int playerNumber = spear.owner.playerState.playerNumber;

            if (globalStats[playerNumber].backSlot.backItem is Weapon) return;

            orig(spear, spr);
        }

        private static void DeathPatch(On.Player.orig_Die orig, Player player)
        {
            int playerNumber = player.playerState.playerNumber;

            if (globalStats[playerNumber].backSlot != null && globalStats[playerNumber].backSlot.backItem != null)
            {
                globalStats[playerNumber].backSlot.DropItem();
            }

            orig(player);
        }

        public static void GraphicsModulePatch(On.Player.orig_GraphicsModuleUpdated orig, Player player, bool actuallyViewed, bool eu)
        {

            orig(player, actuallyViewed, eu);

            int playerNumber = player.playerState.playerNumber;

            if (globalStats[playerNumber].backSlot != null)
            {
                globalStats[playerNumber].backSlot.GraphicsModuleUpdated(actuallyViewed, eu);
            }

            for (int i = 0; i < 2; i++)
            {
                if (player.grasps[i] == null)
                {
                    continue;
                }

                if (actuallyViewed)
                {
                    Vector2 vector = Custom.DirVec(player.mainBodyChunk.pos, player.grasps[i].grabbed.bodyChunks[0].pos) * ((i != 0) ? 1f : (-1f));

                    switch (player.grasps[i].grabbed)
                    {
                        case Club club:
                            player.grasps[i].grabbed.firstChunk.vel = (player.graphicsModule as PlayerGraphics).hands[i].vel;
                            player.grasps[i].grabbed.firstChunk.MoveFromOutsideMyUpdate(eu, (player.graphicsModule as PlayerGraphics).hands[i].pos);
                            if (player.animation != Player.AnimationIndex.HangFromBeam)
                            {
                                vector = Custom.PerpendicularVector(vector);
                            }
                            
                            if (player.animation != Player.AnimationIndex.ClimbOnBeam)
                            {
                                vector = Vector3.Slerp(vector, Custom.DegToVec((80f + Mathf.Cos((float)(player.animationFrame + ((!player.leftFoot) ? 3 : 9)) / 12f * 2f * (float)Math.PI) * 4f * (player.graphicsModule as PlayerGraphics).spearDir) * (player.graphicsModule as PlayerGraphics).spearDir), Mathf.Abs((player.graphicsModule as PlayerGraphics).spearDir));
                            }
                            
                            if (globalStats[playerNumber].animTimer > 0)
                            {
                                float swingProgress = (float)globalStats[playerNumber].animTimer / (float)swingTime;
                                float swingAngle = Mathf.Lerp(110f, -20f, swingProgress * swingProgress);
                                vector = Custom.DegToVec(swingAngle);
                            
                                (player.grasps[i].grabbed as Club).isSwinging = true;
                            
                            
                                if (player.ThrowDirection < 0)
                                {
                                    vector = new Vector2(-vector.x, vector.y);
                                }
                            
                                (player.graphicsModule as PlayerGraphics).hands[i].reachingForObject = true;
                                player.grasps[i].grabbed.firstChunk.MoveFromOutsideMyUpdate(eu, player.mainBodyChunk.pos + vector * 25f);
                                (player.graphicsModule as PlayerGraphics).hands[i].absoluteHuntPos = player.grasps[i].grabbed.firstChunk.pos;
                            }
                            else
                            {
                                (player.grasps[i].grabbed as Club).isSwinging = false;
                            }
                            
                            (player.grasps[i].grabbed as Weapon).setRotation = vector;
                            (player.grasps[i].grabbed as Weapon).rotationSpeed = 0f;
                            
                            break;
                        case Bow bow:
                            // player.grasps[i].grabbed.firstChunk.vel = (player.graphicsModule as PlayerGraphics).hands[i].vel;
                            // player.grasps[i].grabbed.firstChunk.MoveFromOutsideMyUpdate(eu, (player.graphicsModule as PlayerGraphics).hands[i].pos);

                            if (player.bodyMode == Player.BodyModeIndex.Crawl)
                            {
                                vector = Custom.DirVec(player.bodyChunks[1].pos, Vector2.Lerp(player.grasps[i].grabbed.bodyChunks[0].pos, player.bodyChunks[0].pos, 0.8f));
                            }
                            
                            if (player.animation == Player.AnimationIndex.ClimbOnBeam)
                            {
                                vector.y = Mathf.Abs(vector.y);
                                vector = Vector3.Slerp(vector, Custom.DirVec(player.bodyChunks[1].pos, player.bodyChunks[0].pos), 0.75f);
                            }

                            vector = Vector3.Slerp(vector, Custom.DegToVec((35f + Mathf.Cos((float)(player.animationFrame + ((!player.leftFoot) ? 3 : 9)) / 12f * 2f * (float)Math.PI) * 4f * (player.graphicsModule as PlayerGraphics).spearDir) * (player.graphicsModule as PlayerGraphics).spearDir), Mathf.Abs((player.graphicsModule as PlayerGraphics).spearDir));
                            
                            if (i == 1)
                            {
                                vector = Vector2.Lerp(-vector, vector, Mathf.Abs(player.mainBodyChunk.vel.x) / 4.5f);
                            }

                            if (bowStats[playerNumber].isDrawing)
                            {
                                vector = -bowStats[playerNumber].lastAimDir;

                                (player.graphicsModule as PlayerGraphics).hands[i].reachingForObject = true;
                                player.grasps[i].grabbed.firstChunk.MoveFromOutsideMyUpdate(eu, player.mainBodyChunk.pos + bowStats[playerNumber].lastAimDir * 20f);
                                (player.graphicsModule as PlayerGraphics).LookAtPoint(player.grasps[i].grabbed.firstChunk.pos, 10000.0f);
                                (player.graphicsModule as PlayerGraphics).hands[i].absoluteHuntPos = player.grasps[i].grabbed.firstChunk.pos;
                            }

                            (player.grasps[i].grabbed as Weapon).setRotation = vector;
                            (player.grasps[i].grabbed as Weapon).rotationSpeed = 0f;

                            if (i == 0)
                            {
                                int offGrasp = GetOppositeHand(i);
                                PhysicalObject offObject = GetOppositeObject(player, i);

                                if (offObject != null && offObject.abstractPhysicalObject.type == EnumExt_NewItems.Arrow)
                                {
                                    player.grasps[offGrasp].grabbed.firstChunk.MoveFromOutsideMyUpdate(eu, player.grasps[0].grabbed.firstChunk.pos + ((player.grasps[i].grabbed as Weapon).rotation * Mathf.Lerp(-1, 8, (player.grasps[i].grabbed as Bow).drawProgress)));
                                    (player.graphicsModule as PlayerGraphics).hands[offGrasp].reachingForObject = true;
                                    (player.graphicsModule as PlayerGraphics).hands[offGrasp].absoluteHuntPos = player.grasps[0].grabbed.firstChunk.pos + ((player.grasps[i].grabbed as Weapon).rotation * Mathf.Lerp(10, 20, (player.grasps[i].grabbed as Bow).drawProgress));
                                    (player.grasps[offGrasp].grabbed as Weapon).ChangeOverlap((player.grasps[i].grabbed as Weapon).inFrontOfObjects == 1);
                                    (player.grasps[i].grabbed as Bow).drawProgress = Mathf.Pow(bowStats[playerNumber].drawTime / maxDrawTime, 2);

                                    (offObject as Weapon).setRotation = -vector;
                                }
                                else if (offObject == null)
                                {
                                    (player.graphicsModule as PlayerGraphics).hands[offGrasp].reachingForObject = true;
                                    (player.graphicsModule as PlayerGraphics).hands[offGrasp].absoluteHuntPos = player.grasps[0].grabbed.firstChunk.pos + ((player.grasps[i].grabbed as Weapon).rotation * Mathf.Lerp(10, 20, (player.grasps[i].grabbed as Bow).drawProgress));
                                    (player.grasps[i].grabbed as Bow).drawProgress = Mathf.Pow(bowStats[playerNumber].drawTime / maxDrawTime, 2);
                                }
                            }

                            break;
                        case Arrow arrow:
                            if (i == 1)
                            {
                                int offGrasp = GetOppositeHand(i);
                                PhysicalObject offObject = GetOppositeObject(player, i);

                                if (offObject != null && offObject.abstractPhysicalObject.type == EnumExt_NewItems.Bow)
                                {
                                    break;
                                }
                            }

                            player.grasps[i].grabbed.firstChunk.vel = (player.graphicsModule as PlayerGraphics).hands[i].vel;
                            player.grasps[i].grabbed.firstChunk.MoveFromOutsideMyUpdate(eu, (player.graphicsModule as PlayerGraphics).hands[i].pos);

                            if (player.bodyMode == Player.BodyModeIndex.Crawl)
                            {
                                vector = Custom.DirVec(player.bodyChunks[1].pos, Vector2.Lerp(player.grasps[i].grabbed.bodyChunks[0].pos, player.bodyChunks[0].pos, 0.8f));
                            }

                            if (player.animation == Player.AnimationIndex.ClimbOnBeam)
                            {
                                vector.y = Mathf.Abs(vector.y);
                                vector = Vector3.Slerp(vector, Custom.DirVec(player.bodyChunks[1].pos, player.bodyChunks[0].pos), 0.75f);
                            }

                            vector = Vector3.Slerp(vector, Custom.DegToVec((80f + Mathf.Cos((float)(player.animationFrame + ((!player.leftFoot) ? 3 : 9)) / 12f * 2f * (float)Math.PI) * 4f * (player.graphicsModule as PlayerGraphics).spearDir) * (player.graphicsModule as PlayerGraphics).spearDir), Mathf.Abs((player.graphicsModule as PlayerGraphics).spearDir));

                            (player.grasps[i].grabbed as Weapon).setRotation = vector;
                            (player.grasps[i].grabbed as Weapon).rotationSpeed = 0f;

                            break;
                    }
                }
            }
        }

        private static int GetOppositeHand(int grasp)
        {
            return grasp switch
            {
                0 => 1,
                1 => 0,
                _ => -1,
            };
        }

        private static PhysicalObject GetOppositeObject(Player player, int grasp)
        {
            int offGrasp = GetOppositeHand(grasp);
            PhysicalObject offObject;
            
            try
            {
                offObject = player.grasps[offGrasp].grabbed;
            }
            catch
            {
                offObject = null;
            }

            return offObject;
        }

        public static Player.ObjectGrabability GrababilityPatch(On.Player.orig_Grabability orig, Player player, PhysicalObject obj)
        {
            // code that runs before game code

            switch (obj)
            {
                case Club club:
                case Bow bow:
                    if ((obj as Weapon).mode == Weapon.Mode.OnBack)
                    {
                        return Player.ObjectGrabability.CantGrab;
                    }

                    return Player.ObjectGrabability.BigOneHand;
            }

            Player.ObjectGrabability result = orig.Invoke(player, obj);

            // code that runs after game code

            return result;
        }

        public static void ThrowPatch(On.Player.orig_ThrowObject orig, Player player, int grasp, bool eu)
        {
            PhysicalObject thrownObject = player.grasps[grasp].grabbed;
            AbstractPhysicalObject.AbstractObjectType thrownType = thrownObject.abstractPhysicalObject.type;
            RWCustom.IntVector2 throwDir = new RWCustom.IntVector2(player.ThrowDirection, 0);
            int playerNumber = player.playerState.playerNumber;

            if (thrownType == EnumExt_NewItems.Club)
            {
                if (clubStats[playerNumber].swingDelay <= 0 && player.animation != Player.AnimationIndex.Flip && player.animation != Player.AnimationIndex.CrawlTurn && player.animation != Player.AnimationIndex.Roll)
                {
                    player.room.PlaySound(SoundID.Slugcat_Throw_Spear, player.firstChunk);
                    thrownObject.firstChunk.vel.x = thrownObject.firstChunk.vel.x + (float)throwDir.x * 30f;
                    player.room.AddObject(new ExplosionSpikes(player.room, thrownObject.firstChunk.pos + new Vector2((float)player.rollDirection * -40f, 0f), 6, 5.5f, 4f, 4.5f, 21f, new Color(1f, 1f, 1f, 0.25f)));

                    player.bodyChunks[0].vel += throwDir.ToVector2() * 4f;
                    player.bodyChunks[1].vel -= throwDir.ToVector2() * 3f;
                    // animTimer
                    clubStats[playerNumber].comboCooldown = 30;

                    globalStats[playerNumber].animTimer = swingTime;

                    if (clubStats[playerNumber].comboCount >= maxCombo)
                    {
                        clubStats[playerNumber].swingDelay = postComboCooldown;
                        player.room.AddObject(new ExplosionSpikes(player.room, thrownObject.firstChunk.pos + new Vector2((float)player.rollDirection * -40f, 0f), 15, 25f, 4f, 4.5f, 21f, new Color(1f, 0.5f, 0.75f, 0.5f)));
                        clubStats[playerNumber].comboCount = 0;
                    }
                    else
                    {
                        clubStats[playerNumber].swingDelay = swingTime;
                        clubStats[playerNumber].comboCount++;
                    }

                    Vector2 clubTip = (thrownObject.firstChunk.pos + (thrownObject as Weapon).rotation * 50f);

                    SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks((thrownObject as SharedPhysics.IProjectileTracer), player.room, thrownObject.firstChunk.pos, ref clubTip, 10f, player.collisionLayer, player, true);

                    if (collisionResult.obj != null)
                    { 
                        bool arenaHit = false;
                        if (thrownObject.abstractPhysicalObject.world.game.IsArenaSession && thrownObject.abstractPhysicalObject.world.game.GetArenaGameSession.GameTypeSetup.spearHitScore != 0 && player != null && collisionResult.obj is Creature)
                        {
                            arenaHit = true;
                            if ((collisionResult.obj as Creature).State is HealthState && ((collisionResult.obj as Creature).State as HealthState).health <= 0f)
                            {
                                arenaHit = false;
                            }
                            else if (!((collisionResult.obj as Creature).State is HealthState) && (collisionResult.obj as Creature).State.dead)
                            {
                                arenaHit = false;
                            }
                        }

                        if (collisionResult.obj is Creature)
                        {
                            player.room.socialEventRecognizer.WeaponAttack(thrownObject as Club, player, collisionResult.obj as Creature, hit : true);
                            player.room.PlaySound(SoundID.Rock_Hit_Creature, collisionResult.chunk);

                            bool iKilledThis = false;

                            if (((collisionResult.obj as Creature).State as HealthState).health > 0f)
                            {
                                iKilledThis = true;
                            }

                            (collisionResult.obj as Creature).Violence(thrownObject.firstChunk, (thrownObject as Weapon).rotation * thrownObject.firstChunk.mass * 2f, collisionResult.chunk, collisionResult.onAppendagePos, Creature.DamageType.Blunt, globalStats[playerNumber].meleeSkill* 0.6f, 20f);

                            if (((collisionResult.obj as Creature).State as HealthState).health <= 0f && iKilledThis)
                            {
                                player.room.socialEventRecognizer.Killing(player, collisionResult.obj as Creature);
                            }

                            if (arenaHit)
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

            if (thrownType == EnumExt_NewItems.Bow)
            {
                int offGrasp = GetOppositeHand(grasp);
                PhysicalObject offObject = GetOppositeObject(player, grasp);

                if (offObject != null && offObject.abstractPhysicalObject.type != EnumExt_NewItems.Arrow) 
                {
                    orig(player, offGrasp, eu);
                }
                
                return;
            }

            orig(player, grasp, eu);
        }

        public static Vector2 GetAimDir(Player player)
        {
            int playerNumber = player.playerState.playerNumber;
            Vector2 result = playerInput[playerNumber].analogueDir;

            if (result.x != 0f || result.y != 0f)
            {
                result = result.normalized;

                return result;
            }
            if (playerInput[playerNumber].ZeroGGamePadIntVec.x != 0 || playerInput[playerNumber].ZeroGGamePadIntVec.y != 0)
            {
                return playerInput[playerNumber].IntVec.ToVector2().normalized;
            }

            return new Vector2(0f, 0f);
        }

        public static float ReleaseStrengthOnFire(Player player)
        {
            int playerNumber = player.playerState.playerNumber;

            if (bowStats[playerNumber].drawTime / maxDrawTime > 0.1f)
            {

            }


            return 0.01f;
        }
    }
}
