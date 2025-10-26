using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items
{
    public class HookItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 32;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.HookTile>();
            Item.value = Item.sellPrice(silver: 5);
            Item.rare = ItemRarityID.White;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.IronBar, 3)
                .AddIngredient(ItemID.Chain, 2)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.LeadBar, 3)
                .AddIngredient(ItemID.Chain, 2)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
