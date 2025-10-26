using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using Drawbridge.Projectiles;
using System;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Drawbridge.Players;
using Drawbridge.Systems;

namespace Drawbridge.Tiles
{
    public class HorseStableTile : ModTile
    {
        // Track the last right-clicked stable for the local player
        private static Point16? lastRightClickedStable = null;

        // Add a static timer for horse movement
        private static double lastHorseMoveTime = 0;

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
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 9; 
            TileObjectData.newTile.Height = 6;
            TileObjectData.newTile.Origin = new Point16(4, 5); // Center bottom
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16 }; // All rows are 16px for 96px total
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = false;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(180, 140, 90), CreateMapEntryName());
            DustType = DustID.WoodFurniture;
            HitSound = SoundID.Dig;
        }

        public override bool RightClick(int i, int j)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                // Find the center of the stable
                Tile tile = Main.tile[i, j];
                int frameX = tile.TileFrameX / 16;
                int frameY = tile.TileFrameY / 16;
                int topLeftX = i - frameX + 4; // Origin is (4,5)
                int topLeftY = j - frameY + 2; // Center vertically (6 tiles high)
                // Toggle the icon overlay for this stable
                Point16 thisStable = new Point16(topLeftX, topLeftY);
                if (StableSlotUISystem.lastRightClickedStable.HasValue && StableSlotUISystem.lastRightClickedStable.Value == thisStable)
                {
                    // Close overlay
                    StableSlotUISystem.lastRightClickedStable = null;
                    Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuClose);
                }
                else
                {
                    // Open overlay
                    StableSlotUISystem.lastRightClickedStable = thisStable;
                    Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuOpen);
                }
            }
            return true;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // Slot UI is now handled by StableSlotUISystem
        }

        // Add this method to enforce mount removal when no slot is filled
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // Find the center of the stable
            Tile tile = Main.tile[i, j];
            int frameX = tile.TileFrameX / 16;
            int frameY = tile.TileFrameY / 16;
            int topLeftX = i - frameX + 4; // Origin is (4,5)
            int topLeftY = j - frameY + 2; // Center vertically (6 tiles high)
            float stableCenterX = (topLeftX + 4.5f) * 16f - 64f;
            float stableCenterY = (topLeftY + 3f) * 16f + 12f;

            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<DrawbridgePlayer>();
            // Get or create the slot set for this stable
            if (!modPlayer.stableSlotsPerStable.TryGetValue(new Point16(topLeftX, topLeftY), out var slotSet))
            {
                slotSet = new DrawbridgePlayer.StableSlots();
                modPlayer.stableSlotsPerStable[new Point16(topLeftX, topLeftY)] = slotSet;
            }
            int horseType = -1;
            if (slotSet.BlessedApple != null && !slotSet.BlessedApple.IsAir)
                horseType = 0; // Unicorn
            else if (slotSet.BlackSaddle != null && !slotSet.BlackSaddle.IsAir)
                horseType = 3; // Dark
            else if (slotSet.DustySaddle != null && !slotSet.DustySaddle.IsAir)
                horseType = 1; // Painted
            else if (slotSet.RoyalSaddle != null && !slotSet.RoyalSaddle.IsAir)
                horseType = 2; // Majestic

            // Remove any existing horse at this stable if no slot is filled or type doesn't match
            StableUnicornMount foundHorse = null;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<StableUnicornMount>())
                {
                    if (System.Math.Abs(proj.ai[1] - stableCenterX) < 16f && System.Math.Abs(proj.Center.Y - stableCenterY) < 32f)
                    {
                        if (horseType == -1 || (int)proj.ai[2] != horseType)
                        {
                            proj.Kill();
                        }
                        else
                        {
                            foundHorse = proj.ModProjectile as StableUnicornMount;
                        }
                    }
                }
            }
            // Spawn the horse if needed
            if (horseType != -1 && foundHorse == null)
            {
                int projType = ModContent.ProjectileType<StableUnicornMount>();
                int idx = Projectile.NewProjectile(null, new Microsoft.Xna.Framework.Vector2(stableCenterX, stableCenterY), Microsoft.Xna.Framework.Vector2.Zero, projType, 0, 0, Main.myPlayer, 0f, stableCenterX, horseType);
                if (idx >= 0 && idx < Main.maxProjectiles)
                {
                    Main.projectile[idx].netUpdate = true;
                }
            }
            // Trigger horse movement every 10 seconds
            if (foundHorse != null)
            {
                double currentTime = Main.timeForVisualEffects / 60.0; // seconds
                if (currentTime - lastHorseMoveTime > 10.0)
                {
                    foundHorse.TriggerMove();
                    lastHorseMoveTime = currentTime;
                }
            }
        }

        // Add this method to drop mount items and remove mount projectiles when the stable is broken
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            // Find the top-left of the stable
            int topLeftX = i - frameX + 4; // Origin is (4,5)
            int topLeftY = j - frameY + 2; // Center vertically (6 tiles high)
            float stableCenterX = (topLeftX + 4.5f) * 16f - 64f;
            float stableCenterY = (topLeftY + 3f) * 16f + 12f;
            var modPlayer = Main.LocalPlayer.GetModPlayer<DrawbridgePlayer>();
            var stableKey = new Point16(topLeftX, topLeftY);
            if (modPlayer.stableSlotsPerStable.TryGetValue(stableKey, out var slotSet))
            {
                // Drop all mount items in the stable
                if (slotSet.BlessedApple != null && !slotSet.BlessedApple.IsAir)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16 * 9, 16 * 6, slotSet.BlessedApple.type, slotSet.BlessedApple.stack);
                }
                if (slotSet.BlackSaddle != null && !slotSet.BlackSaddle.IsAir)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16 * 9, 16 * 6, slotSet.BlackSaddle.type, slotSet.BlackSaddle.stack);
                }
                if (slotSet.DustySaddle != null && !slotSet.DustySaddle.IsAir)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16 * 9, 16 * 6, slotSet.DustySaddle.type, slotSet.DustySaddle.stack);
                }
                if (slotSet.RoyalSaddle != null && !slotSet.RoyalSaddle.IsAir)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16 * 9, 16 * 6, slotSet.RoyalSaddle.type, slotSet.RoyalSaddle.stack);
                }
                // Clear the slots
                slotSet.BlessedApple.TurnToAir();
                slotSet.BlackSaddle.TurnToAir();
                slotSet.DustySaddle.TurnToAir();
                slotSet.RoyalSaddle.TurnToAir();
                modPlayer.stableSlotsPerStable.Remove(stableKey);
            }
            // Remove any associated horse projectile
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<StableUnicornMount>())
                {
                    if (System.Math.Abs(proj.ai[1] - stableCenterX) < 16f && System.Math.Abs(proj.Center.Y - stableCenterY) < 32f)
                    {
                        proj.Kill();
                    }
                }
            }
        }
    }
}