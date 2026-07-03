using Drawbridge.Common.Tiles;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace Drawbridge.Tiles.Tables;

#nullable enable

internal abstract class TablePlatform : ModTile, IAutoloadTileItem
{
    internal class TablePlatformSetup : ModSystem
    {
        public override void PostSetupContent()
        {
            var platforms = ModContent.GetContent<TablePlatform>();
            int[] types = [.. platforms.Select(x => x.Type)];
            int[] bases = [.. TableBase.Bases];

            foreach (var platform in platforms)
            {
                TileObjectData.GetTileData(platform.Type, 0).AnchorAlternateTiles = bases;

                for (int i = 0; i < 3; ++i)
                    TileObjectData.GetTileData(platform.Type, 0, i + 1).AnchorAlternateTiles = types;
            }
        }
    }

    private static bool BlockRecursion = false;

    public virtual void StaticItemDefaults(ModItem item) => RegisterItemDrop(item.Type);

    public void SetItemDefaults(ModItem item)
    {
        item.Item.rare = ItemRarityID.Blue;
        item.Item.value = Item.buyPrice(0, 0, 0, 15);
    }

    public virtual void AddItemRecipes(ModItem item) => item.CreateRecipe().AddIngredient(ItemID.Wood, 12).AddTile(TileID.Sawmill).Register();

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileSolidTop[Type] = true;

        TileID.Sets.DisableSmartCursor[Type] = true;

        ref TileObjectData tile = ref TileObjectData.newTile;
        tile.CopyFrom(TileObjectData.Style3x2);
        tile.Height = 1;
        tile.CoordinateHeights = [16];
        tile.UsesCustomCanPlace = true;
        tile.Origin = new Point16(1, 0);
        tile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.AlternateTile, 1, 1);

        ref TileObjectData alternate = ref TileObjectData.newAlternate;

        alternate.CopyFrom(tile);
        alternate.AnchorBottom = AnchorData.Empty;
        alternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, 1, 0);
        alternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, 1, 0);
        TileObjectData.addAlternate(2);

        alternate.CopyFrom(tile);
        alternate.AnchorBottom = AnchorData.Empty;
        alternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, 1, 0);
        TileObjectData.addAlternate(1);

        alternate.CopyFrom(tile);
        alternate.AnchorBottom = AnchorData.Empty;
        alternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, 1, 0);
        TileObjectData.addAlternate(4);

        TileObjectData.addTile(Type);

        AddMapEntry(new Color(108, 71, 43), CreateMapEntryName());

        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        Tile tile = Main.tile[i, j];
        int offsetX = tile.TileFrameX % 54 / 18;
        GetInformation(i, j, out _, out _, out _, out TileObjectData mainData);

        if (offsetX == 1)
        {
            if (!BlockRecursion)
            {
                BlockRecursion = true;
                WorldGen.KillTile(i - 1, j, fail, effectOnly, true);
                WorldGen.KillTile(i + 1, j, fail, effectOnly, true);
                BlockRecursion = false;
            }
        }
        else if (offsetX == 0 && Mergeable(i - 1, j, mainData))
        {
            if (!BlockRecursion)
            {
                BlockRecursion = true;
                GetInformation(i - 1, j, out Tile subTile, out int subOffsetX, out bool hasBase, out TileObjectData subData);
                FrameFunctionality(i - 1, j, subTile, subOffsetX, hasBase, subData);
                BlockRecursion = false;
            }
        }
        else if (offsetX == 2 && Mergeable(i + 1, j, mainData))
        {
            if (!BlockRecursion)
            {
                BlockRecursion = true;
                GetInformation(i + 1, j, out Tile subTile, out int subOffsetX, out bool hasBase, out TileObjectData subData);
                FrameFunctionality(i + 1, j, subTile, subOffsetX, hasBase, subData);
                BlockRecursion = false;
            }
        }
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        GetInformation(i, j, out Tile tile, out int offsetX, out bool hasBase, out TileObjectData data);
        return FrameFunctionality(i, j, tile, offsetX, hasBase, data);
    }

    private static bool FrameFunctionality(int i, int j, Tile tile, int offsetX, bool hasBase, TileObjectData data)
    {
        if (ModContent.GetModTile(tile.TileType) is not TablePlatform)
            return true;

        if (offsetX == 1)
        {
            bool left = Mergeable(i - 2, j, data);
            bool right = Mergeable(i + 2, j, data);

            if (left && right)
                tile.TileFrameX = (short)(hasBase ? 180 : 126);
            else if (left && !right)
                tile.TileFrameX = 72;
            else if (!left && right)
                tile.TileFrameX = 234;

            return true;
        }
        else if (offsetX == 0)
        {
            if (Mergeable(i - 1, j, data))
            {
                if (!BlockRecursion)
                {
                    BlockRecursion = true;
                    WorldGen.TileFrame(i - 1, j);
                    BlockRecursion = false;
                }

                tile.TileFrameX = (short)(hasBase ? 162 : 108);
                return false;
            }
            else
            {
                tile.TileFrameX = (short)(hasBase ? 54 : 270);
                return false;
            }
        }
        else if (offsetX == 2)
        {
            if (Mergeable(i + 1, j, data))
            {
                if (!BlockRecursion)
                {
                    BlockRecursion = true;
                    WorldGen.TileFrame(i + 1, j);
                    BlockRecursion = false;
                }

                tile.TileFrameX = (short)(hasBase ? 198 : 144);
                return false;
            }
            else
            {
                tile.TileFrameX = (short)(hasBase ? 252 : 288);
                return false;
            }
        }

        return true;
    }

    private static void GetInformation(int i, int j, out Tile tile, out int offsetX, out bool hasBase, out TileObjectData data)
    {
        tile = Main.tile[i, j];
        offsetX = tile.TileFrameX % 54 / 18;
        Tile @base = Main.tile[i - offsetX + 1, j + 1];
        hasBase = @base.HasTile && (ModContent.GetModTile(@base.TileType) is TableBase || Main.tileSolid[@base.TileType]);
        data = TileObjectData.GetTileData(tile.TileType, 0, 1);
    }

    private static bool Mergeable(int x, int y, TileObjectData? data)
    {
        Tile tile = Main.tile[x, y];

        if (!tile.HasTile)
            return false;

        bool solid = WorldGen.AnchorValid(tile, AnchorType.SolidSide | AnchorType.SolidTile) || TileID.Sets.IsATreeTrunk[tile.TileType];
        bool isAlt = false;

        if (data is not null)
        {
            for (int i = 0; i < data.AnchorAlternateTiles.Length; ++i)
                if (tile.TileType == data.AnchorAlternateTiles[i])
                    isAlt = true;
        }

        return solid || isAlt;
    }
}
