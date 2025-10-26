using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using ReLogic.Content;
using System;

namespace Drawbridge.Tiles
{
    public class ClockTile : ModTile
    {
        public override string Texture => "Drawbridge/Tiles/ClockTile";

        private static int? lastDrawnHour = null;

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileSolidTop[Type] = false;
            TileID.Sets.DisableSmartCursor[Type] = true;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 5;
            TileObjectData.newTile.Height = 5;
            TileObjectData.newTile.Origin = new Point16(2, 4); // bottom-center for 5x5
            TileObjectData.newTile.AnchorWall = true;
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.AnchorTop = AnchorData.Empty;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(200, 200, 200), Mod.GetLocalization("Mods.Drawbridge.Tiles.ClockTile"));
            DustType = DustID.Stone;
            HitSound = SoundID.Tink;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            // Kill the clock hand projectile at this clock's center
            float centerX = (i - (frameX / 16 % 5)) * 16f + 40f; // 5x5 tile, center is +40px
            float centerY = (j - (frameY / 16 % 5)) * 16f + 40f;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.type == ModContent.ProjectileType<Projectiles.ClockHand>() && Vector2.DistanceSquared(proj.Center, new Vector2(centerX, centerY)) < 16 * 16)
                {
                    proj.Kill();
                }
            }
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 80, 80, ModContent.ItemType<Items.ClockItem>());
        }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Center of the 5x5 tile
                float centerX = (i - 2) * 16f + 40f;
                float centerY = (j - 4) * 16f + 40f;
				// Spawn clock hands with a proper source and owner
				var source = new EntitySource_Misc("ClockTile.PlaceInWorld");
				Projectile.NewProjectile(source, new Vector2(centerX, centerY), Vector2.Zero, ModContent.ProjectileType<Projectiles.ClockHand>(), 0, 0, Main.myPlayer, centerX, centerY);
            }
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.5f;
            g = 0.5f;
            b = 0.5f;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            if (tile == null || tile.TileFrameX != 0 || tile.TileFrameY != 0)
                return;

            // Draw the static clock face (5x5, 80x80px)
            Texture2D clockTexture = ModContent.Request<Texture2D>("Drawbridge/Tiles/ClockTile", AssetRequestMode.ImmediateLoad).Value;
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 position = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;
            spriteBatch.Draw(clockTexture, position, null, Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

            // --- Hourly hand movement sound ---
            // Only play for top-left tile
            if (Main.netMode != NetmodeID.Server && tile.TileFrameX == 0 && tile.TileFrameY == 0)
            {
                // Check config for mute
                var config = Terraria.ModLoader.ModContent.GetInstance<DrawbridgeConfig>();
                if (!config.MuteClockSound)
                {
                    double analogHour;
                    if (Main.dayTime) {
                        analogHour = 4.5 + (Main.time / 54000.0) * 15.0;
                    } else {
                        analogHour = 19.5 + (Main.time / 32400.0) * 9.0;
                        if (analogHour >= 24.0) analogHour -= 24.0;
                    }
                    int hourInt = (int)Math.Floor(analogHour + 3.0) % 12;
                    if (!lastDrawnHour.HasValue || lastDrawnHour.Value != hourInt)
                    {
                        lastDrawnHour = hourInt;
                        var sound = new Terraria.Audio.SoundStyle("Drawbridge/Sounds/HandsMovement");
                        Terraria.Audio.SoundEngine.PlaySound(sound, new Vector2(i * 16, j * 16));
                    }
                }
            }
        }

        public override bool CanDrop(int i, int j) => false;
    }
} 