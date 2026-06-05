using Drawbridge.Common.Tiles;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Drawbridge.Tiles.Gargoyles;

internal class TwistingGargoyle : ModTile, IAutoloadTileItem
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Height = 3;
        TileObjectData.newTile.Origin = new(0, 2);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 18];
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.StyleHorizontal = true;

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);

        DustType = DustID.Stone;
        AddMapEntry(new Color(90, 90, 90), Language.GetText("Mods.Drawbridge.Tiles.CrouchingGargoyle.MapEntry"));
        RegisterItemDrop(this.ItemType());
    }
}
