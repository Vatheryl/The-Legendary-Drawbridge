using Drawbridge.Items;
using Drawbridge.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Drawbridge.Tiles
{
    public class LionDrawbridgeTile : ModTile, IToggleTile
    {
        public override string Texture => "Drawbridge/Tiles/Lion_Drawbridge";

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
            
            // Make it triggerable by wire
            Main.tileLighted[Type] = true;

            TileID.Sets.DisableSmartCursor[Type] = true;
            TileID.Sets.DrawsWalls[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 15;
            TileObjectData.newTile.Origin = new Point16(1, 14);
            TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16, 16];
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(212, 175, 55), CreateMapEntryName());
            
            DustType = DustID.Stone;
            HitSound = SoundID.Tink;
        }

        public override bool CanPlace(int i, int j)
        {
            if (j < 14) return false;
            for (int dx = -1; dx <= 0; dx++)
            {
                Tile bottomTile = Main.tile[i + dx, j + 1];
                if (!bottomTile.HasTile || !Main.tileSolid[bottomTile.TileType])
                    return false;
            }
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
                PacketHandler.SendContextPacket(MessageType.ToggleDrawbridge, i, j);
            else
                Toggle(i, j);
            return true;
        }

        public override void HitWire(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int partX = tile.TileFrameX / 16;
            int partY = GetVerticalSegmentIndex(tile.TileFrameY);
            if (partX == 0 && partY == 0)
                Toggle(i, j);
        }

        public void Toggle(int i, int j)
        {
            suppressToggleDrop = true;
            Tile tile = Main.tile[i, j];
            int frameX = tile.TileFrameX;
            int frameY = tile.TileFrameY;
            int segmentX = frameX / 16;
            int segmentY = GetVerticalSegmentIndex(frameY);
            int originX = i - segmentX;
            int originY = j - segmentY + 14;

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 15; y++)
                {
                    int tx = originX + x;
                    int ty = originY - 14 + y;
                    Tile currentTile = Main.tile[tx, ty];
                    if (currentTile.HasTile && currentTile.TileType == Type)
                    {
                        currentTile.HasTile = false;
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendTileSquare(-1, tx, ty, 1, 1);
                    }
                }
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
                StartDrawbridgeAnimation(originX, originY, true);
            suppressToggleDrop = false;
        }

        private int GetVerticalSegmentIndex(int frameY)
        {
            int[] heights = new int[] { 16,16,16,16,16,16,16,16,16,16,16,16,16,16,16 };
            int cum = 0;
            for (int s = 0; s < heights.Length; s++)
            {
                if (frameY < cum + heights[s])
                    return s;
                cum += heights[s];
            }
            return heights.Length - 1;
        }

        private void StartDrawbridgeAnimation(int originX, int originY, bool raising)
        {
            int projType = ModContent.ProjectileType<Projectiles.LionDrawbridgeAnimation>();
            Vector2 pivot = new Vector2(originX * 16f + 32f, originY * 16f + 16f);
            // Store originX in ai[0] (using bits 2+), raising flag in bit 0, flipped flag in bit 1  
            float ai0 = (originX << 2) | (raising ? 1 : 0); // Normal drawbridge is not flipped
            float ai1 = originY; // Store originY directly in ai[1]
            Projectile.NewProjectile(null, pivot, Vector2.Zero, projType, 0, 0, Main.myPlayer, ai0, ai1);
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (suppressToggleDrop)
                return;
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16,
                ModContent.ItemType<Items.LionDrawbridgeItem>());
        }
    }
}