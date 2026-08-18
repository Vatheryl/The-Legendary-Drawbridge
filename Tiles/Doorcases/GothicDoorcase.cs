using Terraria.ID;

namespace Drawbridge.Tiles.Doorcases;
internal class GothicDoorcase : Doorcase
{
    [LegacyName("DoorcaseItem")]
    public class GothicDoorcaseItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<GothicDoorcase>());
            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.Blue;
            Item.mech = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().AddIngredient(ItemID.Book, 20).AddIngredient(ItemID.Ebonwood, 30).AddIngredient(ItemID.ShadowScale, 8).AddIngredient(ItemID.FallenStar, 3)
                .AddTile(TileID.HeavyWorkBench).Register();

            CreateRecipe().AddIngredient(ItemID.Book, 20).AddIngredient(ItemID.Shadewood, 30).AddIngredient(ItemID.TissueSample, 8).AddIngredient(ItemID.FallenStar, 3)
                .AddTile(TileID.HeavyWorkBench).Register();
        }
    }
}
