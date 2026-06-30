using Drawbridge.Common;
using Drawbridge.Systems.Synchronization;
using Drawbridge.Tiles;
using Microsoft.Xna.Framework.Graphics;
using NPCUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace Drawbridge.NPCs.Haulox;

internal class Haulox : ModNPC
{
    public enum Variants : byte
    {
        None = 0,
        Black,
        Brown,
        Ginger,
        Spotted,
        White,
        Count,
    }

    public enum States : byte
    {
        Idle,
        Grazing,
        Walking,
        BeingGuided,
        Staked,
    }

    public class HasArmor : IItemDropRuleCondition, IProvideItemConditionDescription
    {
        public bool CanDrop(DropAttemptInfo info) => info.npc.ModNPC is Haulox haulox && haulox.ArmorId != -1;
        public bool CanShowItemDropInUI() => false;
        public string GetConditionDescription() => null;
    }

    public class ArmorDrop : IItemDropRule
    {
        List<IItemDropRuleChainAttempt> IItemDropRule.ChainedRules => [];

        public ArmorDrop()
        {
        }

        public virtual bool CanDrop(DropAttemptInfo info) => true;

        public virtual ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
        {
            CommonCode.DropItem(info, (info.npc.ModNPC as Haulox).ArmorId, 1);
            ItemDropAttemptResult result = default;
            result.State = ItemDropAttemptResultState.Success;
            return result;
        }

        public virtual void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
        {
        }
    }

    public class YokeHauloxHandler : Handler
    {
        public override MessageType Message => MessageType.YokeHaulox;

        public static void SendYoked(short npcWho, short yokeId, sbyte slot)
        {
            ModPacket packet = PacketHandler.GetPacket(MessageType.YokeHaulox);
            packet.Write(true);
            packet.Write(npcWho);
            packet.Write(yokeId);
            packet.Write(slot);
            packet.Send();
        }

        public static void SendRelease(short npcWho)
        {
            ModPacket packet = PacketHandler.GetPacket(MessageType.YokeHaulox);
            packet.Write(false);
            packet.Write(npcWho);
            packet.Send();
        }

