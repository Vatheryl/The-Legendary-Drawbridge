using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items
{
    public class SpearSignItem : ModItem
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
            Item.createTile = ModContent.TileType<Tiles.SpearSignTile>();
            Item.placeStyle = 0;
            Item.value = Item.buyPrice(copper: 75);
            Item.rare = ItemRarityID.White;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 8)
                .AddIngredient(ItemID.GoldBar, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
