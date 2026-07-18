using Drawbridge.Common;
using Drawbridge.Common.Tiles;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader.IO;
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

                for (int i = 0; i < 2; ++i)
                    TileObjectData.GetTileData(platform.Type, 0, i + 1).AnchorAlternateTiles = types;
            }
        }
    }

    internal class TableEntity : ModTileEntity
    {
        public int TableclothType = 0;

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return ModContent.GetModTile(tile.TileType) is TablePlatform && tile.HasTile;
        }

        public override void SaveData(TagCompound tag) => tag.Add("cloth", SavingUtils.IOType<ModItem>(TableclothType, ItemID.Count));
        public override void LoadData(TagCompound tag) => TableclothType = SavingUtils.GetTypeFromIO<ModItem>(tag.GetString("cloth"));
        public override void NetSend(BinaryWriter writer) => writer.Write((ushort)TableclothType);
        public override void NetReceive(BinaryReader reader) => TableclothType = reader.ReadUInt16();
    }

    private static bool BlockRecursion = false;
    private static int KillOrderInvalidX = 0;
    private static int KillOrderInvalidXMax = 0;

    public virtual void StaticItemDefaults(ModItem item) => RegisterItemDrop(item.Type);

    public void SetItemDefaults(ModItem item)
    {
        item.Item.rare = ItemRarityID.Blue;
        item.Item.value = Item.buyPrice(0, 0, 0, 15);
    }

    public virtual void AddItemRecipes(ModItem item) => item.CreateRecipe().AddIngredient(ItemID.Wood, 2).AddTile(TileID.Sawmill).Register();

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileSolidTop[Type] = true;
        Main.tileTable[Type] = true;

        TileID.Sets.DisableSmartCursor[Type] = true;

        ref TileObjectData tile = ref TileObjectData.newTile;
        tile.CopyFrom(TileObjectData.Style3x2);
        tile.Height = 1;
        tile.CoordinateHeights = [16];
        //tile.UsesCustomCanPlace = true;
        tile.Origin = new Point16(1, 0);
        tile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.AlternateTile, 1, 1);

        ref TileObjectData alternate = ref TileObjectData.newAlternate;

        alternate.CopyFrom(tile);
        alternate.AnchorBottom = AnchorData.Empty;
        alternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, 1, 0);
        TileObjectData.addAlternate(0);

        alternate.CopyFrom(tile);
        alternate.AnchorBottom = AnchorData.Empty;
        alternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide | AnchorType.Tree | AnchorType.AlternateTile, 1, 0);
        TileObjectData.addAlternate(0);

        TileObjectData.addTile(Type);

        AddMapEntry(new Color(108, 71, 43), CreateMapEntryName());
        RegisterItemDrop(this.ItemType());

        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;
    }

    public override bool RightClick(int i, int j)
    {
        if (GetTableAtPosition(i, j, out TableEntity? table) && table.TableclothType != 0 && Main.mouseRight && Main.mouseRightRelease)
        {
            Main.mouseRightRelease = false;

            Main.LocalPlayer.QuickSpawnItem(new EntitySource_TileInteraction(Main.LocalPlayer, i, j), table.TableclothType);
            table.TableclothType = 0;
            table.Kill(i, j);

            Tablecloth.TableclothPlaceHandler.Send(true, new Point16(i, j), 0);

            return true;
        }

        return false;
    }

    private static void GetAnchors(int i, int j, out bool left, out bool right, out bool bottom)
    {
        Tile tile = Main.tile[i - 1, j];
        left = ValidAnchor(tile);

        tile = Main.tile[i + 3, j];
        right = ValidAnchor(tile) && (i < KillOrderInvalidX || i >= KillOrderInvalidXMax);

        tile = Main.tile[i + 1, j + 1];
        bottom = WorldGen.AnchorValid(tile, AnchorType.SolidTile | AnchorType.SolidWithTop) || ModContent.GetModTile(tile.TileType) is TableBase;

        static bool ValidAnchor(Tile tile)
        {
            const AnchorType Anchors = AnchorType.SolidTile | AnchorType.SolidSide;

            return WorldGen.AnchorValid(tile, Anchors) || ModContent.GetModTile(tile.TileType) is TablePlatform || TileID.Sets.IsATreeTrunk[tile.TileType];
        }
    }

    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        TileID.Sets.Platforms[Type] = true;
        Tile tile = Main.tile[i, j];
        int offsetX = tile.TileFrameX % 54 / 18;

        if (!fail && !BlockRecursion)
        {
            BlockRecursion = true;

            for (int x = 0; x < 3; ++x)
            {
                int curX = x + i - offsetX;

                if (curX != i && Main.tile[curX, j].HasTile)
                    WorldGen.KillTile(curX, j);
            }

            BlockRecursion = false;
        }

        if (offsetX == 0)
        {
            bool oldRecur = BlockRecursion;
            BlockRecursion = false;
            KillOrderInvalidX = i - 3;
            KillOrderInvalidXMax = i;

            ReframeAdjacent(i - 3, j);
            ReframeAdjacent(i + 3, j);

            KillOrderInvalidX = 0;
            KillOrderInvalidXMax = 0;
            BlockRecursion = oldRecur;
        }

        if (!BlockRecursion)
            Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, this.ItemType());

        if (GetTableAtPosition(i, j, out TableEntity? table) && table.TableclothType != 0)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, table.TableclothType);
            table.Kill(i, j);
        }
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        GetAnchors(i, j, out bool left, out bool right, out bool bottom);
        Tile tile = Main.tile[i, j];
        int offsetX = tile.TileFrameX % 54 / 18;

        if (offsetX != 0)
            return false;

        int frame = 0;

        if (left && right)
            frame = 2;
        else if (left && !right)
            frame = 3;
        else if (right && !left)
            frame = 1;

        if (!bottom)
            frame += 4;

        for (int x = 0; x < 3; ++x)
        {
            Tile subTile = Main.tile[i - offsetX + x, j];
            subTile.TileFrameX = (short)(frame * 54 + x * 18);
        }

        ReframeAdjacent(i - 3, j);
        ReframeAdjacent(i + 3, j);

        return false;
    }

    static void ReframeAdjacent(int x, int y)
    {
        if (BlockRecursion)
            return;

        Tile tile = Main.tile[x, y];

        if (ModContent.GetModTile(tile.TileType) is TablePlatform)
        {
            BlockRecursion = true;
            WorldGen.Reframe(x, y);
            BlockRecursion = false;
        }
    }

    public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
    {
        if (GetTableAtPosition(i, j, out _))
            Main.instance.TilesRenderer.AddSpecialPoint(i, j, Terraria.GameContent.Drawing.TileDrawing.TileCounterType.CustomNonSolid);
    }

    public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
    {
        if (GetTableAtPosition(i, j, out TableEntity? table))
        {
            int type = table.TableclothType;

            if (type == 0)
                return;

            bool left = GetTableAtPosition(i - 3, j, out _);
            bool right = GetTableAtPosition(i + 3, j, out _);
            int frame = 0;

            if (left && right)
                frame = 2;
            else if (left)
                frame = 3;
            else if (right)
                frame = 1;

            Texture2D tex = Tablecloth.PlacedTexturesByType[type].Value;
            Main.spriteBatch.Draw(tex, new Vector2(i - 1, j) * 16 - Main.screenPosition, new Rectangle(frame * 50, 0, 48, 30), Lighting.GetColor(i, j));
        }
    }

    public static bool GetTableAtPosition(int i, int j, [NotNullWhen(true)] out TableEntity? table)
    {
        table = null;
        bool found = false;

        if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity? ent) && ent is TableEntity tb)
        {
            table = tb;
            found = true;
        }

        return found;
    }
}
