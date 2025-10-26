using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Drawbridge.Tiles;
using ReLogic.Utilities;
using Terraria.GameContent;
using System.IO;
using System;

namespace Drawbridge.Projectiles
{
    public class DrawbridgeAnimation : ModProjectile
    {
        public override string Texture => "Drawbridge/Tiles/DrawbridgeTile";
        private const int ORIGIN_SHIFT = 14;

        // Decode hinge tile coordinates - now stored separately to avoid precision loss
        private int OriginX => ((int)Projectile.ai[0]) >> 2; // Extract from ai[0] bits 2+
        private int OriginY => (int)Projectile.ai[1]; // Stored directly in ai[1]
        private int ai0Int => (int)Projectile.ai[0];
        private bool Raising => (ai0Int & 1) != 0;
        private bool Flipped => (ai0Int & 2) != 0;
        private int animationTimer = 0;
        private const int ANIMATION_DURATION = 300; // ~5 seconds

        private List<Point> _solidTiles = new List<Point>();
        private int _solidTileType;
        private Vector2 p_tl, p_tr, p_bl, p_br; // World positions of corners
        private SlotId _soundId;
        private bool _soundPlaying = false;
        private bool _lastRight = false; // track previous right mouse state
        private const float SOUND_VOLUME = 0.8f;

        public override void SetDefaults()
        {
            // Use world tile extents (32x240) for collision & sizing
            Projectile.width = 32;
            Projectile.height = 240;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = ANIMATION_DURATION + 5;
            Projectile.alpha = 0; // fully visible
            _solidTileType = ModContent.TileType<DrawbridgeSolidTile>();
        }

        public override void AI()
        {
            // Prevent the projectile from expiring while the animation is running.
            Projectile.timeLeft = 2;
            
            // handle click to reverse animation
            bool right = Main.mouseRight;
            if (animationTimer > 0 && right && !_lastRight)
            {
                CalculateCornersForTime(animationTimer, out Vector2 c1, out Vector2 c2, out Vector2 c3, out Vector2 c4);
                if (IsPointInQuad(Main.MouseWorld, c1, c2, c4, c3))
                {
                    bool currentRaising = Raising;
                    float clickRaw = animationTimer / (float)ANIMATION_DURATION;
                    float f0 = currentRaising ? EaseOut(clickRaw) : EaseIn(1f - clickRaw);
                    bool newRaising = !currentRaising;
                    // Preserve OriginX coordinate when updating flags during reversal
                    Projectile.ai[0] = (OriginX << 2) | (Flipped ? 2 : 0) | (newRaising ? 1 : 0);
                    float reversedRaw = newRaising
                        ? 1f - (float)System.Math.Sqrt(1f - f0)
                        : 1f - (float)System.Math.Sqrt(f0);
                    animationTimer = (int)(reversedRaw * ANIMATION_DURATION);
                    // adjust audio for reversal: restart the movement sound
                    if (SoundEngine.TryGetActiveSound(_soundId, out var activeSound))
                    {
                        activeSound.Stop();
                    }
                    _soundId = SoundEngine.PlaySound(new SoundStyle("Drawbridge/Sounds/DrawbridgeMove")
                    {
                        Volume = SOUND_VOLUME,
                        Pitch = 0f,
                        MaxInstances = 0,
                        IsLooped = true
                    }, Projectile.Center);
                }
            }
            _lastRight = right;

            // Ensure movement sound is playing and at correct volume, looped
            var movementStyle = new SoundStyle("Drawbridge/Sounds/DrawbridgeMove")
            {
                Volume = SOUND_VOLUME,
                Pitch = 0f,
                MaxInstances = 0,
                IsLooped = true
            };
            if (!SoundEngine.TryGetActiveSound(_soundId, out var movementSound))
            {
                _soundId = SoundEngine.PlaySound(movementStyle, Projectile.Center);
                SoundEngine.TryGetActiveSound(_soundId, out movementSound);
            }

            if (animationTimer >= ANIMATION_DURATION)
            {
                // At end of swing, clear any remaining temp tiles.
                ClearSolidTiles();

                // On the server, place the final static tile. Clients will receive this via net message.
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Swap the tile in the world to its new orientation
                    if (Raising)
                        SwitchToHorizontal();
                    else
                        SwitchToVertical();
                }
                
                Projectile.Kill();
                return; // Stop further AI processing
            }

