using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items.Vanity
{
	public class SkyLionCloak : ModItem
	{
		public override string Texture => "Drawbridge/Textures/SkyLionSet/SkyLionCloak";

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(0, 1, 0, 0);
			Item.vanity = true;
			Item.accessory = true;
			if (SkyLionVanitySystem.SkyLionCloakFrontSlot >= 0)
			{
				Item.frontSlot = SkyLionVanitySystem.SkyLionCloakFrontSlot;
			}
			if (SkyLionVanitySystem.SkyLionCloakBackSlot >= 0)
			{
				Item.backSlot = SkyLionVanitySystem.SkyLionCloakBackSlot;
			}
		}

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Sky Lion Cloak");
			// Tooltip.SetDefault("A majestic cloak that flows like the wind");
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Cloud, 15)
				.AddIngredient(ItemID.FallenStar, 3)
				.AddIngredient(ItemID.Silk, 8)
				.AddTile(TileID.Loom)
				.Register();
		}
	}
}
