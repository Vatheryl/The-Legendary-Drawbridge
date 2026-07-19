using Terraria.ID;

namespace Drawbridge.Tiles.Tables;

internal class RedTablecloth : Tablecloth
{
    public override void AddRecipes() => CreateRecipe().AddIngredient(ItemID.Silk, 2).AddIngredient(ItemID.RedDye).AddIngredient(ItemID.Loom).Register();
}
