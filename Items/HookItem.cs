using Terraria;
using Terraria.ID;

namespace Drawbridge.Items;

public class HookItem : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 32;
        Item.maxStack = Item.CommonMaxStack;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<Tiles.HookTileNew>();
        Item.value = Item.sellPrice(silver: 5);
        Item.rare = ItemRarityID.White;
    }

    public override void AddRecipes() => CreateRecipe()
        .AddRecipeGroup(RecipeGroupID.IronBar, 3)
        .AddIngredient(ItemID.Chain, 2)
        .AddTile(TileID.Anvils)
        .Register();
}
