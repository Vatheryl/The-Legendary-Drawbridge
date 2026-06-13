using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.ID;
using Drawbridge.Players;
using System;
using Terraria.DataStructures;
using System.Collections.Generic;
using Drawbridge.Tiles;
using Drawbridge.Projectiles;

namespace Drawbridge.Systems
{
    public class StableSlotUISystem : ModSystem
    {
        // Track the last right-clicked stable for the local player
        public static Point16? lastRightClickedStable = null;

        private static readonly (string icon, int itemID, Func<DrawbridgePlayer.StableSlots, Item> getSlot, Action<DrawbridgePlayer.StableSlots, Item> setSlot)[] StableSlots = new (string, int, Func<DrawbridgePlayer.StableSlots, Item>, Action<DrawbridgePlayer.StableSlots, Item>)[]
        {
            ("Drawbridge/Textures/HorseStableIcons/BlessedApple_Slot", 3260, (DrawbridgePlayer.StableSlots s) => s.BlessedApple, (DrawbridgePlayer.StableSlots s, Item v) => s.BlessedApple = v),
            ("Drawbridge/Textures/HorseStableIcons/BlackSaddle_Slot", 4787, (DrawbridgePlayer.StableSlots s) => s.BlackSaddle, (DrawbridgePlayer.StableSlots s, Item v) => s.BlackSaddle = v),
            ("Drawbridge/Textures/HorseStableIcons/DustySaddle_Slot", 4785, (DrawbridgePlayer.StableSlots s) => s.DustySaddle, (DrawbridgePlayer.StableSlots s, Item v) => s.DustySaddle = v),
            ("Drawbridge/Textures/HorseStableIcons/RoyalSaddle_Slot", 4786, (DrawbridgePlayer.StableSlots s) => s.RoyalSaddle, (DrawbridgePlayer.StableSlots s, Item v) => s.RoyalSaddle = v),
        };

        // Helper: consistent horseType mapping for extended stable
        private static readonly (int horseType, Func<DrawbridgePlayer.StableSlots, Item> getSlot)[] ExtendedStableSlots = new (int, Func<DrawbridgePlayer.StableSlots, Item>)[]
        {
            (0, s => s.BlessedApple), // Unicorn
            (3, s => s.BlackSaddle), // Dark
            (1, s => s.DustySaddle), // Painted
            (2, s => s.RoyalSaddle), // Majestic
        };

