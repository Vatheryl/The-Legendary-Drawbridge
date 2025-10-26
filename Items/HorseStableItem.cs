using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Drawbridge.Tiles;

namespace Drawbridge.Items
{
    public class HorseStableItem : ModItem
    {
        public override string Texture => "Drawbridge/Items/HorseStableItem";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

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
            Item.createTile = ModContent.TileType<HorseStableTile>();
        }

        public override void AddRecipes()
        {
            // Gold Bar recipe
            Recipe goldRecipe = CreateRecipe();
            goldRecipe.AddIngredient(ItemID.Wood, 30);
            goldRecipe.AddIngredient(ItemID.GoldBar, 2);
            goldRecipe.AddTile(TileID.WorkBenches);
            goldRecipe.Register();

            // Platinum Bar recipe
            Recipe platRecipe = CreateRecipe();
            platRecipe.AddIngredient(ItemID.Wood, 30);
            platRecipe.AddIngredient(ItemID.PlatinumBar, 2);
            platRecipe.AddTile(TileID.WorkBenches);
            platRecipe.Register();
        }
    }
} 