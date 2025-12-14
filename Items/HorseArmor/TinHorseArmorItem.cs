using Terraria;
using Terraria.ID;

namespace Drawbridge.Items.HorseArmor;

public class TinHorseArmorItem : HorseArmorItem
{
    internal override int DefenseBonus => 2;
    internal override string OverlayPath => "Drawbridge/Textures/TinHorseArmor";

    protected override void Defaults()
    {
        Item.value = Item.buyPrice(silver: 30);
        Item.rare = ItemRarityID.White;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.TinBar, 6)
            .AddIngredient(ItemID.Silk, 3)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
