using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Audio;
using Drawbridge.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Drawbridge.Systems;

namespace Drawbridge.Tiles
{
    public class DrawbridgeHorizontalTile : ModTile, IToggleTile
    {
        public override string Texture => "Drawbridge/Tiles/DrawbridgeHorizontalTile";

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

            // Horizontal drawbridge: 15 tiles wide, 2 tiles tall
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 15;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.Origin = new Point16(14, 1); // Bottom-right of 15x2
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 }; // 16px per tile for 32px total
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(139, 137, 137), CreateMapEntryName());
            DustType = DustID.Stone;
            HitSound = SoundID.Tink;

            // Platform behavior: non-solid but solid-top, never breaks on unsupported
            Main.tileSolid[Type] = false;
            Main.tileSolidTop[Type] = true;
            TileID.Sets.Platforms[Type] = true;
            Main.tileMergeDirt[Type] = false;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            // This prevents the platform from being sloped or connecting to other tiles
            // by forcing it to keep its original frame.
            Tile tile = Main.tile[i, j];
            tile.TileFrameX = tile.TileFrameX;
            tile.TileFrameY = tile.TileFrameY;
            return false; // Return false to prevent vanilla framing logic
        }

        public override bool CanPlace(int i, int j)
        {
            // Allow forced placement by code
            return true;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            // Item drop is handled in KillTile to prevent multiple drops
        }

        private static bool isKillingBridge = false;
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (isKillingBridge || fail)
            {
                return;
            }

            isKillingBridge = true;
            try
            {
                Tile tile = Main.tile[i, j];
                int partX = tile.TileFrameX / 16;
                int partY = tile.TileFrameY / 16;
                int anchorX = i - partX;
                int anchorY = j - partY;

                // We must set noItem to true before recursively calling KillTile, 
                // so that we only drop one item.
                if (!noItem)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<Items.DrawbridgeItem>());
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

        public void Toggle(int i, int j)
        {
                // Determine which segment was clicked
                Tile tile = Main.tile[i, j];
                int frameX = tile.TileFrameX;
                int frameY = tile.TileFrameY;
                int segmentX = frameX / 16;
                int segmentY = frameY / 16;
                // Compute removal origin (rightmost of the horizontal board)
                int removalOriginX = i - segmentX + 14;
                // Compute pivot origin (leftmost of the hinge base)
                int originX = removalOriginX - 1;
                int originY = j - segmentY + 1;
                // Hide the static horizontal drawbridge tiles
                for (int x = 0; x < 15; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        Tile currentTile = Main.tile[removalOriginX - x, originY - y];
                        if (currentTile.HasTile && currentTile.TileType == Type)
                        {
                            currentTile.HasTile = false;
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendTileSquare(-1, removalOriginX - x, originY - y, 1, 1);
                        }
                    }
                }
                // The projectile will now handle the sound
                // SoundEngine.PlaySound(new SoundStyle("Drawbridge/Sounds/DrawbridgeMove") { Volume = 0.8f, Pitch = 0f, MaxInstances = 1 },
                //     new Vector2(originX * 16, originY * 16));
                // Spawn animation (raising = false), packing hinge in ai[1]
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int projType = ModContent.ProjectileType<DrawbridgeAnimation>();
                Vector2 pivot = new Vector2(originX * 16f + 32f, originY * 16f + 16f);
                // Store originX in ai[0] (using bits 2+), raising flag in bit 0, flipped flag in bit 1
                float ai0 = (originX << 2) | 0; // lowering (not raising, not flipped)
                float ai1 = originY; // Store originY directly in ai[1]
                Projectile.NewProjectile(null, pivot, Vector2.Zero, projType, 0, 0, Main.myPlayer, ai0, ai1);
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
    }
} 