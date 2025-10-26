using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Drawbridge.Tiles;

namespace Drawbridge.Items
{
    public class PortcullisItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 128;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = false;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<PortcullisDownTile>();
            Item.value = Item.sellPrice(silver: 50);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.GrayBrick, 10)
                .AddIngredient(ItemID.Wood, 5)
                .AddTile(TileID.HeavyWorkBench)
                .Register();
        }
    }
} 