        public override void Recieve(int whoAmI, BinaryReader reader, bool onServer)
        {
            bool yoked = reader.ReadBoolean();
            short npcWho = reader.ReadInt16();

            NPC npc = Main.npc[npcWho];
            var haul = npc.ModNPC as Haulox;

            if (yoked)
            {
                short yokeId = reader.ReadInt16();
                sbyte slot = reader.ReadSByte();

                haul.GuidingPlayer = whoAmI;
                haul.AttachedYokeId = yokeId;
                haul.StoredSlot = slot;
                haul.SetState(States.BeingGuided);
            }
            else
                haul.GuidingPlayer = -1;

            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npcWho);
        }
    }

    private static readonly Dictionary<Variants, Asset<Texture2D>> Textures = [];

    public override string Texture => "Terraria/Images/NPC_0";

    internal ref Variants Variant => ref Unsafe.As<float, Variants>(ref NPC.ai[0]);
    internal ref States State => ref Unsafe.As<float, States>(ref NPC.ai[1]);
    private ref float Timer => ref NPC.ai[2];
    private ref float Twomer => ref NPC.ai[3];

    internal ref int GuidingPlayer => ref Unsafe.As<float, int>(ref NPC.localAI[0]);
    internal ref bool Staked => ref Unsafe.As<float, bool>(ref NPC.localAI[1]);
    internal ref float Threemer => ref NPC.localAI[3];

    public StakeTile.StakeTileEntity StakeEntity => (StakeTile.StakeTileEntity)TileEntity.ByPosition[StakePosition];

    internal int AttachedYokeId = -1;
    internal sbyte StoredSlot = -1;
    internal bool Initialized = false;
    internal int ArmorId = -1;
    internal Point16 StakePosition = Point16.NegativeOne;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 32;

        NPCID.Sets.CountsAsCritter[Type] = true;

        const string Path = "Drawbridge/NPCs/Haulox/";

        AddTexture(Variants.Black, "Black");
        AddTexture(Variants.Brown, "Brown");
        AddTexture(Variants.Ginger, "Ginger");
        AddTexture(Variants.Spotted, "Spotted");
        AddTexture(Variants.White, "White");

        static void AddTexture(Variants variant, string fix) => Textures.Add(variant, ModContent.Request<Texture2D>(Path + fix + "Haulox"));
    }

    public override void SetDefaults()
    {
        NPC.Size = new(76, 68);
        NPC.aiStyle = -1;
        NPC.lifeMax = 120;
        NPC.defense = 30;
        NPC.damage = 0;
        NPC.knockBackResist = 0.2f;
        NPC.chaseable = false;
    }

    public override bool CheckActive() => !Staked;

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
    {
        bestiaryEntry.AddInfo(this, "Surface");
        bestiaryEntry.UIInfoProvider = new CritterUICollectionInfoProvider(ContentSamples.NpcBestiaryCreditIdsByNpcNetIds[Type]);
    }

    public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
    {
        boundingBox.Width = 98;
        boundingBox.Height = 70;
        boundingBox.X -= boundingBox.Width / 2;
        boundingBox.Y -= boundingBox.Height;
    }

    public override float SpawnChance(NPCSpawnInfo spawnInfo)
    {
        const int Range = 10;

        int x = spawnInfo.SpawnTileX;
        int y = spawnInfo.SpawnTileY;
        float boost = 0;

        for (int i = x - Range; i < x + Range; ++i)
        {
            for (int j = y - Range; j < y + Range; ++j)
            {
                Tile tile = Main.tile[i, j];

                if (tile.HasTile && (tile.TileType == TileID.HayBlock || CrossmodContent.Reforged.EquateType<ModTile>("WheatGrass", tile.TileType)))
                {
                    boost += 0.05f;

                    if (boost > 0.5f)
                        goto exit;
                }
            }
        }

        if (spawnInfo.PlayerInTown)
            boost += 0.05f;

    exit:
        return TileID.Sets.Grass[spawnInfo.SpawnTileType] && y < Main.worldSurface ? boost + 0.05f : 0;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Variant = (Variants)Main.rand.Next((byte)Variants.Count - 1) + 1;
        NPC.netUpdate = true;
    }

    public override void ModifyNPCLoot(NPCLoot npcLoot)
    {
        npcLoot.AddCommon(ItemID.Leather, 1, 2, 5);

        LeadingConditionRule rule = new(new HasArmor());
        rule.OnSuccess(new ArmorDrop());

        npcLoot.Add(rule);
    }

    public override void UpdateLifeRegen(ref int damage) => NPC.lifeRegen = 4;

    public override void AI()
    {
        if (Main.netMode != NetmodeID.Server && Main.LocalPlayer.HeldItem.ModItem is YokeItem)
            NPC.ShowNameOnHover = false;
        else
            NPC.ShowNameOnHover = true;

        if (ArmorId != -1)
            NPC.defense = NPC.defDefense + ContentSamples.ItemsByType[ArmorId].defense;
        else
            NPC.defense = NPC.defDefense;

        if (!Initialized)
        {
            Initialized = true;

            GuidingPlayer = -1;
        }
        
        const float IdleDistance = 100;

        if (GuidingPlayer != -1 && State != States.BeingGuided)
            SetState(States.BeingGuided);

        if (State == States.Idle)
        {
            NPC.velocity.X *= 0.8f;

            if (NPC.justHit)
                SetState(States.Walking);
            else if (Timer >= 3)
            {
                if (HasGrazeableGrass() && Main.rand.NextBool(2))
                    SetState(States.Grazing);
                else
                    SetState(States.Walking);
            }
        }
        else if (State == States.Grazing)
        {
            GrazeFunctionality(ref Timer);
        }
        else if (State == States.Walking)
        {
            Twomer--;

            if (Twomer <= 0)
            {
                SetState(States.Idle);
                return;
            }

            NPC.spriteDirection = NPC.direction = (int)Timer;
            NPC.velocity.X = Timer * 2;

            if (BlockedInDirection(NPC.spriteDirection))
                SetState(States.Idle);

            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
        }
        else if (State == States.BeingGuided)
        {
            if (GuidingPlayer == -1)
            {
                SetState(States.Idle);
                return;
            }

            Player guide = Main.player[GuidingPlayer];
            float distance = guide.DistanceSQ(NPC.Center);

            if (Main.myPlayer == guide.whoAmI && (!guide.active || guide.dead || distance > 600 * 600 || guide.HeldItem.ModItem is not YokeItem || guide.selectedItem != StoredSlot))
            {
                SetState(States.Idle);
                GuidingPlayer = -1;
                guide.GetModPlayer<YokeItem.YokePlayer>().attachedNPC = -1;
                ClearYoke();

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    YokeHauloxHandler.SendRelease((short)NPC.whoAmI);
                    YokeItem.YokePlayerHandler.SendDetatch();
                }

                return;
            }

            const float SlowWalkDistance = 300;

            float offsetX = Math.Abs(NPC.Center.X - guide.Center.X);

            Twomer++;

            if (distance <= IdleDistance * IdleDistance || offsetX < 12)
            {
                NPC.velocity.X *= 0.9f;
                Timer = 0;
                Twomer = 0;
            }
            else if (distance <= SlowWalkDistance * SlowWalkDistance)
            {
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, MathF.Sign(guide.Center.X - NPC.Center.X) * 2f, 0.2f);
                Timer = 1;
            }
            else
            {
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, MathF.Sign(guide.Center.X - NPC.Center.X) * (2f + (MathF.Sqrt(distance) - SlowWalkDistance) / 20f), 0.2f);
                Timer = 1;
            }

            if (MathF.Abs(NPC.velocity.X) > 0.1f && offsetX > 4)
                NPC.spriteDirection = NPC.direction = MathF.Sign(NPC.velocity.X);

            GuidingJump();

            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
        }
        else if (State == States.Staked)
        {
            NPC.velocity.X *= 0.8f;

            if (!TileEntity.ByPosition.ContainsKey(StakePosition))
            {
                Staked = false;
                StakePosition = Point16.NegativeOne;
                SetState(States.Idle);
                return;
            }

            Vector2 stakeWorld = StakeEntity.Position.ToWorldCoordinates();

            Timer++;

            if (Twomer == 0 && Timer > 60) // Idle; do nothing
                SetState(States.Staked);
            else if (Twomer == 1) // Walk for a short distance, randomly directed & lengthed
            {
                if (Timer == 60)
                {
                    Threemer = (Main.rand.NextBool() ? -1 : 1) * Main.rand.NextFloat(40, 80);

                    if (NPC.DistanceSQ(stakeWorld) > IdleDistance * IdleDistance)
                        Threemer = Math.Sign(stakeWorld.X - NPC.Center.X) * 60;
                }
                else if (Timer > 60 && Timer < 60 + Math.Abs(Threemer))
                {
                    if (NPC.DistanceSQ(stakeWorld) > IdleDistance * IdleDistance && Timer > 70)
                        Timer = 60 + Math.Abs(Threemer);
                    else
                        NPC.velocity.X = Math.Sign(Threemer) * 2;
                }
                else if (Timer >= 60 + Math.Abs(Threemer))
                {
                    SetState(States.Staked);
                }
            }
            else if (Twomer == 2) // Graze
            {
                if (Timer == 40)
                {
                    Threemer = Main.rand.NextFloat(80);
                }
                else if (Timer >= 40 + Threemer)
                    SetState(States.Staked);
            }

            if (MathF.Abs(NPC.velocity.X) > 0.1f)
                NPC.spriteDirection = NPC.direction = MathF.Sign(NPC.velocity.X);
        }
    }

    private void ClearYoke()
    {
        SoundEngine.PlaySound(SoundID.Dig with { PitchRange = (-0.3f, 0.3f) }, NPC.Center);
        AttachedYokeId = -1;
        StoredSlot = -1;
    }

    private void GuidingJump()
    {
        bool wall = NPC.spriteDirection == -1 ? Collision.SolidCollision(NPC.TopLeft - new Vector2(6, 0), 6, 14) : Collision.SolidCollision(NPC.TopRight, 6, 14);
        wall |= Main.player[GuidingPlayer].Center.Y < NPC.Center.Y - 120;

        if (wall && (Collision.SolidCollision(NPC.BottomLeft, NPC.width, 6) || PlatformCollision(NPC.BottomLeft - new Vector2(4, 0), NPC.width + 8, 6)))
            NPC.velocity.Y = -7;
    }

    private void GrazeFunctionality(ref float timer)
    {
        NPC.velocity.X *= 0.6f;

        if (timer <= 0)
            SetState(States.Idle);

        if (timer > 10 && NPC.frameCounter > 15 && NPC.frameCounter % 26 == 15)
        {
            Point16 pos = GetEatingTilePosition();
            Collision.HitTiles(pos.ToWorldCoordinates(), new Vector2(0, -2), 4, 4);

            if (!Main.dedServ)
                SoundEngine.PlaySound(SoundID.Grass with { Volume = 0.2f, PitchRange = (-1f, -0.4f), MaxInstances = 6 });
        }
    }

    private bool BlockedInDirection(int dir) => dir == -1 
        ? Collision.SolidCollision(NPC.position - new Vector2(6, 0), 6, NPC.height / 2)
        : Collision.SolidCollision(NPC.TopRight, 6, NPC.height / 2);

    private bool HasGrazeableGrass()
    {
        Point16 pos = GetEatingTilePosition();
        Tile tile = Main.tile[pos];

        return TileID.Sets.Grass[tile.TileType] && tile.HasTile && !TileID.Sets.Corrupt[tile.TileType] && !TileID.Sets.Crimson[tile.TileType];
    }

    private Point16 GetEatingTilePosition() => new(NPC.spriteDirection == -1 ? (int)(NPC.position.X / 16f) : (int)(NPC.Right.X / 16f), (int)(NPC.Bottom.Y + 8) / 16);

    internal void SetState(States state)
    {
        State = state;
        NPC.frameCounter = 0;
        Timer = 0;
        Twomer = 0;
        Threemer = 0;

        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        NPC.netUpdate = true;

        if (State == States.Grazing)
            Timer = Main.rand.Next(120, 280);
        else if (State == States.Walking)
        {
            PriorityQueue<int, float> decisions = new();

            if (!BlockedInDirection(-1))
                decisions.Enqueue(-1, Main.rand.NextFloat());

            if (!BlockedInDirection(1))
                decisions.Enqueue(1, Main.rand.NextFloat());

            if (decisions.Count == 0)
            {
                State = States.Idle;
                Timer = 0;
                return;
            }

            Timer = decisions.Dequeue();
            Twomer = Main.rand.Next(120, 180);
        }
        else if (State == States.Staked)
            Twomer = Main.rand.Next(3);
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write((short)GuidingPlayer);
        writer.Write(StoredSlot);
        writer.Write(StakePosition.X);
        writer.Write(StakePosition.Y);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        GuidingPlayer = reader.ReadInt16();
        StoredSlot = reader.ReadSByte();
        StakePosition = new Point16(reader.ReadInt16(), reader.ReadInt16());
    }

    public override void FindFrame(int frameHeight)
    {
        if (State == States.Idle || NPC.IsABestiaryIconDummy)
        {
            IdleAnimation(true);

            if (NPC.IsABestiaryIconDummy)
            {
                Timer++;

                if (Timer > 240)
                {
                    Variant++;

                    if (Variant >= Variants.Count)
                        Variant = Variants.Black;

                    Timer = 0;
                }
            }
        }
        else if (State == States.Grazing)
        {
            NPC.frameCounter++;

            if (NPC.frameCounter <= 15)
            {
                NPC.frame.Y = (int)(NPC.frameCounter / 5f) + 9;
            }
            else
            {
                NPC.frame.Y = (int)(NPC.frameCounter * 0.08f) % 2 + 12;
                Timer--;

                if (Timer < 10)
                {
                    NPC.frame.Y = 1 - (int)(Timer / 5f) + 14;
                }
            }
        }
        else if (State == States.Walking)
        {
            NPC.frameCounter++;
            NPC.frame.Y = (int)(NPC.frameCounter * 0.14f) % 7 + 17;
        }
        else if (State == States.BeingGuided)
        {
            if (CheckJumping(out int frame))
            {
                NPC.frame.Y = frame;
            }
            else
            {
                if (Timer == 1)
                {
                    NPC.frameCounter += Math.Abs(NPC.velocity.X) * 0.75f;
                    NPC.frame.Y = (int)(NPC.frameCounter * 0.14f) % 7 + 17;
                }
                else
                {
                    IdleAnimation(false);
                }
            }
        }
        else if (State == States.Staked)
        {
            if (Math.Abs(NPC.velocity.X) < 0.1f)
                IdleAnimation(false);
            else
            {
                NPC.frameCounter++;
                NPC.frame.Y = (int)(NPC.frameCounter * 0.14f) % 7 + 17;
            }
        }
    }

    private void IdleAnimation(bool behavior)
    {
        NPC.frameCounter += 0.3f;
        NPC.frame.Y = (int)(NPC.frameCounter * 0.5f) % 10;

        if (NPC.frameCounter > 20)
        {
            NPC.frame.Y = 0;

            if (NPC.frameCounter > 40 + Twomer)
            {
                NPC.frameCounter = 0;

                if (behavior)
                    Timer++;

                Twomer = Main.rand.Next(3, 9) * 20;
            }
        }
    }

    private bool CheckJumping(out int frame)
    {
        frame = 0;

        if (Collision.SolidCollision(NPC.BottomLeft, NPC.width, 8, true) || PlatformCollision(NPC.BottomLeft - new Vector2(6, 0), NPC.width + 12, 8))
            return false;

        if (NPC.velocity.Y < 0)
        {
            frame = NPC.velocity.Y switch
            {
                < -5 => 24,
                < -3 => 25,
                < -1f => 26,
                _ => 27,
            };

            return true;
        }

        frame = NPC.velocity.Y switch
        {
            <= 1f => 27,
            < 3 => 28,
            _ => 29,
        };

        return true;
    }

    public static bool PlatformCollision(Vector2 Position, int Width, int Height)
    {
        int value = (int)(Position.X / 16f) - 1;
        int value2 = (int)((Position.X + Width) / 16f) + 2;
        int value3 = (int)(Position.Y / 16f) - 1;
        int value4 = (int)((Position.Y + Height) / 16f) + 2;
        int num = Utils.Clamp(value, 0, Main.maxTilesX - 1);
        value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
        value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 1);
        value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 1);
        Vector2 vector = default;

        for (int i = num; i < value2; i++)
        {
            for (int j = value3; j < value4; j++)
            {
                Tile tile = Main.tile[i, j];
                if (tile == null || !tile.HasTile || tile.IsActuated)
                    continue;

                bool flag = TileID.Sets.Platforms[tile.TileType];

                if (flag)
                {
                    vector.X = i * 16;
                    vector.Y = j * 16;
                    int num2 = 16;

                    if (Position.X + Width > vector.X && Position.X < vector.X + 16f && Position.Y + Height > vector.Y && Position.Y < vector.Y + num2)
                        return true;
                }
            }
        }

        return false;
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        if (!Main.dedServ)
        {
            bool dead = false;

            if (NPC.life <= 0)
            {
                dead = true;
                string variant = Variant.ToString();

                if (variant == Variants.Ginger.ToString())
                    variant = "Orange";

                int bodyGore = Mod.Find<ModGore>("Haulox_Gore_Body_" + variant).Type;
                int hornGore = Mod.Find<ModGore>("Haulox_Gore_Horn").Type;

                if (variant == "Spotted")
                {
                    bodyGore = Mod.Find<ModGore>("Haulox_Gore_Body_" + variant).Type;
                    variant = Variants.White.ToString();
                }

                int headGore = Mod.Find<ModGore>("Haulox_Gore_Head_" + variant).Type;
                int leg1Gore = Mod.Find<ModGore>("Haulox_Gore_Leg1_" + variant).Type;
                int leg2Gore = Mod.Find<ModGore>("Haulox_Gore_Leg2_" + variant).Type;
                int tailGore = Mod.Find<ModGore>("Haulox_Gore_Tail_" + variant).Type;

                Gore.NewGorePerfect(NPC.GetSource_Death(), NPC.Center, NPC.velocity, bodyGore);
                Gore.NewGorePerfect(NPC.GetSource_Death(), NPC.Center + DirectionalOffset(38, -14), NPC.velocity, hornGore);
                Gore.NewGorePerfect(NPC.GetSource_Death(), NPC.Center + DirectionalOffset(38, -22), NPC.velocity, headGore);
                Gore.NewGorePerfect(NPC.GetSource_Death(), NPC.Center + DirectionalOffset(14, 22), NPC.velocity, leg1Gore);
                Gore.NewGorePerfect(NPC.GetSource_Death(), NPC.Center + DirectionalOffset(-22, 22), NPC.velocity, leg2Gore);
                Gore.NewGorePerfect(NPC.GetSource_Death(), NPC.Center + DirectionalOffset(-40, 10), NPC.velocity, tailGore);

                Vector2 DirectionalOffset(float x, float y) => new(NPC.spriteDirection * x - 16, y);
            }

            for (int i = 0; i < (dead ? 8 : 28); ++i)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Blood, NPC.velocity.X, NPC.velocity.Y, Scale: dead ? Main.rand.NextFloat(1, 2.5f) : 1);
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
            Variant = Variants.Black;

        Texture2D tex = Textures[Variant].Value;
        Rectangle src = new(0, NPC.frame.Y * 94, tex.Width, 92);
        Vector2 pos = NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY + 2 - 12);

        spriteBatch.Draw(tex, pos, src, drawColor, 0f, src.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

        if (ArmorId != -1 && HauloxArmor.ArmorSheetsById.TryGetValue(ArmorId, out var asset))
            spriteBatch.Draw(asset.Value, pos, src, drawColor, 0f, src.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

        return false;
    }
}
