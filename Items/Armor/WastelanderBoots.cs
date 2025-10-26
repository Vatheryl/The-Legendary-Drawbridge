using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items.Armor
{
	public class WastelanderBoots : ModItem
	{
		public override string Texture => "Drawbridge/Textures/WasteLanderSet/Wastelander_Boots_Inv";

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.buyPrice(0, 0, 75, 0);
			Item.vanity = true;
			if (WastelanderVanitySystem.WastelanderLegsSlot >= 0)
			{
				Item.legSlot = WastelanderVanitySystem.WastelanderLegsSlot;
			}
		}

		public override void SetStaticDefaults()
		{
		}

		public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ItemID.Leather, 10)
				.AddIngredient(ItemID.Silk, 12)
				.AddIngredient(ItemID.AntlionMandible, 6)
				.AddTile(TileID.Loom)
				.Register();
		}
	}
}

