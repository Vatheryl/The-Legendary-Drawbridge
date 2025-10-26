using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using Drawbridge.Items;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;

namespace Drawbridge.Tiles
{
    public class SpikedPallisadeFlippedTile : ModTile
    {
        public override string Texture => "Drawbridge/Tiles/SpikedPallisadeFlipped";

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
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 6;
            TileObjectData.newTile.Origin = new Point16(1, 5); // Center bottom
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 18 }; // Last row is 17px for 1px lower
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 3, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(120, 120, 120), CreateMapEntryName());
            DustType = DustID.Stone;
            HitSound = SoundID.Tink;
        }

        public override bool CanPlace(int i, int j)
        {
            Player player = Main.LocalPlayer;
            bool isFlipped = player.direction == 1;
            // Calculate top-left of the multitile
            int topLeftX = i;
            int topLeftY = j - 5;
            // Build solid and damage area info
            string solidAreas = "";
            string damageAreas = "";
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    // Solid: column 1, row 1, or 3x3 area top-left
                    bool isSolid = (x == 0 || y == 0 || (x >= 0 && x <= 2 && y >= 0 && y <= 2));
                    // Damage: matches player/NPC logic
                    bool isDamage = ((x == 2 && y == 0) || (x == 2 && y == 2) || (x == 1 && y == 2) || (x == 0 && y == 5) || (x == 1 && y == 5));
                    if (isSolid) solidAreas += $"[{x+1},{y+1}] ";
                    if (isDamage) damageAreas += $"[{x+1},{y+1}] ";
                }
            }
            // Remove debug message
            if (player.direction == 1)
            {
                WorldGen.PlaceTile(i, j, ModContent.TileType<SpikedPallisadeTile>(), false, true, player.whoAmI);
                return false;
            }
            return base.CanPlace(i, j);
        }

        public override bool RightClick(int i, int j)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                PacketHandler.SendPacket(MessageType.ToggleSpikedPallisade, i, j);
            }
            else
            {
                Toggle(i, j);
            }
            return true;
        }

        public void Toggle(int i, int j)
        {
            // Find the top-left of the multi-tile (origin is (1,5), so top-left is (i - frameX, j - frameY))
            Tile tile = Main.tile[i, j];
            int frameX = tile.TileFrameX / 16;
            int frameY = tile.TileFrameY / 16;
            int topLeftX = i - frameX;
            int topLeftY = j - frameY;

            // First, check if all tiles are actuated
            bool allActuated = true;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    int tx = topLeftX + x;
                    int ty = topLeftY + y;
                    Tile t = Main.tile[tx, ty];
                    if (t != null && t.HasTile && t.TileType == Type)
                    {
                        if (!t.IsActuated)
                        {
                            allActuated = false;
                            break;
                        }
                    }
                }
                if (!allActuated) break;
            }

            // Now toggle all tiles
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    int tx = topLeftX + x;
                    int ty = topLeftY + y;
                    Tile t = Main.tile[tx, ty];
                    if (t != null && t.HasTile && t.TileType == Type)
                    {
                        t.IsActuated = !allActuated;
                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendTileSquare(-1, tx, ty, 1);
                    }
                }
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new Terraria.DataStructures.EntitySource_TileBreak(i, j), i * 16, j * 16, 48, 96,
                ModContent.ItemType<Items.SpikedPallisadeItem>());
        }
    }
} 