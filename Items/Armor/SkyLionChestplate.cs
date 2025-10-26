using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items.Armor
{
	[AutoloadEquip(EquipType.Body)]
	public class SkyLionChestplate : ModItem
	{
		public override string Texture => "Drawbridge/Textures/SkyLionSet/SkyLionCuirass";

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(0, 3, 0, 0);
			Item.defense = 7;
		}

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Sky Lion Chestplate");
			// Tooltip.SetDefault("Protective chest armor of a sky lion\nGrants the power of the celestial realm");
		}

		public override void UpdateEquip(Player player)
		{
			player.GetCritChance(DamageClass.Generic) += 4; // 4% crit chance increase
		}

		public override void AddRecipes()
		{
			// Recipe with Shadow Scales (Corruption variant)
			CreateRecipe()
				.AddIngredient(ItemID.SunplateBlock, 20)
				.AddIngredient(ItemID.FallenStar, 8)
				.AddIngredient(ItemID.Cloud, 12)
				.AddIngredient(ItemID.Starfury, 1)
				.AddIngredient(ItemID.ShadowScale, 5)
				.AddTile(TileID.SkyMill)
				.Register();

			// Recipe with Tissue Samples (Crimson variant)
			CreateRecipe()
				.AddIngredient(ItemID.SunplateBlock, 20)
				.AddIngredient(ItemID.FallenStar, 8)
				.AddIngredient(ItemID.Cloud, 12)
				.AddIngredient(ItemID.Starfury, 1)
				.AddIngredient(ItemID.TissueSample, 5)
				.AddTile(TileID.SkyMill)
				.Register();
		}
	}
}
