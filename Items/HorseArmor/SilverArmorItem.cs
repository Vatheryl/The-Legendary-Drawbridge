using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items.HorseArmor;

public class SilverArmorItem : HorseArmorItem
{
    internal override int DefenseBonus => 5;
    internal override string OverlayPath => "Drawbridge/Textures/Silver_Armor";

    protected override void Defaults()
    {
        Item.value = Item.buyPrice(silver: 50);
        Item.rare = ItemRarityID.White;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.SilverBar, 7)
            .AddIngredient(ItemID.Silk, 4)
            .AddIngredient(ModContent.ItemType<CopperArmorItem>(), 1)
            .AddTile(TileID.Anvils)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.TungstenBar, 7)
            .AddIngredient(ItemID.Silk, 4)
            .AddIngredient(ModContent.ItemType<CopperArmorItem>(), 1)
            .AddTile(TileID.Anvils)
            .Register();
    }
}
