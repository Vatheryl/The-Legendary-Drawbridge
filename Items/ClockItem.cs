using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items
{
    public class ClockItem : ModItem
    {

        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 64;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.ClockTile>();
            Item.placeStyle = 0;
            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.StoneBlock, 30)
                .AddIngredient(ItemID.Glass, 10)
                .AddIngredient(ItemID.GoldBar, 5)
                .AddIngredient(ItemID.Chain, 3)
                .AddTile(TileID.HeavyWorkBench)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.StoneBlock, 30)
                .AddIngredient(ItemID.Glass, 10)
                .AddIngredient(ItemID.PlatinumBar, 5)
                .AddIngredient(ItemID.Chain, 3)
                .AddTile(TileID.HeavyWorkBench)
                .Register();
        }
    }
} 