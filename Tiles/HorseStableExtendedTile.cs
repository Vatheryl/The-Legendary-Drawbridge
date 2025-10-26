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
using System.Collections.Generic;
using Drawbridge;

namespace Drawbridge.Tiles
{
    public class HorseStableExtendedTile : ModTile
    {
        private static Point16? lastRightClickedStable = null;
        private static double lastHorseMoveTime = 0;
        private static double lastStableNoiseTime = 0;
        private static double nextStableNoiseDelay = 0;
        private static readonly string[] StableNoiseFiles = new string[] {
            "Drawbridge/Sounds/StableNoises1",
            "Drawbridge/Sounds/StableNoises2",
            "Drawbridge/Sounds/StableNoises3",
            "Drawbridge/Sounds/StableNoises4",
            "Drawbridge/Sounds/StableNoises5",
            "Drawbridge/Sounds/StableNoises6"
        };

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
            TileObjectData.newTile.Width = 27;
            TileObjectData.newTile.Height = 6;
            TileObjectData.newTile.Origin = new Point16(13, 5); // Center bottom for 27 tiles
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16 };
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
                Tile tile = Main.tile[i, j];
                int frameX = tile.TileFrameX / 16;
                int frameY = tile.TileFrameY / 16;
                int topLeftX = i - frameX + 13; // Origin is (13,5)
                int topLeftY = j - frameY + 2;
                Point16 thisStable = new Point16(topLeftX, topLeftY);
                if (StableSlotUISystem.lastRightClickedStable.HasValue && StableSlotUISystem.lastRightClickedStable.Value == thisStable)
                {
                    StableSlotUISystem.lastRightClickedStable = null;
                    Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuClose);
                }
                else
                {
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

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Tile tile = Main.tile[i, j];
            int frameX = tile.TileFrameX / 16;
            int frameY = tile.TileFrameY / 16;
            int topLeftX = i - frameX + 13; // Origin is (13,5)
            int topLeftY = j - frameY + 2;
            float stableCenterX = (topLeftX + 13.5f) * 16f - 208f;
            float stableCenterY = (topLeftY + 3f) * 16f + 12f;
            float roamingWidth = 408f; // Extended stable roaming area
            bool randomRoam = true; // Enable random movement for extended stable

            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<DrawbridgePlayer>();
            // Get or create the slot set for this stable
            if (!modPlayer.stableSlotsPerStable.TryGetValue(new Point16(topLeftX, topLeftY), out var slotSet))
            {
                slotSet = new DrawbridgePlayer.StableSlots();
                modPlayer.stableSlotsPerStable[new Point16(topLeftX, topLeftY)] = slotSet;
            }

            // List of (horseType, slotItem) pairs
            var slotTypes = new List<(int, Item)> {
                (0, slotSet.BlessedApple),
                (3, slotSet.BlackSaddle),
                (1, slotSet.DustySaddle),
                (2, slotSet.RoyalSaddle)
            };
            float leftEdge = stableCenterX - (roamingWidth / 2f) + 32f;
            float rightEdge = stableCenterX + (roamingWidth / 2f) - 32f;
            float y = stableCenterY;
            foreach (var (horseType, slotItem) in slotTypes)
            {
                if (slotItem != null && !slotItem.IsAir)
                {
                    // Check if a mount of this type already exists in this stable
                    bool found = false;
                    foreach (Projectile proj in Main.projectile)
                    {
                        if (proj.active && proj.type == ModContent.ProjectileType<StableUnicornMount>())
                        {
                            // Use a wider X check to allow for random placement
                            if ((int)proj.ai[2] == horseType &&
                                proj.ai[0] == roamingWidth &&
                                System.Math.Abs(proj.ai[1] - stableCenterX) < 1f &&
                                System.Math.Abs(proj.Center.Y - y) < 32f)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                    {
                        int projType = ModContent.ProjectileType<StableUnicornMount>();
                        var source = Main.LocalPlayer.GetSource_FromThis();
                        float randX = Main.rand.NextFloat(leftEdge, rightEdge);
                        float ai2 = (float)horseType + (randomRoam ? 100f : 0f);
                        int idx = Projectile.NewProjectile(source, randX, y, 0f, 0f, projType, 0, 0f, Main.myPlayer, roamingWidth, stableCenterX, ai2);
                        if (idx >= 0 && idx < Main.maxProjectiles)
                        {
                            Main.projectile[idx].netUpdate = true;
                        }
                    }
                }
            }
            // No need to trigger movement here; each mount will move randomly on its own

            // --- Stable noise logic ---
            double currentTime = Main.timeForVisualEffects / 60.0;
            // Only play noise if at least one slot is filled
            bool hasMount = false;
            foreach (var (_, slotItem) in slotTypes)
            {
                if (slotItem != null && !slotItem.IsAir)
                {
                    hasMount = true;
                    break;
                }
            }
            if (!hasMount)
                return;
            if (Terraria.ModLoader.ModContent.GetInstance<DrawbridgeConfig>().MuteStableNoises)
                return;
            if (nextStableNoiseDelay == 0 || currentTime - lastStableNoiseTime > nextStableNoiseDelay)
            {
                // Always play for testing
                int fileIdx = Main.rand.Next(StableNoiseFiles.Length);
                string soundPath = StableNoiseFiles[fileIdx];
                Terraria.Audio.SoundEngine.PlaySound(new Terraria.Audio.SoundStyle(soundPath)
                {
                    IsLooped = false,
                    Volume = 0.2f, // Quieter
                    Pitch = 0f,
                    PitchVariance = 0f
                }, new Vector2(stableCenterX, stableCenterY));
                lastStableNoiseTime = currentTime;
                nextStableNoiseDelay = Main.rand.NextFloat(30f, 60f);
            }
        }

        // Add this method to drop mount items and remove mount projectiles when the extended stable is broken
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            // Find the top-left of the stable
            int topLeftX = i - frameX + 13; // Origin is (13,5)
            int topLeftY = j - frameY + 2;
            float stableCenterX = (topLeftX + 13.5f) * 16f - 208f;
            float stableCenterY = (topLeftY + 3f) * 16f + 12f;
            float roamingWidth = 408f;
            var modPlayer = Main.LocalPlayer.GetModPlayer<DrawbridgePlayer>();
            var stableKey = new Point16(topLeftX, topLeftY);
            if (modPlayer.stableSlotsPerStable.TryGetValue(stableKey, out var slotSet))
            {
                // Drop all mount items in the stable
                if (slotSet.BlessedApple != null && !slotSet.BlessedApple.IsAir)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16 * 27, 16 * 6, slotSet.BlessedApple.type, slotSet.BlessedApple.stack);
                }
                if (slotSet.BlackSaddle != null && !slotSet.BlackSaddle.IsAir)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16 * 27, 16 * 6, slotSet.BlackSaddle.type, slotSet.BlackSaddle.stack);
                }
                if (slotSet.DustySaddle != null && !slotSet.DustySaddle.IsAir)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16 * 27, 16 * 6, slotSet.DustySaddle.type, slotSet.DustySaddle.stack);
                }
                if (slotSet.RoyalSaddle != null && !slotSet.RoyalSaddle.IsAir)
                {
                    Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16 * 27, 16 * 6, slotSet.RoyalSaddle.type, slotSet.RoyalSaddle.stack);
                }
                // Clear the slots
                slotSet.BlessedApple.TurnToAir();
                slotSet.BlackSaddle.TurnToAir();
                slotSet.DustySaddle.TurnToAir();
                slotSet.RoyalSaddle.TurnToAir();
                modPlayer.stableSlotsPerStable.Remove(stableKey);
            }
            // Remove any associated horse projectiles
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<StableUnicornMount>())
                {
                    // Match by stable center X, Y, and roaming width
                    if (System.Math.Abs(proj.ai[1] - stableCenterX) < 1f && System.Math.Abs(proj.Center.Y - stableCenterY) < 32f && proj.ai[0] == roamingWidth)
                    {
                        proj.Kill();
                    }
                }
            }
        }
    }
} 