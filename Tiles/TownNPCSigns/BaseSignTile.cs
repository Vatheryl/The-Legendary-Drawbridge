using Drawbridge.Common.Tiles;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace Drawbridge.Tiles.TownNPCSigns;

public abstract class BaseSignTile : ModTile, IAutoloadTileItem
{
    public virtual void StaticItemDefaults(ModItem item) { }

    public void SetItemDefaults(ModItem item) 
    {
        item.Item.width = 26;
        item.Item.height = 30;
        item.Item.maxStack = Item.CommonMaxStack;
        item.Item.useTurn = true;
        item.Item.autoReuse = true;
        item.Item.useAnimation = 15;
        item.Item.useTime = 10;
        item.Item.useStyle = ItemUseStyleID.Swing;
        item.Item.consumable = true;
        item.Item.placeStyle = 0;
        item.Item.value = Item.buyPrice(0, 0, 0, 50);
        item.Item.rare = ItemRarityID.White;

        PostSetItemDefaults(item);
    }

    protected virtual void PostSetItemDefaults(ModItem item) { }

    public virtual void AddItemRecipes(ModItem item) => item.CreateRecipe().AddIngredient(ItemID.Wood, 8).AddTile(TileID.WorkBenches).Register();

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
        int signX = i - frameX / 18 % 3;
        int signY = j - frameY / 18 % 3;
        
        // Kill the sign data
        Sign.KillSign(signX, signY);

        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 48, 48, this.ItemType());
    }

    public override bool RightClick(int i, int j)
    {
        Player player = Main.LocalPlayer;
        
        // Get the top-left position of the sign
        Tile tile = Main.tile[i, j];
        int signX = i - tile.TileFrameX / 18 % 3;
        int signY = j - tile.TileFrameY / 18 % 3;
        
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
