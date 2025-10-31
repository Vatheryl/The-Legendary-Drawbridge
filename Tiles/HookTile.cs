using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.GameContent;

namespace Drawbridge.Tiles
{
    public class HookTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = false;
            Main.tileSolid[Type] = false; // Players can pass through
            Main.tileSolidTop[Type] = false;
            TileID.Sets.DisableSmartCursor[Type] = true;

            // Set up 1x2 tile object data
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.Origin = new Point16(0, 0); // Top-left
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 }; // Two 16px high segments
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            // Place a tile entity when the hook is placed
            TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<HookTileEntity>().Generic_HookPostPlaceMyPlayer;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(139, 137, 137), CreateMapEntryName());
            DustType = DustID.Iron;
            HitSound = SoundID.Tink;
        }

        public override bool CanPlace(int i, int j)
        {
            // Check if there's a solid tile or chain above to hang from
            Tile aboveTile = Main.tile[i, j - 1];
            if (!aboveTile.HasTile || (!Main.tileSolid[aboveTile.TileType] && aboveTile.TileType != TileID.Chain))
                return false;

            // Check if the 1x2 area is clear
            for (int dy = 0; dy < 2; dy++)
            {
                Tile tile = Main.tile[i, j + dy];
                if (tile.HasTile)
                    return false;
            }

            return true;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            // Ensure TE drops any stored fish and gets removed
            ModContent.GetInstance<HookTileEntity>().Kill(i, j);
            // Drop the hook item when destroyed
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 32,
                ModContent.ItemType<Items.HookItem>());
        }

        public override bool CanDrop(int i, int j) => false;

        public override bool RightClick(int i, int j)
        {
            if (!TileEntity.TryGet(i, j, out HookTileEntity hookTE))
                return false;

            Player player = Main.LocalPlayer;

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                // Ask server to handle interaction
                PacketHandler.SendContextPacket(MessageType.HookFishInteract, i, j);
                return true;
            }

            if (hookTE.StoredItemType == 0)
            {
                Item held = player.HeldItem;
                if (held != null && !held.IsAir && IsFishItemType(held.type))
                {
                    hookTE.StoredItemType = held.type;
                    held.stack--;
                    if (held.stack <= 0)
                        held.TurnToAir();

                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.TileEntitySharing, number: hookTE.ID, number2: hookTE.Position.X, number3: hookTE.Position.Y);
                }
            }
            else
            {
                player.QuickSpawnItem(player.GetSource_TileInteraction(i, j), hookTE.StoredItemType, 1);
                hookTE.StoredItemType = 0;
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.TileEntitySharing, number: hookTE.ID, number2: hookTE.Position.X, number3: hookTE.Position.Y);
            }

            return true;
        }

        // PostDraw removed - now handled by HookProjectile for proper drawing order
        
        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            // Don't draw the tile - let the HookProjectile handle all drawing
            return false;
        }

        public static bool IsFishItemType(int type)
        {
            return type == ItemID.Bass
                || type == ItemID.Trout
                || type == ItemID.Salmon
                || type == ItemID.NeonTetra
                || type == ItemID.Tuna
                || type == ItemID.RedSnapper
                || type == ItemID.Damselfish
                || type == ItemID.GoldenCarp
                || type == ItemID.SpecularFish
                || type == ItemID.Prismite
                || type == ItemID.ArmoredCavefish
                || type == ItemID.ChaosFish
                || type == ItemID.Obsidifish
                || type == ItemID.FrostMinnow
                || type == ItemID.FlarefinKoi
                || type == ItemID.Ebonkoi
                || type == ItemID.Hemopiranha
                || type == ItemID.Stinkfish
                || type == ItemID.CrimsonTigerfish
                || type == ItemID.DoubleCod
                || type == ItemID.VariegatedLardfish
                || type == ItemID.Shrimp
                || type == ItemID.RockLobster
                || type == ItemID.PrincessFish
                || type == ItemID.Honeyfin
                || type == ItemID.AtlanticCod;
        }
    }
}
