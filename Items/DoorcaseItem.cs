using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items
{
    public class DoorcaseItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.DoorcaseTile>();
            Item.placeStyle = 0;
            Item.value = Item.buyPrice(gold: 3);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Book, 20)
                .AddIngredient(ItemID.Ebonwood, 30)
                .AddIngredient(ItemID.ShadowScale, 8)
                .AddIngredient(ItemID.FallenStar, 3)
                .AddTile(TileID.HeavyWorkBench)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.Book, 20)
                .AddIngredient(ItemID.Shadewood, 30)
                .AddIngredient(ItemID.TissueSample, 8)
                .AddIngredient(ItemID.FallenStar, 3)
                .AddTile(TileID.HeavyWorkBench)
                .Register();
        }
    }
}
