using Terraria;
using Terraria.ID;

namespace Drawbridge.Items.HorseArmor;

public class CopperArmorItem : HorseArmorItem
{
    internal override int DefenseBonus => 1;
    internal override string OverlayPath => "Drawbridge/Textures/Copper_Armor";

    protected override void Defaults()
    {
        Item.value = Item.buyPrice(silver: 25);
        Item.rare = ItemRarityID.White;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.CopperBar, 6)
            .AddIngredient(ItemID.Silk, 3)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
