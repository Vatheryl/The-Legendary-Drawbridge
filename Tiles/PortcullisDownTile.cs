using Drawbridge.Items;
using Drawbridge.Projectiles;
using Drawbridge.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Drawbridge.Tiles
{
    public class PortcullisDownTile : ModTile, IToggleTile
    {
        public override string Texture => "Drawbridge/Tiles/PortcullisTile";

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileObsidianKill[Type] = false;
            Main.tileLavaDeath[Type] = false;
            Main.tileWaterDeath[Type] = false;
            Main.tileSolid[Type] = false;
            Main.tileSolidTop[Type] = false;
            TileID.Sets.DisableSmartCursor[Type] = true;

            // Make it triggerable by wire
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 8;
            TileObjectData.newTile.Origin = new Point16(0, 7);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 1, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(150, 150, 150), CreateMapEntryName());
            DustType = -1;
            HitSound = null;
        }

        public void Toggle(int i, int j)
            {
                Tile tile = Main.tile[i, j];
                int segment = tile.TileFrameY / 16; // row 0=top .. 7=bottom
                int originX = i;
                int originY = j - segment + 7; // world Y of bottom tile
                for (int y = 0; y < 8; y++)
                {
                    int killY = originY - y;
                    Tile currentTile = Main.tile[originX, killY];
                    if (currentTile.HasTile && currentTile.TileType == Type)
                    {
                        currentTile.HasTile = false;
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendTileSquare(-1, originX, killY, 1);
                    }
                }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Store originX in ai[0] (using bits 1+), raising flag in bit 0
                float ai0 = (originX << 1) | 1; // raising
                float ai1 = originY; // Store originY directly in ai[1]
                Vector2 pivot = new Vector2(originX * 16f + 8f, originY * 16f + 8f);
                Projectile.NewProjectile(null, pivot, Vector2.Zero,
                    ModContent.ProjectileType<PortcullisAnimation>(),
                    0, 0, Main.myPlayer, ai0, ai1);
            }
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
            // Only trigger on the top part of the portcullis
            Tile tile = Main.tile[i, j];
            int segment = tile.TileFrameY / 16;
            if (segment == 0)
            {
                Toggle(i, j);
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            //Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
            //    ModContent.ItemType<PortcullisItem>());
        }
    }
} 