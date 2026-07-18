using Drawbridge.Common.Tiles;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace Drawbridge.Tiles.Tables;

internal abstract class TableBase : ModTile, IAutoloadTileItem
{
    internal class TableBaseSetup : ModSystem
    {
        public override void PostSetupContent()
        {
            var platforms = ModContent.GetContent<TablePlatform>();
            int[] types = [.. platforms.Select(x => x.Type)];
            int[] bases = [.. Bases];

            foreach (int @base in Bases)
            {
                int[] alts = TileObjectData.GetTileData(@base, 0).AnchorAlternateTiles ?? [];
                TileObjectData.GetTileData(@base, 0).AnchorAlternateTiles = [.. alts, .. bases, .. types];
            }
        }
    }

    internal static List<int> Bases = [];

    public virtual void StaticItemDefaults(ModItem item) { }

    public void SetItemDefaults(ModItem item)
    {
        item.Item.rare = ItemRarityID.Blue;
        item.Item.value = Item.buyPrice(0, 0, 0, 5);
    }

    public virtual void AddItemRecipes(ModItem item) => item.CreateRecipe().AddIngredient(ItemID.Wood, 3).AddTile(TileID.Sawmill).Register();

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileSolidTop[Type] = true;
        Main.tileTable[Type] = true;

        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateHeights = [18];
        const AnchorType Anchors = AnchorType.SolidWithTop | AnchorType.Platform | AnchorType.PlanterBox | AnchorType.Table | AnchorType.SolidTile | AnchorType.AlternateTile;
        TileObjectData.newTile.AnchorBottom = new AnchorData(Anchors, 1, 0);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(108, 71, 43), CreateMapEntryName());
        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);

        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;
        Bases.Add(Type);
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        Tile tile = Main.tile[i, j - 1];

        if (tile.HasTile && ModContent.GetModTile(tile.TileType) is TablePlatform)
            WorldGen.TileFrame(i, j - 1);
    }
}
