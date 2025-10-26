using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items.Armor
{
	[AutoloadEquip(EquipType.Head)]
	public class SkyLionHelmet : ModItem
	{
		public override string Texture => "Drawbridge/Textures/SkyLionSet/SkyLionMane";

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(0, 2, 50, 0);
			Item.defense = 6;
		}

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Sky Lion Helmet");
			// Tooltip.SetDefault("The majestic mane of a sky lion\nProvides protection from the heavens");
		}

		public override void UpdateEquip(Player player)
		{
			player.GetDamage(DamageClass.Generic) += 0.05f; // 5% damage increase
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ModContent.ItemType<SkyLionChestplate>() && legs.type == ModContent.ItemType<SkyLionLeggings>();
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "Causes stars to fall after taking damage";	
			player.GetModPlayer<SkyLionPlayer>().skyLionSet = true;
		}

		public override void AddRecipes()
		{
			// Recipe with Shadow Scales (Corruption variant)
			CreateRecipe()
				.AddIngredient(ItemID.SunplateBlock, 12)
				.AddIngredient(ItemID.FallenStar, 5)
				.AddIngredient(ItemID.Cloud, 8)
				.AddIngredient(ItemID.ShadowScale, 3)
				.AddTile(TileID.SkyMill)
				.Register();

			// Recipe with Tissue Samples (Crimson variant)
			CreateRecipe()
				.AddIngredient(ItemID.SunplateBlock, 12)
				.AddIngredient(ItemID.FallenStar, 5)
				.AddIngredient(ItemID.Cloud, 8)
				.AddIngredient(ItemID.TissueSample, 3)
				.AddTile(TileID.SkyMill)
				.Register();
		}
	}
}