            // Check if the path for the next frame is blocked.
            if (IsPathObstructed())
            {
                // If blocked, mute the sound and do not advance the animation.
                if (movementSound != null)
                {
                    movementSound.Volume = 0f;
                }
                return;
            }

            // Unblocked, ensure volume
            if (movementSound != null)
            {
                movementSound.Volume = SOUND_VOLUME;
            }

            animationTimer++;
            if (Main.netMode == NetmodeID.Server)
                Projectile.netUpdate = true;

            float raw = (float)animationTimer / ANIMATION_DURATION;
            raw = MathHelper.Clamp(raw, 0f, 1f);
            float eased = Raising ? EaseOut(raw) : EaseIn(1 - raw);
            Projectile.rotation = -MathHelper.PiOver2 * eased;

            UpdateCorners();
            UpdateSolidTiles();
            DealContactDamage();
        }

        private float EaseOut(float t) => 1f - (1f - t) * (1f - t);
        private float EaseIn(float t) => t * t;

        public override bool PreDraw(ref Color lightColor)
        {
            Color baseLight = Lighting.GetColor(OriginX, OriginY);
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            // Hinge shifts up 2 tiles over the swing
            float raw = (float)animationTimer / ANIMATION_DURATION;
            raw = MathHelper.Clamp(raw, 0f, 1f);
            float shiftEased = Raising ? EaseOut(raw) : EaseIn(1 - raw);
            float hingeX = OriginX * 16f + (Flipped ? 0f : 32f);
            Vector2 hinge = new Vector2(hingeX, OriginY * 16f + 16f - shiftEased * 32f);
            // Full-sprite scale so 32x270 -> 32x240
            float scale = (float)Projectile.height / tex.Height;
            SpriteEffects spriteEffects = Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            float rot = Projectile.rotation * (Flipped ? -1f : 1f);
            Vector2 origin = new Vector2(Flipped ? 0f : tex.Width, tex.Height);
            Vector2 drawPos = hinge - Main.screenPosition;
            Main.spriteBatch.Draw(tex, drawPos, null, baseLight, rot, origin, scale, spriteEffects, 0);

            // Draw Hell cover if this is a Hell drawbridge animation
            if (Projectile.type == ModContent.ProjectileType<HellDrawbridgeAnimation>())
            {
                string coverTexturePath = GetHellCoverTexture();
                if (!string.IsNullOrEmpty(coverTexturePath))
                {
                    Texture2D coverTex = ModContent.Request<Texture2D>(coverTexturePath).Value;
                    Vector2 coverOrigin = GetHellCoverOrigin(coverTex);
                    // Don't flip the cover texture - it's already oriented correctly
                    Main.spriteBatch.Draw(coverTex, drawPos, null, baseLight, rot, coverOrigin, scale, SpriteEffects.None, 0);
                    
                    // Draw glow mask overlay
                    string glowMaskTexturePath = GetHellGlowMaskTexture();
                    if (!string.IsNullOrEmpty(glowMaskTexturePath))
                    {
                        // End current batch and start additive blending for glow
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(
                            SpriteSortMode.Deferred,
                            BlendState.Additive,
                            Main.DefaultSamplerState,
                            DepthStencilState.None,
                            RasterizerState.CullCounterClockwise,
                            null,
                            Main.GameViewMatrix.ZoomMatrix
                        );
                        
                        Texture2D glowMaskTex = ModContent.Request<Texture2D>(glowMaskTexturePath).Value;
                        // Use same origin, position, rotation, and scale as the cover with pulsing effect
                        float glowPulse = 0.6f + 0.4f * (float)Math.Sin(Main.GameUpdateCount * 0.03f);
                        Main.spriteBatch.Draw(glowMaskTex, drawPos, null, Color.White * glowPulse, rot, coverOrigin, scale, SpriteEffects.None, 0);
                        
                        // End additive batch and restart normal blending
                        Main.spriteBatch.End();
                        Main.spriteBatch.Begin(
                            SpriteSortMode.Deferred,
                            BlendState.AlphaBlend,
                            Main.DefaultSamplerState,
                            DepthStencilState.None,
                            RasterizerState.CullCounterClockwise,
                            null,
                            Main.GameViewMatrix.ZoomMatrix
                        );
                    }
                }
            }

            // Draw Lion cover if this is a Lion drawbridge animation
            if (Projectile.type == ModContent.ProjectileType<LionDrawbridgeAnimation>())
            {
                string coverTexturePath = GetLionCoverTexture();
                if (!string.IsNullOrEmpty(coverTexturePath))
                {
                    Texture2D coverTex = ModContent.Request<Texture2D>(coverTexturePath).Value;
                    Vector2 coverOrigin = GetLionCoverOrigin(coverTex);
                    Main.spriteBatch.Draw(coverTex, drawPos, null, baseLight, rot, coverOrigin, scale, SpriteEffects.None, 0);
                }
            }

            // Draw chain connecting drawbridge to adjacent support tile
            Texture2D chainTexture = TextureAssets.Chain.Value;
            Vector2 chainOrigin = new Vector2(chainTexture.Width * 0.5f, chainTexture.Height * 0.5f);
            int xIndex = 1; // second tile column (0-based)
            int segmentY = 2; // 13th tile from bottom
            // Compute attach offset relative to hinge
            Vector2 baseLocal = new Vector2(-Projectile.width + (xIndex * 16 + 8), -Projectile.height + (segmentY * 16 + 8));
            float chainRotFactor = Projectile.rotation * (Flipped ? -1f : 1f);
            Vector2 offset = new Vector2(Flipped ? -baseLocal.X : baseLocal.X, baseLocal.Y);
            Vector2 attachPoint = hinge + offset.RotatedBy(chainRotFactor);
            // Compute static support tile position
            int supportIndex = Flipped ? OriginX - 1 : OriginX + 2;
            Vector2 supportPoint = new Vector2(supportIndex * 16f + 8f, (OriginY - 14 + segmentY) * 16f + 8f);
            Vector2 start = attachPoint - Main.screenPosition;
            Vector2 end = supportPoint - Main.screenPosition;
            Vector2 dir = end - start;
            float length = dir.Length();
            if (length > 0f)
            {
                dir /= length; // normalize
                float chainRotation = dir.ToRotation() + MathHelper.PiOver2;
                float chainStep = chainTexture.Height * 0.9f;
                for (float t = chainStep; t < length - chainStep; t += chainStep)
                {
                    Vector2 pos = start + dir * t;
                    // Apply color tints for different drawbridge types
                    Color chainColor;
                    if (Projectile.type == ModContent.ProjectileType<HellDrawbridgeAnimation>())
                    {
                        chainColor = Color.Lerp(baseLight, new Color(20, 40, 120), 0.8f); // Dark blue tint
                    }
                    else if (Projectile.type == ModContent.ProjectileType<LionDrawbridgeAnimation>())
                    {
                        chainColor = Color.Lerp(baseLight, new Color(255, 215, 0), 0.3f); // Golden tint
                    }
                    else
                    {
                        chainColor = baseLight; // Default lighting
                    }
                    Main.spriteBatch.Draw(chainTexture, pos, null, chainColor, chainRotation, chainOrigin, 1f, spriteEffects, 0f);
                }
            }
            return false;
        }

