using Drawbridge.Common;
using Microsoft.Xna.Framework.Graphics;
using NPCUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace Drawbridge.NPCs.Haulox;

internal class Haulox : ModNPC
{
    public const bool Enabled = false;

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

    private static readonly Dictionary<Variants, Asset<Texture2D>> Textures = [];

    public override string Texture => "Terraria/Images/NPC_0";

    internal ref Variants Variant => ref Unsafe.As<float, Variants>(ref NPC.ai[0]);
    internal ref States State => ref Unsafe.As<float, States>(ref NPC.ai[1]);
    private ref float Timer => ref NPC.ai[2];
    private ref float Twomer => ref NPC.ai[3];

    internal ref int GuidingPlayer => ref Unsafe.As<float, int>(ref NPC.localAI[0]);
    internal ref bool Staked => ref Unsafe.As<float, bool>(ref NPC.localAI[1]);
    internal ref int StakeID => ref Unsafe.As<float, int>(ref NPC.localAI[2]);
    internal ref float Threemer => ref NPC.localAI[3];

    private bool Initialized
    {
        get => NPC.localAI[1] == 1;
        set => NPC.localAI[1] = value ? 1 : 0;
    }

    public override bool IsLoadingEnabled(Mod mod) => Enabled;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 24;

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
        NPC.knockBackResist = 0.2f;
    }

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

    public override void UpdateLifeRegen(ref int damage) => NPC.lifeRegen = 4;

    public override void AI()
    {
        if (!Initialized)
        {
            Initialized = true;

            GuidingPlayer = -1;
        }
        
        const float IdleDistance = 100;

        if (GuidingPlayer != -1 && State != States.BeingGuided)
        {
            SetState(States.BeingGuided);
        }

        if (State == States.Idle)
        {
            NPC.velocity.X *= 0.8f;

            if (Timer >= 3)
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

            if (!guide.active || guide.dead)
            {
                SetState(States.Idle);
                GuidingPlayer = -1;
                return;
            }

            const float SlowWalkDistance = 300;

            float distance = guide.DistanceSQ(NPC.Center);

            Twomer++;

            if (distance <= IdleDistance * IdleDistance)
            {
                NPC.velocity.X *= 0.9f;
                Timer = 0;
                Twomer = 0;
            }
            else if (distance <= SlowWalkDistance * SlowWalkDistance)
            {
                NPC.velocity.X = MathF.Sign(guide.Center.X - NPC.Center.X) * 2f;
                Timer = 1;
            }
            else
            {
                NPC.velocity.X = MathF.Sign(guide.Center.X - NPC.Center.X) * (2f + (MathF.Sqrt(distance) - SlowWalkDistance) / 20f);
                Timer = 1;
            }

            if (MathF.Abs(NPC.velocity.X) > 0.1f)
            {
                NPC.spriteDirection = NPC.direction = MathF.Sign(NPC.velocity.X);
            }

            Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);
        }
        else if (State == States.Staked)
        {
            NPC.velocity.X *= 0.8f;
            Vector2 stakeWorld = TileEntity.ByID[StakeID].Position.ToWorldCoordinates();

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
        }
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
        writer.Write(GuidingPlayer);
        writer.Write(Staked);
        writer.Write((short)StakeID);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        GuidingPlayer = reader.ReadInt32();
        Staked = reader.ReadBoolean();
        StakeID = reader.ReadInt16();
    }

    public override void FindFrame(int frameHeight)
    {
        if (State == States.Idle)
        {
            NPC.frameCounter += 0.3f;
            NPC.frame.Y = (int)(NPC.frameCounter * 0.5f) % 10;

            if (NPC.frameCounter > 20)
            {
                NPC.frame.Y = 0;

                if (NPC.frameCounter > 40 + Twomer)
                {
                    NPC.frameCounter = 0;
                    Timer++;
                    Twomer = Main.rand.Next(3, 9) * 20;
                }
            }

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
            if (Timer == 1)
            {
                NPC.frameCounter += Math.Abs(NPC.velocity.X) * 0.75f;
                NPC.frame.Y = (int)(NPC.frameCounter * 0.14f) % 7 + 17;
            }
            else
            {
                NPC.frameCounter = 0;
            }
        }
        else if (State == States.Staked)
        {
            if (Twomer == 0)
            {

            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
        {
            Variant = Variants.Black;
        }

        Texture2D tex = Textures[Variant].Value;
        Rectangle src = new(0, NPC.frame.Y * 70, tex.Width, 70);
        Vector2 pos = NPC.Center - screenPos + new Vector2(0, NPC.gfxOffY + 2);

        spriteBatch.Draw(tex, pos, src, drawColor, 0f, src.Size() / 2f, 1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);
        return false;
    }
}
