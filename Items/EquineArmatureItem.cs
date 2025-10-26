using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Drawbridge.Tiles;

namespace Drawbridge.Items
{
    public class EquineArmatureItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<EquineArmatureTile>();
            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.White;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 40)
                .AddTile(TileID.Sawmill)
                .Register();
        }
    }
}


