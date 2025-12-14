using Terraria;
using Terraria.ID;

namespace Drawbridge.Items.HorseArmor;

public class IronFieldBardingItem : HorseArmorItem
{
    internal override int DefenseBonus => 3;
    internal override string OverlayPath => "Drawbridge/Textures/IronFieldBarding";

    protected override void Defaults()
    {
        Item.value = Item.buyPrice(silver: 75);
        Item.rare = ItemRarityID.White;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.IronBar, 8)
            .AddIngredient(ItemID.Silk, 5)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.LeadBar, 8)
            .AddIngredient(ItemID.Silk, 5)
            .AddTile(TileID.Anvils)
            .Register();
    }
}


