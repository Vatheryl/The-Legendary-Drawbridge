using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Drawbridge.Tiles.Drawbridges.Lion;

namespace Drawbridge.Items
{
    public class LionDrawbridgeItem : ModItem
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
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Orange; // Lion-themed rarity
        }

        public override void HoldItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                Item.createTile = player.direction == 1
                    ? ModContent.TileType<LionDrawbridgeVerticalFlippedTile>()
                    : ModContent.TileType<LionDrawbridgeTile>();
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.GoldBrick, 30)
                .AddIngredient(ItemID.Chain, 10)
                .AddIngredient(ItemID.Marble, 20)
                .AddTile(TileID.HeavyWorkBench)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.PlatinumBrick, 30)
                .AddIngredient(ItemID.Chain, 10)
                .AddIngredient(ItemID.Marble, 20)
                .AddTile(TileID.HeavyWorkBench)
                .Register();
        }
    }
}