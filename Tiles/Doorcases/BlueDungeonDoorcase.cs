using Terraria.ID;

namespace Drawbridge.Tiles.Doorcases;

internal class BlueDungeonDoorcase : Doorcase<BlueDungeonDoorcase.BlueDungeonDoorcaseItem>
{
    public class BlueDungeonDoorcaseItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<BlueDungeonDoorcase>());
            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.Blue;
            Item.mech = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient(ItemID.Book, 8).AddIngredient(ItemID.Wire, 8).AddIngredient(ItemID.BlueBrick, 20).AddIngredient(ItemID.FallenStar, 3)
                .AddTile(TileID.HeavyWorkBench).Register();
        }
    }
}
