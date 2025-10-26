using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items.Armor
{
	[AutoloadEquip(EquipType.Legs)]
	public class SkyLionLeggings : ModItem
	{
		public override string Texture => "Drawbridge/Textures/SkyLionSet/SkyLionGreaves";

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(0, 2, 0, 0);
			Item.defense = 5;
		}

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Sky Lion Leggings");
			// Tooltip.SetDefault("Leg armor that grants the agility of a sky lion\nAllows swift movement through the clouds");
		}

		public override void UpdateEquip(Player player)
		{
			player.moveSpeed += 0.10f; // 10% movement speed increase
			player.jumpSpeedBoost += 0.8f; // Jump speed boost
		}

		public override void AddRecipes()
		{
			// Recipe with Shadow Scales (Corruption variant)
			CreateRecipe()
				.AddIngredient(ItemID.SunplateBlock, 15)
				.AddIngredient(ItemID.FallenStar, 6)
				.AddIngredient(ItemID.Cloud, 10)
				.AddIngredient(ItemID.ShadowScale, 4)
				.AddTile(TileID.SkyMill)
				.Register();

			// Recipe with Tissue Samples (Crimson variant)
			CreateRecipe()
				.AddIngredient(ItemID.SunplateBlock, 15)
				.AddIngredient(ItemID.FallenStar, 6)
				.AddIngredient(ItemID.Cloud, 10)
				.AddIngredient(ItemID.TissueSample, 4)
				.AddTile(TileID.SkyMill)
				.Register();
		}
	}
}
