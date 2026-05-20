using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Drawbridge.Tiles;
using ReLogic.Utilities;
using System.Collections.Generic;
using System.IO;
using System;

namespace Drawbridge.Projectiles
{
    public class PortcullisAnimation : ModProjectile
    {
        public override string Texture => "Drawbridge/Tiles/PortcullisTile";
        private const int ORIGIN_SHIFT = 14;
        // Updated coordinate decoding to avoid precision loss at high Y values
        private int OriginX => ((int)Projectile.ai[0]) >> 1; // Extract from ai[0] bits 1+
        private int OriginY => (int)Projectile.ai[1]; // Stored directly in ai[1]
        private bool Raising => ((int)Projectile.ai[0] & 1) != 0;
        private ref float Timer => ref Projectile.ai[2];

        // Public access for world-save persistence
        public float AnimationTimer
        {
            get => Timer;
            set
            {
                Timer = value;
                UpdateSolidTiles();
            }
        }

        private const int DURATION = 300;

        private List<Point> _solidTiles = new List<Point>();
        private int _solidTileType;

        private SlotId _soundId;
        private bool _init = false;
        private const float SOUND_VOLUME = 0.8f;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 128;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = DURATION + 2;
            Projectile.alpha = 0;
            _solidTileType = ModContent.TileType<DrawbridgeSolidTile>();
        }

        public override void AI()
        {
            if (!_init)
            {
                // Start the sound
                _soundId = SoundEngine.PlaySound(new SoundStyle("Drawbridge/Sounds/PortcullisMove")
                {
                    Volume = SOUND_VOLUME,
                    IsLooped = true,
                    SoundLimitBehavior = SoundLimitBehavior.IgnoreNew
                }, Projectile.Center);

                Timer = 0;

                _init = true;
            }

            Projectile.timeLeft = 2;

            UpdateSolidTiles();

            Timer += ModContent.GetInstance<DrawbridgeConfig>().PortcullisSpeed;

            if (Main.netMode == NetmodeID.Server)
                Projectile.netUpdate = true;

            if (Timer >= DURATION)
            {
                ClearSolidTiles();
                if (Raising)
                    PlaceUp();
                else
                    PlaceDown();
                Projectile.Kill();
            }
        }

        private void PlaceUp()
        {
            int tileType = ModContent.TileType<PortcullisUpTile>();
            int placeX = OriginX;
            int placeY = OriginY - 7;
            if (WorldGen.PlaceTile(placeX, placeY, tileType, mute: false, forced: false))
            {
                // Set the frame to be the bottom-most part of the portcullis texture
                Main.tile[placeX, placeY].TileFrameY = 7 * 16;
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendTileSquare(-1, placeX, placeY, 1);
            }
        }

        private void PlaceDown()
        {
            int tileType = ModContent.TileType<PortcullisDownTile>();
            WorldGen.PlaceTile(OriginX, OriginY, tileType, mute: false, forced: true);
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendTileSquare(-1, OriginX, OriginY - 7, 1, 8);
        }

        public override void OnKill(int timeLeft)
        {
            if (_init && SoundEngine.TryGetActiveSound(_soundId, out var activeSound))
            {
                activeSound.Stop();
            }

            ClearSolidTiles();
        }

        private void ClearSolidTiles()
        {
            foreach (Point p in _solidTiles)
            {
                Tile tile = Main.tile[p.X, p.Y];
                if (tile != null && tile.HasTile && tile.TileType == _solidTileType)
                {
                    tile.HasTile = false;
                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendTileSquare(-1, p.X, p.Y, 1);
                    }
                }
            }

            _solidTiles.Clear();
        }

        private void UpdateSolidTiles()
        {
            ClearSolidTiles();

            float t = MathHelper.Clamp(Timer / (float)DURATION, 0f, 1f);
            const float totalShift = 7 * 16f; // in pixels

            int topY, bottomY;

            if (Raising)
            {
                float e = 1f - (1f - t) * (1f - t); // EaseOut
                float shift = e * totalShift;

                int topWorldY = (OriginY - 7) * 16;
                int bottomWorldY = (int)(OriginY * 16 + 16 - shift);

                topY = topWorldY / 16;
                bottomY = (bottomWorldY - 1) / 16;
            }
            else // Lowering
            {
                float e = t * t; // EaseIn
                float shift = e * totalShift;

                int topWorldY = (OriginY - 7) * 16;
                int bottomWorldY = (int)((OriginY * 16 + 16 - totalShift) + shift);

                topY = topWorldY / 16;
                bottomY = (bottomWorldY - 1) / 16;

                Rectangle portcullisRect = new Rectangle(OriginX * 16, topY * 16, 16, (bottomY - topY + 1) * 16);
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player.active && !player.dead && player.getRect().Intersects(portcullisRect))
                    {
                        player.Hurt(PlayerDeathReason.ByOther(2), 100, 0);
                    }
                }
                // Damage hostile NPCs as well
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.noGravity && npc.getRect().Intersects(portcullisRect))
                    {
                        NPC.HitInfo hit = new NPC.HitInfo();
                        hit.Damage = 50;
                        npc.StrikeNPC(hit);
                    }
                }
            }

            for (int y = topY; y <= bottomY; y++)
            {
                Point tilePos = new Point(OriginX, y);
                if (!WorldGen.InWorld(tilePos.X, tilePos.Y, 1))
                {
                    continue;
                }

                Tile tile = Main.tile[tilePos.X, tilePos.Y];
                if (tile != null && (!tile.HasTile || !Main.tileSolid[tile.TileType]))
                {
                    WorldGen.PlaceTile(tilePos.X, tilePos.Y, _solidTileType, true, true);
                    _solidTiles.Add(tilePos);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Color baseLight = Lighting.GetColor(OriginX, OriginY);
            SpriteBatch sb = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            float t = MathHelper.Clamp(Timer / DURATION, 0f, 1f);

            Rectangle sourceRect;
            Vector2 origin;
            Vector2 drawPos;
            Vector2 hinge = new Vector2(OriginX * 16f + 8f, OriginY * 16f + 16f);
            const float totalShift = 7 * 16f;

            if (Raising)
            {
                float e = 1f - (1f - t) * (1f - t); // EaseOut
                float shift = e * totalShift;

                int cropPx = (int)shift;
                int visibleHeight = tex.Height - cropPx;

                sourceRect = new Rectangle(0, cropPx, tex.Width, visibleHeight);
                origin = new Vector2(tex.Width * 0.5f, visibleHeight);
                drawPos = hinge - new Vector2(0, shift) - Main.screenPosition;
            }
            else // Lowering
            {
                float e = t * t; // EaseIn
                float shift = e * totalShift;

                int visibleHeight = Math.Min(16 + (int)shift, tex.Height);
                int sourceY = tex.Height - visibleHeight;

                sourceRect = new Rectangle(0, sourceY, tex.Width, visibleHeight);
                origin = new Vector2(tex.Width * 0.5f, visibleHeight);
                drawPos = hinge - new Vector2(0, totalShift - shift) - Main.screenPosition;
            }

            sb.Draw(tex, drawPos, sourceRect, baseLight, 0f, origin, 1f, SpriteEffects.None, 0f);
            return false;
        }
    }
} 