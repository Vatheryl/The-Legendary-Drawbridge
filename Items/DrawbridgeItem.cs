using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Drawbridge.Tiles;

namespace Drawbridge.Items
{
    public class DrawbridgeItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = false;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            // Item.createTile will be set in HoldItem
            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Blue;
        }

        public override void HoldItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                // Place flipped vertical tile when facing right
                Item.createTile = player.direction == 1
                    ? ModContent.TileType<DrawbridgeVerticalFlippedTile>()
                    : ModContent.TileType<DrawbridgeTile>();
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 50)
                .AddIngredient(ItemID.GrayBrick, 30)
                .AddIngredient(ItemID.Chain, 10)
                .AddRecipeGroup(RecipeGroupID.IronBar, 15)
                .AddTile(TileID.HeavyWorkBench)
                .Register();
        }
    }
}