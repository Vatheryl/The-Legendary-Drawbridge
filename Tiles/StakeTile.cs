using Drawbridge.Common;
using Drawbridge.NPCs.Haulox;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Drawbridge.Tiles;

internal class StakeTile : ModTile
{
    public class StakeTileEntity : ModTileEntity
    {
        public (Haulox.Variants variant, int yoke)? Data = null;

        public override void Update()
        {
        }

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<StakeTile>();
        }

        public override void SaveData(TagCompound tag)
        {
            if (Data.HasValue)
            {
                TagCompound data = [];
                data.Add("variant", (byte)Data.Value.variant);
                data.Add("yoke", SavingUtils.IOType<ModItem>(Data.Value.yoke, ItemID.Count));
                tag.Add("data", data);
            }
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.TryGet("data", out TagCompound data))
                Data = ((Haulox.Variants)data.GetByte("variant"), SavingUtils.GetTypeFromIO<ModItem>(data.GetString("yoke")));
        }
    }

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileSolidTop[Type] = false;
        
        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Origin = new Point16(0, 1); // top-center for hanging
        TileObjectData.newTile.CoordinateHeights = [16, 16]; 
        TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<StakeTileEntity>().Generic_HookPostPlaceMyPlayer;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(191, 142, 111), CreateMapEntryName());
        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;
    }

    //public override bool RightClick(int i, int j)
    //{

    //}
}
