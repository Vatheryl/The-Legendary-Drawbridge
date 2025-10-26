using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items
{
    public class SwordSignItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 70;
            Item.height = 34;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.SwordSignTile>();
            Item.placeStyle = 0;
            Item.value = Item.buyPrice(copper: 150);
            Item.rare = ItemRarityID.White;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 8)
                .AddIngredient(ItemID.PlatinumBar, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
