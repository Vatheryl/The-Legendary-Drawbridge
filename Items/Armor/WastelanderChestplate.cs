using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items.Armor
{
	public class WastelanderChestplate : ModItem
	{
		public override string Texture => "Drawbridge/Textures/WasteLanderSet/Wastelander_Chesplate";

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(0, 1, 50, 0);
			Item.vanity = true;
			if (WastelanderVanitySystem.WastelanderBodySlot >= 0)
			{
				Item.bodySlot = WastelanderVanitySystem.WastelanderBodySlot;
			}
		}

		public override void SetStaticDefaults()
		{
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Leather, 12)
				.AddIngredient(ItemID.Silk, 15)
				.AddIngredient(ItemID.AntlionMandible, 8)
				.AddIngredient(ItemID.Cactus, 20)
				.AddTile(TileID.Loom)
				.Register();
		}
	}
}