        public override void PostDraw(Color lightColor)
        {
            Color baseLight = Lighting.GetColor(OriginX, OriginY);
            
            // Use appropriate wheels for different drawbridge types - always use vertical wheel during animation
            string wheelTexturePath;
            if (Projectile.type == ModContent.ProjectileType<HellDrawbridgeAnimation>())
            {
                wheelTexturePath = "Drawbridge/Tiles/Hell_Wheel";
            }
            else if (Projectile.type == ModContent.ProjectileType<LionDrawbridgeAnimation>())
            {
                wheelTexturePath = "Drawbridge/Tiles/Lion_Wheel";
            }
            else
            {
                wheelTexturePath = "Drawbridge/Tiles/WheelTile";
            }
            
            Texture2D wheelTex = ModContent.Request<Texture2D>(wheelTexturePath).Value;
            float hingeX = OriginX * 16f + (Flipped ? 0f : 32f);
            Vector2 hinge = new Vector2(hingeX, OriginY * 16f + 16f);
            Vector2 wheelPivot = new Vector2(hinge.X - 16f, hinge.Y) - Main.screenPosition;
            // Draw wheel with flipped hinge if needed
            float wheelRot = Projectile.rotation * (Flipped ? -1f : 1f);
            SpriteEffects wheelEffect = Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Vector2 wheelOrigin = new Vector2(wheelTex.Width * 0.5f, wheelTex.Height * 0.5f);
            if (Flipped)
                wheelPivot.X += 32f; // shift wheel right 2 tiles when flipped
            Main.spriteBatch.Draw(wheelTex, wheelPivot, null, baseLight * 0.9f, wheelRot, wheelOrigin, 1f, wheelEffect, 0f);
            
            // Draw hell wheel glow mask for hell drawbridge animations
            if (Projectile.type == ModContent.ProjectileType<HellDrawbridgeAnimation>())
            {
                // End current batch and start additive blending for glow
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.Additive,
                    Main.DefaultSamplerState,
                    DepthStencilState.None,
                    RasterizerState.CullCounterClockwise,
                    null,
                    Main.GameViewMatrix.ZoomMatrix
                );
                
                // Always use vertical hell wheel glow mask during animation
                Texture2D hellWheelGlowMask = ModContent.Request<Texture2D>("Drawbridge/Tiles/Hell_Wheel-Glow_mask").Value;
                float glowPulse = 0.6f + 0.4f * (float)Math.Sin(Main.GameUpdateCount * 0.03f);
                Main.spriteBatch.Draw(hellWheelGlowMask, wheelPivot, null, Color.White * glowPulse, wheelRot, wheelOrigin, 1f, wheelEffect, 0f);
                
                // End additive batch and restart normal blending
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(
                    SpriteSortMode.Deferred,
                    BlendState.AlphaBlend,
                    Main.DefaultSamplerState,
                    DepthStencilState.None,
                    RasterizerState.CullCounterClockwise,
                    null,
                    Main.GameViewMatrix.ZoomMatrix
                );
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (SoundEngine.TryGetActiveSound(_soundId, out var sound))
            {
                sound.Stop();
            }
            ClearSolidTiles();
        }