        // Remove SyncExtendedStableProjectiles and use per-slot logic below

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1 && lastRightClickedStable.HasValue)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
                    "Drawbridge: Stable Slot UI",
                    delegate {
                        DrawStableSlotUI(Main.spriteBatch);
                        return true;
                    },
                    InterfaceScaleType.Game)
                );
            }
        }

        private void DrawStableSlotUI(SpriteBatch spriteBatch)
        {
            if (!lastRightClickedStable.HasValue)
                return;
            Point16 stableTopLeft = lastRightClickedStable.Value;
            Player player = Main.LocalPlayer;
            Vector2 stableCenter = new Vector2((stableTopLeft.X * 16f) + 72f, (stableTopLeft.Y * 16f) + 48f);
            float maxDist = 200f;
            if (!player.active || player.dead || Vector2.Distance(player.Center, stableCenter) > maxDist)
            {
                lastRightClickedStable = null;
                Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.MenuClose);
                return;
            }
            var modPlayer = player.GetModPlayer<DrawbridgePlayer>();
            // Get or create the slot set for this stable
            if (!modPlayer.stableSlotsPerStable.TryGetValue(stableTopLeft, out var slotSet))
            {
                slotSet = new DrawbridgePlayer.StableSlots();
                modPlayer.stableSlotsPerStable[stableTopLeft] = slotSet;
            }
            float iconSize = 32f;
            float iconScale = iconSize / 52f;
            Vector2 basePos = new Vector2((stableTopLeft.X * 16f) + 24f - 73f, (stableTopLeft.Y * 16f) - 12f) - Main.screenPosition;
            float slotSpacing = 38f;

            // --- Exclusivity logic ---
            bool isExtendedStable = false;
            if (WorldGen.InWorld(stableTopLeft.X, stableTopLeft.Y)) {
                Tile t = Main.tile[stableTopLeft.X, stableTopLeft.Y];
                if (t != null && t.HasTile && t.TileType == ModContent.TileType<HorseStableExtendedTile>())
                    isExtendedStable = true;
            }
            int filledSlotIdx = -1;
            if (!isExtendedStable) {
                for (int idx = 0; idx < StableSlots.Length; idx++)
                {
                    var (_, _, getSlot, _) = StableSlots[idx];
                    Item slotItem = getSlot(slotSet);
                    if (slotItem != null && !slotItem.IsAir)
                    {
                        filledSlotIdx = idx;
                        break;
                    }
                }
            }
            
            for (int slotIdx = 0; slotIdx < StableSlots.Length; slotIdx++)
            {
                var (slotIcon, slotItemID, getSlot, setSlot) = StableSlots[slotIdx];
                Vector2 iconPos = basePos + new Vector2(slotSpacing * slotIdx, 0);
                Texture2D slotTex = ModContent.Request<Texture2D>(slotIcon, ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
                float alpha = isExtendedStable ? 0.95f : ((filledSlotIdx == -1 || filledSlotIdx == slotIdx) ? 0.95f : 0.3f);
                spriteBatch.Draw(slotTex, iconPos, null, Color.White * alpha, 0f, new Vector2(slotTex.Width / 2, slotTex.Height / 2), iconScale, SpriteEffects.None, 0f);
                Item slotItem = getSlot(slotSet);
                if (slotItem != null && !slotItem.IsAir)
                {
                    float itemScale = 0.6f;
                    Vector2 itemPos = iconPos + new Vector2(0, 0);
                    Main.instance.LoadItem(slotItem.type);
                    Texture2D itemTex = Terraria.GameContent.TextureAssets.Item[slotItem.type].Value;
                    spriteBatch.Draw(itemTex, itemPos, null, Color.White, 0f, new Vector2(itemTex.Width / 2, itemTex.Height / 2), itemScale, SpriteEffects.None, 0f);
                }
                Rectangle slotRect = new Rectangle((int)(iconPos.X - iconSize / 2), (int)(iconPos.Y - iconSize / 2), (int)iconSize, (int)iconSize);
                bool allowInteraction = isExtendedStable ? true : (filledSlotIdx == -1 || filledSlotIdx == slotIdx);
                if (allowInteraction && slotRect.Contains(Main.mouseX, Main.mouseY) && Main.mouseItem != null)
                {
                    Main.LocalPlayer.mouseInterface = true;
                    if (Main.mouseLeft && Main.mouseLeftRelease)
                    {
                        // Place item in slot (no exclusivity for extended stable)
                        if (Main.mouseItem.type == slotItemID && (slotItem == null || slotItem.IsAir) && (isExtendedStable || filledSlotIdx == -1))
                        {
                            setSlot(slotSet, Main.mouseItem.Clone());
                            getSlot(slotSet).stack = 1;
                            Main.mouseItem.stack--;
                            if (Main.mouseItem.stack <= 0)
                                Main.mouseItem.TurnToAir();
                            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Grab);
                            if (isExtendedStable)
                            {
                                // Only spawn the horse for this slot if it doesn't already exist
                                float stableCenterX = (stableTopLeft.X + 13.5f) * 16f - 208f;
                                float stableCenterY = (stableTopLeft.Y + 3f) * 16f + 12f;
                                float roamingWidth = 408f;
                                float y = stableCenterY;
                                float leftEdge = stableCenterX - (roamingWidth / 2f) + 32f;
                                float rightEdge = stableCenterX + (roamingWidth / 2f) - 32f;
                                int horseType = ExtendedStableSlots[slotIdx].horseType;
                                bool found = false;
                                foreach (Projectile proj in Main.projectile)
                                {
                                    if (proj.active && proj.type == ModContent.ProjectileType<StableUnicornMount>())
                                    {
                                        if ((int)proj.ai[2] % 100 == horseType && proj.ai[0] == roamingWidth && System.Math.Abs(proj.ai[1] - stableCenterX) < 1f && System.Math.Abs(proj.Center.Y - stableCenterY) < 32f)
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
                                    float ai2 = (float)horseType + 100f; // always randomRoam for extended
                                    int idx = Projectile.NewProjectile(source, randX, y, 0f, 0f, projType, 0, 0f, Main.myPlayer, roamingWidth, stableCenterX, ai2);
                                    if (idx >= 0 && idx < Main.maxProjectiles)
                                    {
                                        Main.projectile[idx].netUpdate = true;
                                    }
                                }
                            }
                        }
                        // Pick up item from slot
                        else if (Main.mouseItem.IsAir && slotItem != null && !slotItem.IsAir)
                        {
                            Main.mouseItem = slotItem.Clone();
                            setSlot(slotSet, new Item());
                            Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Grab);
                            if (isExtendedStable)
                            {
                                // Only remove the horse for this slot
                                float stableCenterX = (stableTopLeft.X + 13.5f) * 16f - 208f;
                                float stableCenterY = (stableTopLeft.Y + 3f) * 16f + 12f;
                                float roamingWidth = 408f;
                                int horseType = ExtendedStableSlots[slotIdx].horseType;
                                foreach (Projectile proj in Main.projectile)
                                {
                                    if (proj.active && proj.type == ModContent.ProjectileType<StableUnicornMount>())
                                    {
                                        if ((int)proj.ai[2] % 100 == horseType && proj.ai[0] == roamingWidth && System.Math.Abs(proj.ai[1] - stableCenterX) < 1f && System.Math.Abs(proj.Center.Y - stableCenterY) < 32f)
                                        {
                                            proj.Kill();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
} 