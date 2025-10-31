using Drawbridge.Items;
using Drawbridge.Projectiles;
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
    public class HellDrawbridgeVerticalFlippedTile : ModTile, IToggleTile
    {
        public override string Texture => "Drawbridge/Tiles/Hell_DrawbridgeFlipped";

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

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 15;
            TileObjectData.newTile.Origin = new Point16(0, 14);
            TileObjectData.newTile.CoordinateHeights = new int[] {16,16,16,16,16,16,16,16,16,16,16,16,16,16,16};
            TileObjectData.newTile.CoordinateWidth = 16;
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
            if (j < 14) return false;
            for (int dx = 0; dx <= 1; dx++)
            {
                Tile bottomTile = Main.tile[i + dx, j + 1];
                if (!bottomTile.HasTile || !Main.tileSolid[bottomTile.TileType])
                    return false;
            }
            for (int dx = 0; dx <= 1; dx++)
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
            int partY = tile.TileFrameY / 16;
            if (partX == 0 && partY == 0)
                Toggle(i, j);
        }

        public void Toggle(int i, int j)
        {
            int frameX = Main.tile[i, j].TileFrameX;
            int frameY = Main.tile[i, j].TileFrameY;
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
                    Tile t = Main.tile[tx, ty];
                    if (t.HasTile && t.TileType == Type)
                    {
                        t.HasTile = false;
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendTileSquare(-1, tx, ty, 1, 1);
                    }
                }
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int projType = ModContent.ProjectileType<Projectiles.HellDrawbridgeAnimation>();
                Vector2 pivot = new Vector2(originX * 16f + 32f, originY * 16f + 16f);
                // Store originX in ai[0] (using bits 2+), raising flag in bit 0, flipped flag in bit 1
                float ai0 = (originX << 2) | 1 | 2; // raising + flipped
                float ai1 = originY; // Store originY directly in ai[1]
                Projectile.NewProjectile(null, pivot, Vector2.Zero, projType, 0, 0, Main.myPlayer, ai0, ai1);
            }
        }

        private int GetVerticalSegmentIndex(int frameY)
        {
            int cum = 0;
            const int segmentCount = 15;
            for (int s = 0; s < segmentCount; s++)
            {
                if (frameY < cum + 16)
                    return s;
                cum += 16;
            }
            return segmentCount - 1;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 16,
                ModContent.ItemType<Items.HellDrawbridgeItem>());
        }
    }
}