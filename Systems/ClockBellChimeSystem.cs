using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Drawbridge.Tiles;
using System;

namespace Drawbridge.Systems
{
    public class ClockBellChimeSystem : ModSystem
    {
        private bool wasDay = true;
        private double lastTime = 0;

        public override void PostUpdateWorld()
        {
            // Detect day/night transition
            bool isDay = Main.dayTime;
            double time = Main.time;

            // Morning: transition from night to day (time resets to 0)
            if (!wasDay && isDay && time < lastTime)
            {
                ChimeAllConnectedBells(true); // Morning
            }
            // Night: transition from day to night (time resets to 0)
            else if (wasDay && !isDay && time < lastTime)
            {
                ChimeAllConnectedBells(false); // Night
            }

            wasDay = isDay;
            lastTime = time;
        }

        private void ChimeAllConnectedBells(bool isMorning)
        {
            int clockType = ModContent.TileType<ClockTile>();
            int goldBellType = ModContent.TileType<GoldBellTile>();
            int graveyardBellType = ModContent.TileType<GraveyardBellTile>();
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = 0; j < Main.maxTilesY; j++)
                {
                    Tile tile = Main.tile[i, j];
                    if (tile == null || !tile.HasTile || tile.TileType != clockType)
                        continue;
                    if (tile.TileFrameX != 0 || tile.TileFrameY != 0)
                        continue;
                    var clockTiles = new Tile[4, 4];
                    for (int cx = 0; cx < 4; cx++)
                        for (int cy = 0; cy < 4; cy++)
                            clockTiles[cx, cy] = Main.tile[i + cx, j + cy];
                    // Scan for both bell types
                    for (int bx = 0; bx < Main.maxTilesX; bx++)
                    {
                        for (int by = 0; by < Main.maxTilesY; by++)
                        {
                            Tile bellTile = Main.tile[bx, by];
                            if (bellTile == null || !bellTile.HasTile) continue;
                            if (bellTile.TileType != goldBellType && bellTile.TileType != graveyardBellType) continue;
                            if (bellTile.TileFrameX != 0 || bellTile.TileFrameY != 0) continue;
                            var bellTiles = new Tile[3, 3];
                            for (int dx = 0; dx < 3; dx++)
                                for (int dy = 0; dy < 3; dy++)
                                    bellTiles[dx, dy] = Main.tile[bx + dx, by + dy];
                            if (AnyTilesShareWire(clockTiles, bellTiles))
                            {
                                TriggerBell(bx, by, bellTile.TileType);
                            }
                        }
                    }
                }
            }
        }

        private bool AnyTilesShareWire(Tile[,] clockTiles, Tile[,] bellTiles)
        {
            for (int cx = 0; cx < clockTiles.GetLength(0); cx++)
            {
                for (int cy = 0; cy < clockTiles.GetLength(1); cy++)
                {
                    Tile ct = clockTiles[cx, cy];
                    if (ct == null || !ct.HasTile) continue;
                    for (int bx = 0; bx < bellTiles.GetLength(0); bx++)
                    {
                        for (int by = 0; by < bellTiles.GetLength(1); by++)
                        {
                            Tile bt = bellTiles[bx, by];
                            if (bt == null || !bt.HasTile) continue;
                            if ((ct.RedWire && bt.RedWire) || (ct.BlueWire && bt.BlueWire) || (ct.GreenWire && bt.GreenWire) || (ct.YellowWire && bt.YellowWire))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        private void TriggerBell(int i, int j, ushort bellTileType)
        {
            Tile tile = Main.tile[i, j];
            int topLeftX = i - (tile.TileFrameX / 16 % 3);
            int topLeftY = j - (tile.TileFrameY / 16 % 3);
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Tile currentTile = Main.tile[topLeftX + x, topLeftY + y];
                    if (currentTile.HasTile && currentTile.TileType == bellTileType)
                    {
                        currentTile.HasTile = false;
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendTileSquare(-1, topLeftX + x, topLeftY + y, 1);
                    }
                }
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 center = new Vector2((topLeftX + 1.5f) * 16f, (topLeftY + 1.5f) * 16f);
                if (bellTileType == ModContent.TileType<GoldBellTile>())
                {
                    Terraria.Projectile.NewProjectile(null, center, Vector2.Zero, ModContent.ProjectileType<Projectiles.GoldBellAnimation>(), 0, 0, Main.myPlayer, 0f, topLeftX, topLeftY);
                }
                else if (bellTileType == ModContent.TileType<GraveyardBellTile>())
                {
                    Terraria.Projectile.NewProjectile(null, center, Vector2.Zero, ModContent.ProjectileType<Projectiles.GraveyardBellAnimation>(), 0, 0, Main.myPlayer, 0f, topLeftX, topLeftY);
                }
            }
            float bellBottomCenterX = topLeftX * 16f + 24f;
            float bellBottomCenterY = topLeftY * 16f + 48f;
            float clapperCenterX = bellBottomCenterX;
            float clapperCenterY = bellBottomCenterY - 8f;
            Vector2 expectedClapperPos = new Vector2(clapperCenterX, clapperCenterY);
            foreach (Terraria.Projectile proj in Main.projectile)
            {
                if (bellTileType == ModContent.TileType<GoldBellTile>())
                {
                    if (proj.active && proj.type == ModContent.ProjectileType<Projectiles.GoldBellClapper>() && Vector2.DistanceSquared(proj.Center, expectedClapperPos) < 16 * 16)
                    {
                        proj.Kill();
                    }
                }
                else if (bellTileType == ModContent.TileType<GraveyardBellTile>())
                {
                    if (proj.active && proj.type == ModContent.ProjectileType<Projectiles.GraveyardBellClapper>() && Vector2.DistanceSquared(proj.Center, expectedClapperPos) < 16 * 16)
                    {
                        proj.Kill();
                    }
                }
            }
        }
    }
} 