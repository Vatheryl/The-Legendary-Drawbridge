using Drawbridge.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Drawbridge.Tiles;

public class MacchicolationTile : ModTile
{
    public override string Texture => "Drawbridge/Tiles/Machicolation";

    public static bool GetEntity(ref int i, ref int j, out MacchicolationTileEntity entity)
    {
        Tile tile = Main.tile[i, j];

        if (TileEntity.ByPosition.TryGetValue(new Point16(i - tile.TileFrameX % 180 / 18, j - tile.TileFrameY / 18), out TileEntity te) && te is MacchicolationTileEntity mte)
        {
            i -= tile.TileFrameX % 180 / 18;
            j -= tile.TileFrameY / 18;
            entity = mte;
            return true;
        }

        entity = null;
        return false;
    }

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileSolid[Type] = false;
        Main.tileSolidTop[Type] = false;

        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.Width = 10;
        TileObjectData.newTile.Height = 6;
        TileObjectData.newTile.Origin = new Point16(0, 5);
        TileObjectData.newTile.AnchorTop = AnchorData.Empty;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, 9, 1);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16, 16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.CoordinatePadding = 2;
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<MacchicolationTileEntity>().Hook_AfterPlacement, -1, 0, true);
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.StyleHorizontal = true;

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.newAlternate.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, 9, 0);
        TileObjectData.addAlternate(1);

        TileObjectData.addTile(Type);

        AddMapEntry(new Color(139, 69, 19), CreateMapEntryName());
        DustType = DustID.Stone;
        HitSound = SoundID.Tink;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 160, 96, ModContent.ItemType<Items.MacchicolationItem>());
        
        TileExtensions.GetTopLeft(ref i, ref j);
        ModContent.GetInstance<MacchicolationTileEntity>().Kill(i, j);
    }

    public override bool CanDrop(int i, int j) => false;

    public override bool RightClick(int i, int j)
    {
        if (GetEntity(ref i, ref j, out MacchicolationTileEntity mte))
        {
            mte.Activate();

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.TileEntitySharing, number: mte.ID, number2: i, number3: j);

            return true;
        }

        return false;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];

        if (tile == null)
            return true;

        int thX = i;
        int thY = j;

        if (!GetEntity(ref thX, ref thY, out MacchicolationTileEntity mte))
            return true;

        if (mte.State == MacchicolationTileEntity.AnimationState.Off)
            return true;

        bool isTopLeft = tile.TileFrameX is 0 or 180 && tile.TileFrameY == 0;

        if (!isTopLeft)
            return false;

        int frameHeight = 108;

        string texturePath = mte.State switch
        {
            MacchicolationTileEntity.AnimationState.High => "Drawbridge/Tiles/MachicolationHigh",
            _ => Texture,
        };

        Texture2D texture = ModContent.Request<Texture2D>(texturePath, AssetRequestMode.ImmediateLoad).Value;
        Vector2 off = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

        int frame = mte.Frame + mte.State switch
        {
            MacchicolationTileEntity.AnimationState.Low => 1,
            MacchicolationTileEntity.AnimationState.Medium => 5,
            MacchicolationTileEntity.AnimationState.High => 0,
            _ => 0,
        };

        int frameTop = frameHeight * frame;
        int frameXOffset = tile.TileFrameX >= 180 ? 180 : 0;

        const int tilesWide = 10;
        const int tilesHigh = 6;
        const int frameSize = 18;

        for (int y = 0; y < tilesHigh; y++)
        {
            for (int x = 0; x < tilesWide; x++)
            {
                int sourceX = x * frameSize + frameXOffset;
                int sourceY = frameTop + y * frameSize;
                int sw = 16;
                int sh = 16;
                
                Rectangle src = new(sourceX, sourceY, sw, sh);
                Rectangle dest = new((i + x) * 16 - (int)Main.screenPosition.X + (int)off.X,
                    (j + y) * 16 - (int)Main.screenPosition.Y + (int)off.Y,
                    16, 16);
                Color c = Lighting.GetColor(i + x, j + y);
                spriteBatch.Draw(texture, dest, src, c);
            }
        }

        return false;
    }
}

public class MacchicolationTileEntity : ModTileEntity, IClientSideTE
{
    public enum AnimationState
    {
        Off = 0,
        Low = 1,
        Medium = 2,
        High = 3
    }

    public AnimationState State;
    public int Frame;
    public int FrameCounter;
    public int StateTimer;
    public int CooldownTimer; // Cooldown before next use

    private const int CooldownDuration = 10800; // 3 minutes at 60 fps (180 seconds)

    public override void Update()
    {
        if (CooldownTimer > 0)
            CooldownTimer--;

        switch (State)
        {
            case AnimationState.Off:
                return;
            case AnimationState.Low:
                Animate(4, 8);
                if (--StateTimer <= 0)
                    StartMedium();
                break;
            case AnimationState.Medium:
                Animate(8, 6);
                if (--StateTimer <= 0)
                    StartHigh();
                break;
            case AnimationState.High:
                StateTimer++;
                DropWater();

                if (AnimateOnce(30, 3))
                    Stop();
                break;
        }
    }

