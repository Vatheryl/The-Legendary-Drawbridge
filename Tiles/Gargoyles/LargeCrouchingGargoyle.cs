using Drawbridge.Common.Tiles;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace Drawbridge.Tiles.Gargoyles;

internal class LargeCrouchingGargoyle : ModTile, IAutoloadTileItem
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
        TileObjectData.newTile.Width = 5;
        TileObjectData.newTile.Height = 5;
        TileObjectData.newTile.Origin = new(3, 4);
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16];
        TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.PlatformNonHammered, TileObjectData.newTile.Width, 0);

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
        TileObjectData.addAlternate(1);
        TileObjectData.addTile(Type);

        DustType = DustID.Stone;
        AddMapEntry(new Color(90, 90, 90), Language.GetText("Mods.Drawbridge.Tiles.CrouchingGargoyle.MapEntry"));
        RegisterItemDrop(this.ItemType());
    }
}
