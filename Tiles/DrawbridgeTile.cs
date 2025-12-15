using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Drawbridge.Systems;

namespace Drawbridge.Tiles;

public class DrawbridgeTile : ModTile, IToggleTile
{
    private static bool suppressToggleDrop = false;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileObsidianKill[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileWaterDeath[Type] = false;
        Main.tileSolid[Type] = true;
        Main.tileSolidTop[Type] = false;
        Main.tileLighted[Type] = true;

        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.DrawsWalls[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.Width = 2;
        TileObjectData.newTile.Height = 15;
        TileObjectData.newTile.Origin = new Point16(1, 14); // Bottom-right of 2x15
        TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16 };
        TileObjectData.newTile.CoordinateWidth = 16; // 16px per tile for 32px total
        TileObjectData.newTile.CoordinatePadding = 0;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.UsesCustomCanPlace = true;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(139, 137, 137), CreateMapEntryName());

        DustType = DustID.Stone;
        HitSound = SoundID.Tink;
    }

    public override bool CanPlace(int i, int j)
    {
        // Check if there's enough space and solid ground
        if (j < 14)
            return false; // Need room above

        // Check if bottom is solid under both base tiles
        for (int dx = -1; dx <= 0; dx++)
        {
            Tile bottomTile = Main.tile[i + dx, j + 1];
            if (!bottomTile.HasTile || !Main.tileSolid[bottomTile.TileType])
                return false;
        }

        // Check if 2x15 area is clear
        for (int dx = -1; dx <= 0; dx++)
        {
            for (int dy = 0; dy < 15; dy++)
            {
                Tile tile = Main.tile[i + dx, j - 14 + dy];
                if (tile.HasTile)
                    return false;
            }
        }

        return true;
    }

    public override bool RightClick(int i, int j)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
        {
            PacketHandler.SendContextPacket(MessageType.ToggleDrawbridge, i, j);
        }
        else
        {
            Toggle(i, j);
        }

        return true;
    }

    public override void HitWire(int i, int j)
    {
        // Lever-like behavior: only respond to the top-left tile.
        Tile tile = Main.tile[i, j];
        int partX = tile.TileFrameX / 16;
        int partY = GetVerticalSegmentIndex(tile.TileFrameY);
        if (partX == 0 && partY == 0)
        {
            Toggle(i, j);
        }
    }

    public void Toggle(int i, int j)
    {
        suppressToggleDrop = true;
        Tile tile = Main.tile[i, j];

        // Vertical tile: map frame to segment index
        int frameX = tile.TileFrameX;
        int frameY = tile.TileFrameY;
        int segmentX = frameX / 16; // 16px tile width
        int segmentY = GetVerticalSegmentIndex(frameY);
        int originX = i - segmentX;
        int originY = j - segmentY + 14;


        // Hide the static drawbridge immediately so only the projectile is visible
        // Remove 2x15 vertical multi-tile by manually clearing them to avoid item drops
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 15; y++)
            {
                int tileX = originX + x;
                int tileY = originY - 14 + y; // originY is bottom, iterate up
                Tile currentTile = Main.tile[tileX, tileY];
                if (currentTile.HasTile && currentTile.TileType == Type)
                {
                    currentTile.HasTile = false;
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendTileSquare(-1, tileX, tileY, 1, 1);
                }
            }
        }

        // Start animation
        if (Main.netMode != NetmodeID.MultiplayerClient)
            StartDrawbridgeAnimation(originX, originY, true);
        suppressToggleDrop = false;
    }

    // Map a tile frameY pixel offset into its segment index (0–14)
    private static int GetVerticalSegmentIndex(int frameY)
    {
        int[] heights = [16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16];
        int cum = 0;
        for (int s = 0; s < heights.Length; s++)
        {
            if (frameY < cum + heights[s])
                return s;
            cum += heights[s];
        }

        return heights.Length - 1;
    }

    private static void StartDrawbridgeAnimation(int originX, int originY, bool raising)
    {
        // Spawn projectile at hinge (bottom-right of 2-tile base)
        int projType = ModContent.ProjectileType<Projectiles.DrawbridgeAnimation>();
        Vector2 pivot = new Vector2(originX * 16f + 32f, originY * 16f + 16f);
        // Store originX in ai[0] (using bits 2+), raising flag in bit 0, flipped flag in bit 1
        float ai0 = (originX << 2) | (raising ? 1 : 0); // Normal drawbridge is not flipped
        float ai1 = originY; // Store originY directly in ai[1]
        Projectile.NewProjectile(null, pivot, Vector2.Zero, projType, 0, 0, Main.myPlayer, ai0, ai1);
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        if (suppressToggleDrop)
            return;
        // Drop item when destroyed
        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16,
            ModContent.ItemType<Items.DrawbridgeItem>());
    }
}