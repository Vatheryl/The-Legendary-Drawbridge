using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Drawbridge.Items;
using Drawbridge.Systems;
using Drawbridge.Items.HorseArmor;

namespace Drawbridge.Tiles
{
    // 96x80 px (6 tiles wide, 5 tiles tall) mannequin that can hold IronFieldBarding overlay
    public class EquineArmatureTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;
            Main.tileWaterDeath[Type] = false;
            Main.tileLighted[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 6; // 6 * 16 = 96 px
            TileObjectData.newTile.Height = 5; // 5 * 16 = 80 px
            TileObjectData.newTile.Origin = new Point16(3, 4); // center bottom
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(160, 120, 80), CreateMapEntryName());
            DustType = DustID.WoodFurniture;
            HitSound = SoundID.Dig;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            // Drop the item
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16 * 6, 16 * 5, ModContent.ItemType<EquineArmatureItem>());

            // If this tile stored armor, drop it
            GetTopLeft(i, j, out int left, out int top);
            Point16 key = new(left, top);

            if (EquineArmatureWorld.TryGetType(key, out int overlayType) && overlayType != 0)
            {
                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16 * 6, 16 * 5, overlayType);
                EquineArmatureWorld.SetType(key, 0);
            }

            // Clear flip state and sync so clients clean up state
            EquineArmatureWorld.SetAndSyncFlipped(key, false);
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            GetTopLeft(i, j, out int left, out int top);
            Point16 key = new(left, top);
            int heldItemType = player.HeldItem.type; 

            // Toggle/equip overlay when holding an overlay item
            if (player.HeldItem.ModItem is HorseArmorItem)
            {
                int currentType = EquineArmatureWorld.GetType(key);

                if (currentType == player.HeldItem.type)
                {
                    // Remove current overlay - only handle item dropping on server/single player
                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        Item.NewItem(new EntitySource_TileInteraction(player, left, top), left * 16, top * 16, 16 * 6, 16 * 5, player.HeldItem.type);
                    }

                    EquineArmatureWorld.SetAndSyncType(key, 0);
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
                }
                else
                {
                    // Equip or replace overlay
                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        // Drop previous if any
                        if (currentType != 0)
                        {
                            Item.NewItem(new EntitySource_TileInteraction(player, left, top), left * 16, top * 16, 16 * 6, 16 * 5, currentType);
                        }

                        // Consume the item used
                        player.ConsumeItem(player.HeldItem.type);
                    }
                    else if (Main.netMode == NetmodeID.MultiplayerClient)
                    {
                        // In multiplayer, client consumes item immediately; server handles drops via packet
                        player.ConsumeItem(player.HeldItem.type);
                    }

                    EquineArmatureWorld.SetAndSyncType(key, heldItemType);
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
                }

                return true;
            }

            // Empty hand right-click also toggles: remove if present
            if (player.HeldItem == null || player.HeldItem.IsAir)
            {
                int currentType = EquineArmatureWorld.GetType(key);

                if (currentType != 0)
                {
                    // Only drop items in single player; server handles drops via packet in multiplayer
                    if (Main.netMode == NetmodeID.SinglePlayer)
                    {
                        Item.NewItem(new EntitySource_TileInteraction(player, left, top), left * 16, top * 16, 16 * 6, 16 * 5, currentType);
                    }

                    EquineArmatureWorld.SetAndSyncType(key, 0);
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
                    return true;
                }
            }

            return false;
        }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            // Persist placement orientation: flipped when player is facing left
            GetTopLeft(i, j, out int left, out int top);
            Point16 key = new Point16(left, top);
            bool flipped = Main.LocalPlayer.direction == -1;

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                EquineArmatureWorld.SetFlipped(key, flipped);
            }
            else if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Send desired flip to server; server will broadcast
                EquineArmatureWorld.SetAndSyncFlipped(key, flipped);
            }
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // Draw the mannequin sprite with correct facing and the same 12-tile offset as the overlay
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameX != 0 || tile.TileFrameY != 0)
                return; // only draw once from the top-left

            GetTopLeft(i, j, out int left, out int top);
            Point16 key = new Point16(left, top);
            bool flipped = EquineArmatureWorld.GetFlipped(key);

            Texture2D baseTex = ModContent.Request<Texture2D>("Drawbridge/Tiles/EquineArmatureTile").Value;
            Vector2 drawPos = new Vector2(left * 16 + 12 * 16, top * 16 + 12 * 16) - Main.screenPosition;
            Color light = Lighting.GetColor(left, top);
            SpriteEffects fx = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(baseTex, drawPos, null, light, 0f, Vector2.Zero, 1f, fx, 0f);
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // Suppress the default per-tile drawing; we draw the full sprite once in PostDraw
            return false;
        }

        // Prevent default drop to avoid duplicate items; we manually drop in KillMultiTile
        public override bool CanDrop(int i, int j) => false;

        private static void GetTopLeft(int i, int j, out int left, out int top)
        {
            Tile tile = Main.tile[i, j];
            int frameX = tile.TileFrameX / 16; // 0..5
            int frameY = tile.TileFrameY / 16; // 0..4
            left = i - frameX;
            top = j - frameY;
        }
    }
}


