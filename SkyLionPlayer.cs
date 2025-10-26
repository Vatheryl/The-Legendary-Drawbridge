using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge
{
	public class SkyLionPlayer : ModPlayer
	{
		public bool skyLionSet = false;
		public bool skyLionFullSetWithCloak = false;

		public override void ResetEffects()
		{
			skyLionSet = false;
			skyLionFullSetWithCloak = false;
		}

		public override void PostUpdateEquips()
		{
			// Check if player has the full Sky Lion set including the cloak
			bool hasHelmet = Player.armor[0].type == ModContent.ItemType<Items.Armor.SkyLionHelmet>();
			bool hasChestplate = Player.armor[1].type == ModContent.ItemType<Items.Armor.SkyLionChestplate>();
			bool hasLeggings = Player.armor[2].type == ModContent.ItemType<Items.Armor.SkyLionLeggings>();
			
			// Check for cloak in accessory slots (including vanity slots)
			bool hasCloak = false;
			for (int i = 0; i < Player.armor.Length; i++)
			{
				if (Player.armor[i].type == ModContent.ItemType<Items.Vanity.SkyLionCloak>())
				{
					hasCloak = true;
					break;
				}
			}

			skyLionFullSetWithCloak = hasHelmet && hasChestplate && hasLeggings && hasCloak;

			// Add dim golden light effect when full set is equipped
			if (skyLionFullSetWithCloak)
			{
				Lighting.AddLight(Player.Center, 0.4f, 0.35f, 0.1f); // Dim golden/white light
			}
		}

		public override void OnHurt(Player.HurtInfo info)
		{
			if (skyLionSet && info.Damage > 1)
			{
				// Star Cloak effect - spawn falling stars when taking damage
				int starCount = Main.rand.Next(2, 5); // 2-4 stars
				
				for (int i = 0; i < starCount; i++)
				{
					// Spawn stars above the player
					Vector2 spawnPosition = new Vector2(
						Player.Center.X + Main.rand.Next(-400, 401), // Random X offset
						Player.Center.Y - Main.rand.Next(300, 501)   // Above the player
					);

					// Create falling star projectile
					Vector2 velocity = new Vector2(
						Main.rand.Next(-30, 31) * 0.1f, // Slight horizontal drift
						Main.rand.Next(50, 101) * 0.1f  // Downward velocity
					);

					int starDamage = (int)(Player.GetTotalDamage(DamageClass.Generic).ApplyTo(35)); // Base 35 damage
					
					Projectile.NewProjectile(
						Player.GetSource_Accessory(Player.armor[0]),
						spawnPosition,
						velocity,
						ProjectileID.StarCannonStar,
						starDamage,
						4f, // Knockback
						Player.whoAmI
					);
				}
			}
		}
	}
}
