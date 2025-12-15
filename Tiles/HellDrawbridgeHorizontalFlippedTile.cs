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
    public class HellDrawbridgeHorizontalFlippedTile : ModTile, IToggleTile
    {
        public override string Texture => "Drawbridge/Tiles/Hell_DrawbridgeHorizontalFlippedTile";

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileObsidianKill[Type] = true;
            Main.tileLavaDeath[Type] = false;
            Main.tileWaterDeath[Type] = false;
            Main.tileLighted[Type] = true;

            TileID.Sets.DisableSmartCursor[Type] = true;
            TileID.Sets.DrawsWalls[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 15;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.Origin = new Point16(0, 1);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(139, 137, 137), CreateMapEntryName());
            DustType = DustID.Stone;
            HitSound = SoundID.Tink;

            Main.tileSolid[Type] = false;
            Main.tileSolidTop[Type] = true;
            TileID.Sets.Platforms[Type] = true;
            Main.tileMergeDirt[Type] = false;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            // Prevent sloping
            Tile tile = Main.tile[i, j];
            tile.TileFrameX = tile.TileFrameX;
            tile.TileFrameY = tile.TileFrameY;
            return false;
        }

        public override bool CanPlace(int i, int j) => true;

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            // Item drop is handled in KillTile to prevent multiple drops
        }

        private static bool isKillingBridge = false;
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (isKillingBridge || fail)
                return;
            isKillingBridge = true;
            try
            {
                Tile tile = Main.tile[i, j];
                int partX = tile.TileFrameX / 16;
                int partY = tile.TileFrameY / 16;
                int anchorX = i - partX;
                int anchorY = j - partY;

                if (!noItem)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<Items.HellDrawbridgeItem>());
                }
                noItem = true;

                for (int x = anchorX; x < anchorX + 15; x++)
                {
                    for (int y = anchorY; y < anchorY + 2; y++)
                    {
                        if ((x != i || y != j) && Main.tile[x, y].HasTile && Main.tile[x, y].TileType == Type)
                        {
                            WorldGen.KillTile(x, y, false, false, false);
                        }
                    }
                }
            }
            finally
            {
                isKillingBridge = false;
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
            Toggle(i, j);
        }

        // Draw decorative wheel behind drawbridge base when static (horizontal orientation)
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // Static wheel drawing is now handled in WheelRenderSystem.PostDrawTiles
        }

        public void Toggle(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int frameX = tile.TileFrameX;
            int frameY = tile.TileFrameY;
            int segmentX = frameX / 16;
            int segmentY = frameY / 16;
            int removalOriginX = i - segmentX;
            int originX = removalOriginX;
            int originY = j - segmentY + 1;

            for (int x = 0; x < 15; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    Tile currentTile = Main.tile[removalOriginX + x, originY - y];
                    if (currentTile.HasTile && currentTile.TileType == Type)
                    {
                        currentTile.HasTile = false;
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendTileSquare(-1, removalOriginX + x, originY - y, 1, 1);
                    }
                }
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int projType = ModContent.ProjectileType<HellDrawbridgeAnimation>();
                Vector2 pivot = new Vector2(originX * 16f, originY * 16f + 16f);
                // Store originX in ai[0] (using bits 2+), raising flag in bit 0, flipped flag in bit 1
                float ai0 = (originX << 2) | 2; // flipped lowering (not raising, but flipped)
                float ai1 = originY; // Store originY directly in ai[1]
                Projectile.NewProjectile(null, pivot, Vector2.Zero, projType, 0, 0, Main.myPlayer, ai0, ai1);
            }
        }
    }
}