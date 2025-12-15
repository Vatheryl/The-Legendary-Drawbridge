using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items.HorseArmor;

public class MoltenArmorItem : HorseArmorItem
{
    internal override int DefenseBonus => 15;
    internal override string OverlayPath => "Drawbridge/Textures/Molten_Horse_Armor";

    protected override void Defaults()
    {
        Item.value = Item.buyPrice(gold: 3);
        Item.rare = ItemRarityID.Orange;
    }

    public override void AddRecipes() => CreateRecipe()
        .AddIngredient(ItemID.HellstoneBar, 12)
        .AddIngredient(ItemID.Obsidian, 5)
        .AddIngredient(ModContent.ItemType<GoldArmorItem>(), 1)
        .AddTile(TileID.Hellforge)
        .Register();
}