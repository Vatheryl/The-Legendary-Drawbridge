using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Drawbridge.Tiles
{
    public class GoldBellTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;

            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Origin = new Point16(1, 1); // Middle of the second row from the top
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(255, 215, 0));
            DustType = -1;
            HitSound = null;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            // Calculate top-left
            int topLeftX = i - (frameX / 16 % 3);
            int topLeftY = j - (frameY / 16 % 3);
            // Calculate expected clapper center
            float bellBottomCenterX = topLeftX * 16f + 24f;
            float bellBottomCenterY = topLeftY * 16f + 48f;
            float clapperCenterX = bellBottomCenterX;
            float clapperCenterY = bellBottomCenterY - 8f;
            Vector2 expectedClapperPos = new Vector2(clapperCenterX, clapperCenterY);
            // Find and kill matching projectile
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<Projectiles.GoldBellClapper>() && Vector2.DistanceSquared(proj.Center, expectedClapperPos) < 16 * 16)
                {
                    proj.Kill();
                }
            }
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 48, 48, ModContent.ItemType<Items.GoldBellItem>());
        }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            // Calculate the bottom center of the bell
            float bellBottomCenterX = (i - 1) * 16f + 24f;
            float bellBottomCenterY = (j - 1) * 16f + 48f;
            // Position the clapper's center so its top aligns with the bell's bottom, adjusted 1.5 tiles higher
            float clapperCenterY = bellBottomCenterY + 16f - 24f;
            Vector2 clapperCenter = new Vector2(bellBottomCenterX, clapperCenterY);
            Projectile.NewProjectile(null, clapperCenter, Vector2.Zero, ModContent.ProjectileType<Projectiles.GoldBellClapper>(), 0, 0, 255);
        }

        public void TriggerAnimation(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int topLeftX = i - (tile.TileFrameX / 16 % 3);
            int topLeftY = j - (tile.TileFrameY / 16 % 3);
            // Remove static tiles without break effects
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Tile currentTile = Main.tile[topLeftX + x, topLeftY + y];
                    if (currentTile.HasTile && currentTile.TileType == Type)
                    {
                        currentTile.HasTile = false;
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendTileSquare(-1, topLeftX + x, topLeftY + y, 1);
                    }
                }
            }
            // Kill static clapper
            float bellBottomCenterX = topLeftX * 16f + 24f;
            float bellBottomCenterY = topLeftY * 16f + 48f;
            float clapperCenterX = bellBottomCenterX;
            float clapperCenterY = bellBottomCenterY - 8f;
            Vector2 expectedClapperPos = new Vector2(clapperCenterX, clapperCenterY);
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<Projectiles.GoldBellClapper>() && Vector2.DistanceSquared(proj.Center, expectedClapperPos) < 16 * 16)
                {
                    proj.Kill();
                    break;
                }
            }
            // Spawn animation projectile, pass topLeftX and topLeftY as ai[1] and ai[2]
            Vector2 center = new Vector2((topLeftX + 1.5f) * 16f, (topLeftY + 1.5f) * 16f);
            Projectile.NewProjectile(null, center, Vector2.Zero, ModContent.ProjectileType<Projectiles.GoldBellAnimation>(), 0, 0, Main.myPlayer, 0f, topLeftX, topLeftY);
        }

        public override bool RightClick(int i, int j)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                PacketHandler.SendContextPacket(MessageType.ToggleGoldBell, i, j);
            }
            else
            {
                TriggerAnimation(i, j);
            }
            return true;
        }

        public override bool CanDrop(int i, int j) => false;
    }
} 