        private void CalculateCornersForTime(int time, out Vector2 corner1, out Vector2 corner2, out Vector2 corner3, out Vector2 corner4)
        {
            float raw = (float)time / ANIMATION_DURATION;
            raw = MathHelper.Clamp(raw, 0f, 1f);
            float eased = Raising ? EaseOut(raw) : EaseIn(1 - raw);
            float rotation = -MathHelper.PiOver2 * eased * (Flipped ? -1f : 1f);
            float shiftEased = Raising ? EaseOut(raw) : EaseIn(1 - raw);
            float hingeX = OriginX * 16f + (Flipped ? 0f : 32f);
            Vector2 hinge = new Vector2(hingeX, OriginY * 16f + 16f - shiftEased * 32f);

            // Define the four corners of the drawbridge rectangle relative to the hinge
            Vector2 v_tl, v_tr, v_bl, v_br;
            if (!Flipped)
            {
                v_tl = new Vector2(-Projectile.width, -Projectile.height);
                v_tr = new Vector2(0, -Projectile.height);
                v_bl = new Vector2(-Projectile.width, 0);
                v_br = Vector2.Zero; // Hinge is bottom-right

                // User reported solid tiles are 1 tile too far left.
                // Shift the quad to the right by 8 pixels (1/2 tile)
                Vector2 shift = new Vector2(8, 0);
                v_tl += shift;
                v_tr += shift;
                v_bl += shift;
                v_br += shift;
            }
            else
            {
                v_tl = new Vector2(0, -Projectile.height);
                v_tr = new Vector2(Projectile.width, -Projectile.height);
                v_bl = Vector2.Zero; // Hinge is bottom-left
                v_br = new Vector2(Projectile.width, 0);
                // No shift for flipped case
            }

            // Get world positions of corners
            corner1 = hinge + v_tl.RotatedBy(rotation);
            corner2 = hinge + v_tr.RotatedBy(rotation);
            corner3 = hinge + v_br.RotatedBy(rotation);
            corner4 = hinge + v_bl.RotatedBy(rotation);
        }

