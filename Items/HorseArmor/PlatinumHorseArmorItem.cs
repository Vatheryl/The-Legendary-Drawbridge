using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items.HorseArmor;

public class PlatinumHorseArmorItem : HorseArmorItem
{
	internal override int DefenseBonus => 9;
	internal override string OverlayPath => "Drawbridge/Textures/PlatinumHorseArmor";

    protected override void Defaults()
	{
		Item.value = Item.buyPrice(gold: 1, silver: 75);
		Item.rare = ItemRarityID.Yellow;
	}

	public override void AddRecipes()
	{
		CreateRecipe()
			.AddIngredient(ItemID.GoldBar, 10)
			.AddIngredient(ItemID.Silk, 8)
			.AddIngredient(ModContent.ItemType<IronFieldBardingItem>(), 1)
			.AddTile(TileID.Anvils)
			.Register();

		CreateRecipe()
			.AddIngredient(ItemID.PlatinumBar, 10)
			.AddIngredient(ItemID.Silk, 8)
			.AddIngredient(ModContent.ItemType<IronFieldBardingItem>(), 1)
			.AddTile(TileID.Anvils)
			.Register();
	}
}



