using Drawbridge.Common.Tiles;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ObjectData;

namespace Drawbridge.Tiles.Tables;

internal abstract class TableBase : ModTile, IAutoloadTileItem
{
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
        Main.tileSolidTop[Type] = false;

        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateHeights = [18];
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(108, 71, 43), CreateMapEntryName());

        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;
        Bases.Add(Type);
    }
}