        private bool IsPathObstructed()
        {
            // Calculate the corners for the next animation frame.
            CalculateCornersForTime(animationTimer + 1, out Vector2 next_p_tl, out Vector2 next_p_tr, out Vector2 next_p_bl, out Vector2 next_p_br);

            Rectangle nextBoundingBox = GetBoundingBoxForQuad(next_p_tl, next_p_tr, next_p_br, next_p_bl);

            for (int i = nextBoundingBox.X / 16; i <= nextBoundingBox.Right / 16; i++)
            {
                for (int j = nextBoundingBox.Y / 16; j <= nextBoundingBox.Bottom / 16; j++)
                {
                    if (WorldGen.InWorld(i, j))
                    {
                        Vector2 tileCenter = new Vector2(i * 16f + 8f, j * 16f + 8f);
                        if (IsPointInQuad(tileCenter, next_p_tl, next_p_tr, next_p_br, next_p_bl))
                        {
                            Tile tile = Main.tile[i, j];
                            if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType] && tile.TileType != _solidTileType)
                            {
                                return true; // Found an obstruction.
                            }
                        }
                    }
                }
            }
            return false; // No obstructions found.
        }

        private void UpdateCorners()
        {
            CalculateCornersForTime(animationTimer, out p_tl, out p_tr, out p_bl, out p_br);
        }

        private void DealContactDamage()
        {
            // Calculate midpoints to define the outer half of the bridge, which deals damage
            Vector2 p_tm = (p_tl + p_tr) / 2f;
            Vector2 p_bm = (p_bl + p_br) / 2f;

            // Define the vertices of the damaging part of the quad (the outer half)
            Vector2 quad_p1, quad_p2, quad_p3, quad_p4;

            if (!Flipped)
            {
                // For a normal drawbridge, the hinge is on the right, so the outer part is the left half.
                quad_p1 = p_tl;
                quad_p2 = p_tm;
                quad_p3 = p_bm;
                quad_p4 = p_bl;
            }
            else
            {
                // For a flipped drawbridge, the hinge is on the left, so the outer part is the right half.
                quad_p1 = p_tm;
                quad_p2 = p_tr;
                quad_p3 = p_br;
                quad_p4 = p_bm;
            }


            Rectangle damageBoundingBox = GetBoundingBoxForQuad(quad_p1, quad_p2, quad_p3, quad_p4);

            // Get the world positions of the tiles directly under the bridge
            List<Point> tilesUnderBridge = new List<Point>();
            
            // Calculate the positions of tiles directly under the bridge based on current rotation
            float angle = Projectile.rotation;
            int bridgeLength = 15; // Length of the bridge in tiles
            
            // Starting point for calculating tiles under the bridge
            int startX = OriginX;
            int startY = OriginY;
            
            if (!Flipped)
            {
                // For normal bridge, start from the hinge and go left
                startX -= bridgeLength;
            }
            
            // Add all tiles directly under the bridge to our list
            for (int i = 0; i < bridgeLength; i++)
            {
                int tileX = startX + (Flipped ? i : bridgeLength - i);
                int tileY = startY + 1; // One tile below the bridge base
                
                if (WorldGen.InWorld(tileX, tileY))
                {
                    tilesUnderBridge.Add(new Point(tileX, tileY));
                }
            }

            // Check players
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && !player.dead && !player.ghost)
                {
                    if (player.getRect().Intersects(damageBoundingBox) && IsRectInQuad(player.getRect(), quad_p1, quad_p2, quad_p3, quad_p4))
                    {
                        // Define the player's head area (top third of their hitbox)
                        Rectangle playerRect = player.getRect();
                        Rectangle playerHeadRect = new Rectangle(
                            playerRect.X,
                            playerRect.Y,
                            playerRect.Width,
                            playerRect.Height / 3
                        );
                        
                        // Check if the bridge is touching the player's head
                        bool bridgeTouchingHead = false;
                        if (IsRectInQuad(playerHeadRect, quad_p1, quad_p2, quad_p3, quad_p4))
                        {
                            bridgeTouchingHead = true;
                        }
                        
                        // If the bridge isn't touching the player's head, skip damage check
                        if (!bridgeTouchingHead)
                        {
                            continue;
                        }
                        
                        // Check if the player is being crushed against tiles directly under the bridge
                        bool crushed = false;
                        Point playerTileMin = (playerRect.TopLeft() / 16f).ToPoint();
                        Point playerTileMax = (playerRect.BottomRight() / 16f).ToPoint();

                        for (int x = playerTileMin.X; x <= playerTileMax.X; x++)
                        {
                            for (int y = playerTileMin.Y; y <= playerTileMax.Y; y++)
                            {
                                if (WorldGen.InWorld(x, y))
                                {
                                    Tile tile = Main.tile[x, y];
                                    if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType] && tile.TileType != _solidTileType)
                                    {
                                        // Check if this tile is in our list of tiles under the bridge
                                        if (tilesUnderBridge.Contains(new Point(x, y)))
                                        {
                                            crushed = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (crushed) break;
                        }

                        if (crushed)
                        {
                            player.Hurt(PlayerDeathReason.ByProjectile(i, Projectile.whoAmI), 50, 0);
                        }
                    }
                }
            }

            // Check NPCs
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.noGravity)
                {
                    if (npc.getRect().Intersects(damageBoundingBox) && IsRectInQuad(npc.getRect(), quad_p1, quad_p2, quad_p3, quad_p4))
                    {
                        // Define the NPC's head area (top third of their hitbox)
                        Rectangle npcRect = npc.getRect();
                        Rectangle npcHeadRect = new Rectangle(
                            npcRect.X,
                            npcRect.Y,
                            npcRect.Width,
                            npcRect.Height / 3
                        );
                        
                        // Check if the bridge is touching the NPC's head
                        bool bridgeTouchingHead = false;
                        if (IsRectInQuad(npcHeadRect, quad_p1, quad_p2, quad_p3, quad_p4))
                        {
                            bridgeTouchingHead = true;
                        }
                        
                        // If the bridge isn't touching the NPC's head, skip damage check
                        if (!bridgeTouchingHead)
                        {
                            continue;
                        }
                        
                        // Check if the NPC is being crushed against tiles directly under the bridge
                        bool crushed = false;
                        Point npcTileMin = (npcRect.TopLeft() / 16f).ToPoint();
                        Point npcTileMax = (npcRect.BottomRight() / 16f).ToPoint();

                        for (int x = npcTileMin.X; x <= npcTileMax.X; x++)
                        {
                            for (int y = npcTileMin.Y; y <= npcTileMax.Y; y++)
                            {
                                if (WorldGen.InWorld(x, y))
                                {
                                    Tile tile = Main.tile[x, y];
                                    if (tile != null && tile.HasTile && Main.tileSolid[tile.TileType] && tile.TileType != _solidTileType)
                                    {
                                        // Check if this tile is in our list of tiles under the bridge
                                        if (tilesUnderBridge.Contains(new Point(x, y)))
                                        {
                                            crushed = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (crushed) break;
                        }

                        if (crushed)
                        {
                            NPC.HitInfo hit = new NPC.HitInfo();
                            hit.Damage = 50;
                            npc.StrikeNPC(hit);
                        }
                    }
                }
            }
        }

        private Rectangle GetBoundingBoxForQuad(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            int minX = (int)System.Math.Min(System.Math.Min(p1.X, p2.X), System.Math.Min(p3.X, p4.X));
            int maxX = (int)System.Math.Max(System.Math.Max(p1.X, p2.X), System.Math.Max(p3.X, p4.X));
            int minY = (int)System.Math.Min(System.Math.Min(p1.Y, p2.Y), System.Math.Min(p3.Y, p4.Y));
            int maxY = (int)System.Math.Max(System.Math.Max(p1.Y, p2.Y), System.Math.Max(p3.Y, p4.Y));
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        private bool IsRectInQuad(Rectangle rect, Vector2 q1, Vector2 q2, Vector2 q3, Vector2 q4)
        {
            // This is a simplified check. It checks if any corner of the rect is inside the quad,
            // or if any corner of the quad is inside the rect. It doesn't handle all complex intersection cases
            // but is a good and performant approximation for this use case.
            if (IsPointInQuad(rect.TopLeft(), q1, q2, q3, q4)) return true;
            if (IsPointInQuad(rect.TopRight(), q1, q2, q3, q4)) return true;
            if (IsPointInQuad(rect.BottomLeft(), q1, q2, q3, q4)) return true;
            if (IsPointInQuad(rect.BottomRight(), q1, q2, q3, q4)) return true;
            if (rect.Contains(q1.ToPoint())) return true;
            if (rect.Contains(q2.ToPoint())) return true;
            if (rect.Contains(q3.ToPoint())) return true;
            if (rect.Contains(q4.ToPoint())) return true;
            return false;
        }

        private void ClearSolidTiles()
        {
            foreach (Point p in _solidTiles)
            {
                if (WorldGen.InWorld(p.X, p.Y))
                {
                    Tile tile = Main.tile[p.X, p.Y];
                    if (tile.HasTile && tile.TileType == _solidTileType)
                    {
                        tile.HasTile = false;
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendTileSquare(-1, p.X, p.Y, 1, 1);
                    }
                }
            }
            _solidTiles.Clear();
        }

        private void UpdateSolidTiles()
        {
            ClearSolidTiles();

            Rectangle bridgeBoundingBox = GetBoundingBoxForQuad(p_tl, p_tr, p_br, p_bl);

            // Iterate through the bounding box and place tiles inside the polygon
            for (int i = bridgeBoundingBox.X / 16; i <= bridgeBoundingBox.Right / 16; i++)
            {
                for (int j = bridgeBoundingBox.Y / 16; j <= bridgeBoundingBox.Bottom / 16; j++)
                {
                    Vector2 tileCenter = new Vector2(i * 16f + 8f, j * 16f + 8f);
                    if (IsPointInQuad(tileCenter, p_tl, p_tr, p_br, p_bl))
                    {
                        if (WorldGen.InWorld(i, j) && !Main.tile[i, j].HasTile)
                        {
                            Tile tile = Main.tile[i, j];
                            tile.HasTile = true;
                            tile.TileType = (ushort)_solidTileType;
                            _solidTiles.Add(new Point(i, j));
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.SendTileSquare(-1, i, j, 1, 1);
                        }
                    }
                }
            }
        }

        private bool IsPointInQuad(Vector2 p, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            return IsPointInTriangle(p, a, b, c) || IsPointInTriangle(p, a, c, d);
        }

        private bool IsPointInTriangle(Vector2 pt, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            float d1 = Sign(pt, v1, v2);
            float d2 = Sign(pt, v2, v3);
            float d3 = Sign(pt, v3, v1);
            bool has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            return !(has_neg && has_pos);
        }

        private float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        private void ClearArea(int startX, int startY, int width, int height)
        {
            for (int i = startX; i < startX + width; i++)
            {
                for (int j = startY; j < startY + height; j++)
                {
                    if (WorldGen.InWorld(i, j))
                    {
                        Tile tile = Main.tile[i, j];
                        if (tile != null && tile.HasTile && tile.TileType != _solidTileType)
                        {
                            WorldGen.KillTile(i, j, false, false, false);
                            if (Main.netMode == NetmodeID.Server)
                            {
                                NetMessage.SendTileSquare(-1, i, j, 1);
                            }
                        }
                    }
                }
            }
        }

        private void SwitchToHorizontal()
        {
            // A horizontal bridge is either normal or flipped.
            int tileType;
            if (Projectile.type == ModContent.ProjectileType<HellDrawbridgeAnimation>())
            {
                tileType = Flipped
                    ? ModContent.TileType<HellDrawbridgeHorizontalFlippedTile>()
                    : ModContent.TileType<HellDrawbridgeHorizontalTile>();
            }
            else if (Projectile.type == ModContent.ProjectileType<LionDrawbridgeAnimation>())
            {
                tileType = Flipped
                    ? ModContent.TileType<LionDrawbridgeHorizontalFlippedTile>()
                    : ModContent.TileType<LionDrawbridgeHorizontalTile>();
            }
            else
            {
                tileType = Flipped
                    ? ModContent.TileType<DrawbridgeHorizontalFlippedTile>()
                    : ModContent.TileType<DrawbridgeHorizontalTile>();
            }
            int originX = OriginX;
            int originY = OriginY;

            // The hinge is at the bottom of the vertical drawbridge. When it becomes
            // horizontal, this hinge position needs to be mapped to the new multitile's origin.
            // Horizontal normal origin: (14,1) bottom-right
            // Horizontal flipped origin: (0,1) bottom-left
            int placeX, placeY;
            int areaX, areaY;
            if (Flipped)
            {
                // Flipped vertical (hinge bottom-left) to flipped horizontal (hinge bottom-left)
                // Hinge at (originX, originY)
                placeX = originX;
                placeY = originY;
                areaX = placeX;
                areaY = placeY - 1;
            }
            else
            {
                // Normal vertical (hinge bottom-right) to normal horizontal (hinge bottom-right)
                // Hinge at (originX+1, originY)
                // The horizontal tile is 15 wide, so its anchor needs to be placed correctly.
                placeX = originX + 1;
                placeY = originY;
                areaX = placeX - 14;
                areaY = placeY - 1;
            }

            ClearArea(areaX, areaY, 15, 2);
            bool placed = WorldGen.PlaceObject(placeX, placeY, tileType, mute: true, style: 0, alternate: 0, random: -1, direction: Flipped ? 1 : -1);

            if (!placed)
            {
                int itemType = Projectile.type == ModContent.ProjectileType<HellDrawbridgeAnimation>()
                    ? ModContent.ItemType<Items.HellDrawbridgeItem>()
                    : ModContent.ItemType<Items.DrawbridgeItem>();
                Item.NewItem(Projectile.GetSource_FromThis(), new Vector2(placeX * 16, placeY * 16), itemType);
                return;
            }

            if (Main.netMode == NetmodeID.Server)
            {
                int netX = Flipped ? placeX : placeX - 14;
                int netY = placeY - 1;
                NetMessage.SendTileSquare(-1, netX, netY, 15, 2);
            }
        }

        private void SwitchToVertical()
        {
            int tileType;
            if (Projectile.type == ModContent.ProjectileType<HellDrawbridgeAnimation>())
            {
                tileType = Flipped
                    ? ModContent.TileType<HellDrawbridgeVerticalFlippedTile>()
                    : ModContent.TileType<HellDrawbridgeTile>();
            }
            else if (Projectile.type == ModContent.ProjectileType<LionDrawbridgeAnimation>())
            {
                tileType = Flipped
                    ? ModContent.TileType<LionDrawbridgeVerticalFlippedTile>()
                    : ModContent.TileType<LionDrawbridgeTile>();
            }
            else
            {
                tileType = Flipped
                    ? ModContent.TileType<DrawbridgeVerticalFlippedTile>()
                    : ModContent.TileType<DrawbridgeTile>();
            }
            int originX = OriginX;
            int originY = OriginY;

            // We must subtract the tile origin offset from the hinge coordinates
            // to get the correct tile placement origin
            int placeX = originX + (Flipped ? 0 : 1);
            int placeY = originY;

            // Fix: Only clear the correct area for flipped case
            if (Flipped)
            {
                ClearArea(placeX, placeY - 14, 2, 15);
            }
            else
            {
                ClearArea(placeX - 1, placeY - 14, 2, 15);
            }
            bool placed = WorldGen.PlaceObject(placeX, placeY, tileType, mute: true, style: 0, alternate: 0, random: -1, direction: Flipped ? 1 : -1);
            if (!placed)
            {
                // If placement fails, drop the item for the player.
                int itemType = Projectile.type == ModContent.ProjectileType<HellDrawbridgeAnimation>()
                    ? ModContent.ItemType<Items.HellDrawbridgeItem>()
                    : ModContent.ItemType<Items.DrawbridgeItem>();
                Item.NewItem(Projectile.GetSource_FromThis(), new Vector2(placeX * 16, placeY * 16), itemType);
                return;
            }

            if (Main.netMode == NetmodeID.Server)
            {
                // For multiplayer, we need to send the tile changes to clients.
                // The placement area is 2 wide by 15 high.
                // placeX is the anchor, so we need to adjust for Flipped
                int netX = Flipped ? placeX : placeX - 1;
                int netY = placeY - 14;
                NetMessage.SendTileSquare(-1, netX, netY, 2, 15);
            }
        }

        // Sync animationTimer and _lastRight so clients reconnecting see the current state
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(animationTimer);
            writer.Write(_lastRight);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            animationTimer = reader.ReadInt32();
            _lastRight = reader.ReadBoolean();
        }

        // Public access for world-save persistence
        public int AnimationTimer
        {
            get => animationTimer;
            set
            {
                animationTimer = value;
                UpdateCorners();
                UpdateSolidTiles();
            }
        }

        private string GetHellCoverTexture()
        {
            // Always use vertical covers during animation - only use horizontal when fully horizontal and stationary
            return Flipped ? "Drawbridge/Tiles/Hell_DrawbridgeFlipped_cover" : "Drawbridge/Tiles/Hell_Drawbridge_cover";
        }

        private string GetHellGlowMaskTexture()
        {
            // Always use vertical glow masks during animation - only use horizontal when fully horizontal and stationary
            return Flipped ? "Drawbridge/Tiles/Hell_Drawbridge-Glow_MaskFlipped" : "Drawbridge/Tiles/Hell_Drawbridge-Glow_Mask";
        }

        private Vector2 GetHellCoverOrigin(Texture2D coverTex)
        {
            // Adjust offsets for animation: non-flipped 6px left, flipped 6px right
            if (Flipped)
            {
                return new Vector2(coverTex.Width - 46f - 4f, coverTex.Height); // Move 6px right by subtracting 6
            }
            else
            {
                return new Vector2(30f + 16f + 4f, coverTex.Height); // Move 6px left by adding 6 to origin
            }
        }

        private string GetLionCoverTexture()
        {
            return Flipped ? "Drawbridge/Tiles/Lion_DrawbridgeFlipped_cover" : "Drawbridge/Tiles/Lion_Drawbridge_cover";
        }

        private Vector2 GetLionCoverOrigin(Texture2D coverTex)
        {
            float halfWidth = coverTex.Width / 2f;
            if (Flipped)
            {
                return new Vector2(halfWidth - 16f, coverTex.Height); // Adjust for left hinge
            }
            else
            {
                return new Vector2(16f + halfWidth, coverTex.Height); // Adjust for right hinge
            }
        }
    }
}