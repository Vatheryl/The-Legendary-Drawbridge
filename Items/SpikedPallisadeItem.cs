using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Drawbridge.Tiles;

namespace Drawbridge.Items
{
    public class SpikedPallisadeItem : ModItem
    {
        public override string Texture => "Drawbridge/Tiles/SpikedPallisade";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 96;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<SpikedPallisadeTile>();
            Item.value = Item.buyPrice(silver: 1);
            Item.rare = ItemRarityID.White;
        }

        public override void HoldItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                Item.createTile = player.direction == 1
                    ? ModContent.TileType<SpikedPallisadeTile>()
                    : ModContent.TileType<SpikedPallisadeFlippedTile>();
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 20)
                .AddRecipeGroup(RecipeGroupID.IronBar, 5)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
} 