using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Drawbridge.Tiles
{
    public abstract class BaseSignTile : ModTile
    {
        protected abstract int DropItemType { get; }

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileSolidTop[Type] = false;
            Main.tileSign[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;

            // Hanging sign - 52x52 sprite with padding, similar to vanilla hanging signs
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Origin = new Point16(1, 0); // top-center for hanging
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(191, 142, 111), CreateMapEntryName());
            DustType = DustID.WoodFurniture;
            HitSound = SoundID.Dig;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            // Get the top-left position of the sign
            int signX = i - (frameX / 18 % 3);
            int signY = j - (frameY / 18 % 3);
            
            // Kill the sign data
            Sign.KillSign(signX, signY);
            
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 48, 48, DropItemType);
        }

        public override bool RightClick(int i, int j)
        {
            Player player = Main.LocalPlayer;
            
            // Get the top-left position of the sign
            Tile tile = Main.tile[i, j];
            int signX = i - (tile.TileFrameX / 18 % 3);
            int signY = j - (tile.TileFrameY / 18 % 3);
            
            if (Main.editSign)
            {
                SoundEngine.PlaySound(SoundID.MenuClose);
                player.sign = -1;
                Main.editSign = false;
                Main.npcChatText = "";
            }
            else
            {
                int signIndex = Sign.ReadSign(signX, signY);
                if (signIndex >= 0)
                {
                    SoundEngine.PlaySound(SoundID.MenuOpen);
                    player.sign = signIndex;
                    Main.editSign = true;
                    Main.npcChatText = Main.sign[signIndex].text;
                }
            }
            return true;
        }

        public override bool CanDrop(int i, int j) => false;
    }
}
