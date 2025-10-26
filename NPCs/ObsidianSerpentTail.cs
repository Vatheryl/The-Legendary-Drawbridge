using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.NPCs
{
	public class ObsidianSerpentTail : ModNPC
	{
		public override string Texture => "Terraria/Images/NPC_41"; // Bone Serpent Tail

		public override void SetStaticDefaults()
		{
			Main.npcFrameCount[Type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.width = 26;
			NPC.height = 26;
			NPC.damage = 0;
			NPC.defense = 9999;
			NPC.lifeMax = 9999;
			NPC.knockBackResist = 0f;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.dontTakeDamage = true;
			NPC.chaseable = false;
			NPC.friendly = false;
			// Prevent damaging or colliding with its own head or other segments
			NPC.dontTakeDamageFromHostiles = true;
		}

		public override bool? CanBeHitByItem(Player player, Item item) => false;
		public override bool? CanBeHitByProjectile(Projectile projectile) => false;
		public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;

		public override void AI()
		{
			// Passively positioned by the head NPC; this segment should not do autonomous movement
			NPC.spriteDirection = NPC.direction;
			NPC.velocity = Vector2.Zero;
		}
	}
}


