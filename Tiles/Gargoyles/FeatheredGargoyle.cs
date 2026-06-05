using Drawbridge.Common.Tiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Drawbridge.Tiles.Gargoyles;

internal class FeatheredGargoyleSpout : ModTile, IAutoloadTileItem, IWaterfallTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Width = 3;
        TileObjectData.newTile.Origin = new(0, 2);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
        TileObjectData.newTile.AnchorRight = GetAnchorData();

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.newAlternate.AnchorRight = AnchorData.Empty;
        TileObjectData.newAlternate.AnchorLeft = GetAnchorData();
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);

        DustType = DustID.Stone;
        AddMapEntry(new Color(90, 90, 90), Language.GetText("Mods.Drawbridge.Tiles.CrouchingGargoyle.MapEntry"));
        RegisterItemDrop(this.ItemType());
    }

    public virtual AnchorData GetAnchorData() => new(AnchorType.SolidTile | AnchorType.SolidSide, 2, 0);

    bool IWaterfallTile.HasWaterfall(ref int i, ref int j, Tile tile, ref int waterfallType)
    {
        waterfallType = 10;

        return tile.TileFrameX is 0 or 90 && tile.TileFrameY == 18;
    }
}

internal class GargoyleSpout : FeatheredGargoyleSpout
{
    public override AnchorData GetAnchorData() => new(AnchorType.SolidTile | AnchorType.SolidSide, 1, 1);
}

internal class LungingGargoyleSpout : FeatheredGargoyleSpout;