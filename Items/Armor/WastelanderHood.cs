using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items.Armor
{
	public class WastelanderHood : ModItem
	{
		public override string Texture => "Drawbridge/Textures/WasteLanderSet/Wasterlander_Hood_inv";

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(0, 1, 0, 0);
			Item.vanity = true;
			if (WastelanderVanitySystem.WastelanderHeadSlot >= 0)
			{
				Item.headSlot = WastelanderVanitySystem.WastelanderHeadSlot;
			}
		}

		public override void SetStaticDefaults()
		{
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Leather, 8)
				.AddIngredient(ItemID.Silk, 10)
				.AddIngredient(ItemID.AntlionMandible, 5)
				.AddTile(TileID.Loom)
				.Register();
		}
	}
}

