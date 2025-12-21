using Drawbridge.Common.Tiles;
using Drawbridge.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Drawbridge.Tiles
{
    public class PortcullisUpTile : ModTile, IToggleTile
    {
        public override string Texture => "Drawbridge/Tiles/PortcullisTile";

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

            // Make it triggerable by wire
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.AnchorTop = AnchorData.Empty;
            TileObjectData.newTile.AnchorLeft = AnchorData.Empty;
            TileObjectData.newTile.AnchorRight = AnchorData.Empty;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(150, 150, 150), CreateMapEntryName());
            DustType = -1;
            HitSound = null;
        }

        public void Toggle(int i, int j)
            {
                int originX = i;
                int originY = j + 7; // bottom of the down-state portcullis
                // remove this one tile
                Tile currentTile = Main.tile[i, j];
                if (currentTile.HasTile && currentTile.TileType == Type)
                {
                    currentTile.HasTile = false;
                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendTileSquare(-1, i, j, 1);
                }
                // spawn lowering animation
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Store originX in ai[0] (using bits 1+), raising flag in bit 0
                float ai0 = (originX << 1) | 0; // lowering (not raising)
                float ai1 = originY; // Store originY directly in ai[1]
                Vector2 pivot = new Vector2(originX * 16f + 8f, originY * 16f + 8f);
                Projectile.NewProjectile(null, pivot, Vector2.Zero,
                    ModContent.ProjectileType<PortcullisAnimation>(), 0, 0, Main.myPlayer, ai0, ai1);
            }
        }

        public override bool RightClick(int i, int j)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                PacketHandler.SendContextPacket(MessageType.ToggleDrawbridge, i, j);
            }
            else
        {
            Toggle(i, j);
            }
            return true;
        }

        public override void HitWire(int i, int j)
        {
            Toggle(i, j);
        }

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (fail || noItem)
            {
                return;
            }
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16,
                ModContent.ItemType<Items.PortcullisItem>());
        }
    }
} 