    void Animate(int frameCount, int ticksPerFrame)
    {
        FrameCounter++;
        if (FrameCounter >= ticksPerFrame)
        {
            FrameCounter = 0;
            Frame++;
            if (Frame >= frameCount)
                Frame = 0;
        }
    }

    bool AnimateOnce(int frameCount, int ticksPerFrame)
    {
        FrameCounter++;
        if (FrameCounter >= ticksPerFrame)
        {
            FrameCounter = 0;
            Frame++;
        }

        return Frame >= frameCount;
    }

    void DropWater()
    {
        // Only drop water on the server or in single player
        if (Main.netMode == NetmodeID.Server)
            return;

        // Only drop water every 10 ticks to reduce the amount
        if (StateTimer < 50 || StateTimer > 60)
            return;

        Tile tile = Main.tile[Position];
        bool left = tile.TileFrameX < 180;
        Vector2 offset = left ? new Vector2(26, 50) : new Vector2(116, 50);
        Vector2 velocity = new Vector2(left ? -4 : 4, Main.rand.NextFloat(2f, 4.5f)) * Main.rand.NextFloat(1, 1.5f);
        int type = ModContent.ProjectileType<MachicolationWater>();

        Projectile.NewProjectile(new EntitySource_TileEntity(this), Position.ToWorldCoordinates() + offset, velocity, type, 30, 0);
    }

    public void Activate()
    {
        if (State != AnimationState.Off)
            return;

        if (CooldownTimer > 0)
        {
            // Display cooldown message to the player
            if (Main.netMode != NetmodeID.Server)
            {
                int secondsRemaining = (int)System.Math.Ceiling(CooldownTimer / 60.0);
                int minutes = secondsRemaining / 60;
                int seconds = secondsRemaining % 60;
                string timeText = minutes > 0 
                    ? $"{minutes}:{seconds:D2}" 
                    : $"{seconds}s";
                Main.NewText($"Machicolation cooling down... ({timeText} remaining)", 255, 100, 100);
            }

            return;
        }

        StartLow();
    }

    void StartLow()
    {
        State = AnimationState.Low;
        Frame = 0;
        FrameCounter = 0;
        StateTimer = 180;
        
        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
    }

    void StartMedium()
    {
        State = AnimationState.Medium;
        Frame = 0;
        FrameCounter = 0;
        StateTimer = 180;

        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
    }

    void StartHigh()
    {
        State = AnimationState.High;
        Frame = 0;
        FrameCounter = 0;
        StateTimer = 0;

        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
    }

    void Stop()
    {
        State = AnimationState.Off;
        Frame = 0;
        FrameCounter = 0;
        StateTimer = 0;
        CooldownTimer = CooldownDuration;

        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
    }

    public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
    {
        TileExtensions.GetTopLeft(ref i, ref j);
        var d = TileObjectData.GetTileData(Main.tile[i, j].TileType, 0);
        var size = d is null ? new Point(1, 1) : new Point(d.Width, d.Height);

        Dust.NewDustPerfect(new Vector2(i + 0.5f, j + 0.5f) * 16, DustID.AncientLight, Vector2.Zero, Scale: 3f).noGravity = true;

        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            NetMessage.SendTileSquare(Main.myPlayer, i, j, size.X, size.Y);
            NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type);
            return -1;
        }

        return Place(i, j);
    }

    public override bool IsTileValidForEntity(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        return tile.HasTile && tile.TileType == ModContent.TileType<MacchicolationTile>() && tile.TileFrameX % 180 == 0 && tile.TileFrameY == 0;
    }

    public override void SaveData(TagCompound tag)
    {
        tag["s"] = (int)State;
        tag["f"] = Frame;
        tag["fc"] = FrameCounter;
        tag["t"] = StateTimer;
        tag["cd"] = CooldownTimer;
    }

    public override void LoadData(TagCompound tag)
    {
        State = (AnimationState)tag.Get<int>("s");
        Frame = tag.Get<int>("f");
        FrameCounter = tag.Get<int>("fc");
        StateTimer = tag.Get<int>("t");
        CooldownTimer = tag.Get<int>("cd");
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write((byte)State);
        writer.Write((short)Frame);
        writer.Write((short)FrameCounter);
        writer.Write((short)StateTimer);
        writer.Write(CooldownTimer);
    }

    public override void NetReceive(BinaryReader reader)
    {
        State = (AnimationState)reader.ReadByte();
        Frame = reader.ReadInt16();
        FrameCounter = reader.ReadInt16();
        StateTimer = reader.ReadInt16();
        CooldownTimer = reader.ReadInt32();
    }
}
