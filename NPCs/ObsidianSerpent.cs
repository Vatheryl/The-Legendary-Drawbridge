using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Chat;
using Terraria.Localization;

namespace Drawbridge.NPCs
{
	public class ObsidianSerpent : ModNPC
	{
		public override string Texture => "Terraria/Images/NPC_39"; // Bone Serpent Head

		public override void SetStaticDefaults()
		{
			// Display name is provided via localization files if desired
			Main.npcFrameCount[Type] = 1;
		}

		public override void SetDefaults()
		{
			NPC.width = 34;
			NPC.height = 34;
			NPC.aiStyle = -1; // custom to avoid vanilla vine drawing
			NPC.damage = 35;
			NPC.defense = 8;
			NPC.lifeMax = 180;
			NPC.knockBackResist = 0.35f;
			NPC.value = Item.buyPrice(0, 0, 50);
			NPC.HitSound = SoundID.NPCHit2;
			NPC.DeathSound = SoundID.NPCDeath2;
			NPC.noGravity = true;
			NPC.noTileCollide = false; // collide with tiles
			NPC.behindTiles = false;
		}

		public override void AI()
		{
			Vector2 anchor = new Vector2(NPC.ai[0] * 16f + 8f, NPC.ai[1] * 16f + 16f - 6f);
			UpdateHeadMovement(anchor);
			NPC.spriteDirection = NPC.direction;

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				UpdateSegments(anchor);
			}
		}

		private const float SegmentSpacingStraight = 16f; // spacing in straight/chase state
		private const float SegmentSpacingCurved = 9f;  // tighter spacing while curving in idle
		private const int MaxSegments = 40; // generous maximum to cover long extensions
		private const float MaxReach = 150f; // Man Eater-like max reach
		private const float TurnAccel = 0.035f; // Man Eater-like turn speed
		// Idle behavior tuning
		private const float IdleDistanceTiles = 32f; // player distance from anchor to trigger idle
		private const float PixelsPerTile = 16f;
		private const float IdleDistancePixels = IdleDistanceTiles * PixelsPerTile;
		private const float IdleSitHeight = 120f; // how high above the anchor the head rises when idle
		private const float IdleBobAmplitude = 6f; // small vertical bob while idle
		private const float IdleBobSpeed = 0.6f; // slower cycles per second for smoother idle
		private const float IdleBlendSpeed = 0.03f; // how fast to blend into/out of idle

		private static float MoveTowards(float current, float target, float maxDelta)
		{
			if (current < target)
				return Math.Min(current + maxDelta, target);
			if (current > target)
				return Math.Max(current - maxDelta, target);
			return current;
		}

		private static float AngleTowards(float current, float target, float maxDelta)
		{
			float diff = MathHelper.WrapAngle(target - current);
			if (diff > maxDelta) diff = maxDelta;
			if (diff < -maxDelta) diff = -maxDelta;
			return current + diff;
		}

		private void UpdateHeadMovement(Vector2 anchor)
		{
			// If the anchor tile was destroyed, despawn
			int tileX = (int)NPC.ai[0];
			int tileY = (int)NPC.ai[1];
			bool anchorValid = !(tileX < 0 || tileY < 0 || tileX >= Main.maxTilesX || tileY >= Main.maxTilesY || Main.tile[tileX, tileY] == null || !Main.tile[tileX, tileY].HasTile);
			if (!anchorValid)
			{
				NPC.StrikeInstantKill();
				return;
			}

			NPC.TargetClosest();
			Player target = Main.player[NPC.target];
			// Decide if we should idle based on player distance from the anchor and smoothly blend
			bool idleNow = !target.active || target.dead || Vector2.Distance(target.Center, anchor) > IdleDistancePixels;
			float idleWeight = MoveTowards(NPC.localAI[1], idleNow ? 1f : 0f, IdleBlendSpeed);
			NPC.localAI[1] = idleWeight;
			Vector2 desiredPos;
			// Vector from anchor to target, clamped to MaxReach (chase position)
			Vector2 offsetChase = target.Center - anchor;
			float len = offsetChase.Length();
			if (len > MaxReach)
				offsetChase *= MaxReach / (len + 1e-6f);
			Vector2 desiredChase = anchor + offsetChase;
			// Idle sit-up position straight above anchor with a little bob
			float bob = (float)Math.Sin(Main.GlobalTimeWrappedHourly * (float)(Math.PI * 2.0) * IdleBobSpeed) * IdleBobAmplitude;
			Vector2 desiredIdle = anchor + new Vector2(0f, -IdleSitHeight + bob);
			desiredPos = Vector2.Lerp(desiredChase, desiredIdle, idleWeight);
			// Accelerate toward desiredPos in axis-aligned fashion (Man Eater feel)
			if (NPC.Center.X < desiredPos.X)
			{
				NPC.velocity.X += TurnAccel;
				if (NPC.velocity.X < 0f)
					NPC.velocity.X += TurnAccel;
			}
			else
			{
				NPC.velocity.X -= TurnAccel;
				if (NPC.velocity.X > 0f)
					NPC.velocity.X -= TurnAccel;
			}
			if (NPC.Center.Y < desiredPos.Y)
			{
				NPC.velocity.Y += TurnAccel;
				if (NPC.velocity.Y < 0f)
					NPC.velocity.Y += TurnAccel;
			}
			else
			{
				NPC.velocity.Y -= TurnAccel;
				if (NPC.velocity.Y > 0f)
					NPC.velocity.Y -= TurnAccel;
			}
			// Clamp speeds similar to vanilla behavior
			float maxSpeedX = 3.0f;
			float maxSpeedY = 3.0f;
			if (NPC.velocity.X > maxSpeedX) NPC.velocity.X = maxSpeedX;
			if (NPC.velocity.X < -maxSpeedX) NPC.velocity.X = -maxSpeedX;
			if (NPC.velocity.Y > maxSpeedY) NPC.velocity.Y = maxSpeedY;
			if (NPC.velocity.Y < -maxSpeedY) NPC.velocity.Y = -maxSpeedY;
			NPC.direction = (NPC.velocity.X >= 0f) ? 1 : -1;

			// Head rotation: always face the player
			Player targetPlayer = Main.player[NPC.target];
			Vector2 toPlayer = targetPlayer.Center - NPC.Center;
			if (toPlayer.Length() > 0.001f)
			{
				NPC.rotation = (float)Math.Atan2(toPlayer.Y, toPlayer.X) + 1.57f;
			}

			// Bounce off tiles if colliding (prevents going through solids)
			if (NPC.collideX)
			{
				NPC.velocity.X = NPC.oldVelocity.X * -0.7f;
				if (NPC.velocity.X > 0f && NPC.velocity.X < 1f) NPC.velocity.X = 1f;
				if (NPC.velocity.X < 0f && NPC.velocity.X > -1f) NPC.velocity.X = -1f;
			}
			if (NPC.collideY)
			{
				NPC.velocity.Y = NPC.oldVelocity.Y * -0.7f;
				if (NPC.velocity.Y > 0f && NPC.velocity.Y < 1f) NPC.velocity.Y = 1f;
				if (NPC.velocity.Y < 0f && NPC.velocity.Y > -1f) NPC.velocity.Y = -1f;
			}
		}

		private void UpdateSegments(Vector2 anchor)
		{
			Vector2 toHead = NPC.Center - anchor;
			float distance = toHead.Length();
			if (distance < 0.001f)
				return;
			Vector2 dir = toHead / distance;
			float rotation = (float)Math.Atan2(dir.Y, dir.X);

			// Blend segment density so there are more segments (tighter spacing) while curving
			float spacing = MathHelper.Lerp(SegmentSpacingStraight, SegmentSpacingCurved, NPC.localAI[1]);
			int desiredCount = Math.Max(1, Math.Min(MaxSegments, (int)Math.Ceiling(distance / spacing)));
			if ((int)NPC.localAI[0] != desiredCount)
			{
				NPC.localAI[0] = desiredCount;
			}

			// Collect existing segments owned by this head, keyed by order
			Dictionary<int, int> orderToIndex = new Dictionary<int, int>();
			List<int> extras = new List<int>();
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				NPC seg = Main.npc[i];
				if (!seg.active || seg.realLife != NPC.whoAmI)
					continue;
				if (seg.type != ModContent.NPCType<ObsidianSerpentBody>() && seg.type != ModContent.NPCType<ObsidianSerpentTail>())
					continue;
				int order = (int)seg.ai[0];
				if (order <= 0 || orderToIndex.ContainsKey(order))
				{
					extras.Add(i);
					continue;
				}
				orderToIndex[order] = i;
			}

			var source = NPC.GetSource_FromAI();
			// Ensure 1..desiredCount exist; order 1 is tail, others are bodies
			for (int order = 1; order <= desiredCount; order++)
			{
				if (!orderToIndex.ContainsKey(order))
				{
					int type = (order == 1) ? ModContent.NPCType<ObsidianSerpentTail>() : ModContent.NPCType<ObsidianSerpentBody>();
					int idx = NPC.NewNPC(source, (int)NPC.Center.X, (int)NPC.Center.Y, type);
					if (idx >= 0)
					{
						Main.npc[idx].realLife = NPC.whoAmI;
						Main.npc[idx].ai[0] = order;
						Main.npc[idx].netUpdate = true;
						orderToIndex[order] = idx;
					}
				}
			}

			// Kill extras and orders beyond desiredCount
			foreach (var kv in orderToIndex)
			{
				if (kv.Key > desiredCount)
				{
					Main.npc[kv.Value].active = false; // silent remove to avoid OnKill cascade
				}
			}
			foreach (int idx in extras)
			{
				if (idx >= 0 && idx < Main.maxNPCs && Main.npc[idx].active)
					Main.npc[idx].active = false;
			}

			// Position existing segments
			// If the target is far from the anchor (idle), arrange segments in an "S" curve for an idle pose
			Player target = Main.player[NPC.target];
			float distPlayerFromAnchor = Vector2.Distance(target.Center, anchor);
			bool outOfReach = distPlayerFromAnchor > IdleDistancePixels || !target.active || target.dead;
			Vector2 normal = new Vector2(-dir.Y, dir.X);
			Vector2 previousPoint = anchor;
			for (int order = 1; order <= desiredCount; order++)
			{
				if (!orderToIndex.TryGetValue(order, out int idx))
					continue;
				NPC seg = Main.npc[idx];
				float desiredOffset = spacing * order;
				float endBuffer = MathHelper.Lerp(8f, 0f, NPC.localAI[1]);
				if (desiredOffset > distance - endBuffer)
					desiredOffset = Math.Max(0f, distance - endBuffer);

				Vector2 desiredCenter;
				if (outOfReach)
				{
					// Idle inverted S-shape: lateral offsets mirrored so the curve inverts; strength blends with idle
					float t = order / (float)desiredCount; // [0,1]
					float envelope = (float)Math.Sin(Math.PI * t); // zero at ends, max mid
					float amplitude = MathHelper.Clamp(distance * 0.25f, 12f, 36f) * NPC.localAI[1];
					int sideSign = (target.Center.X >= NPC.Center.X) ? 1 : -1; // orient curve toward player side
					float micro = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 0.6f + t * 0.7f) * 0.3f * (1f - t) * NPC.localAI[1];
					float s = sideSign * (float)Math.Sin(2f * Math.PI * t) * amplitude * envelope + micro;
					// Build straight and idle positions and blend for smooth transition
					Vector2 posStraight = anchor + dir * desiredOffset;
					Vector2 posIdle = anchor + dir * desiredOffset + normal * s;
					desiredCenter = Vector2.Lerp(posStraight, posIdle, NPC.localAI[1]);
					// Ensure the very last segment reaches the head when almost fully idle
					if (NPC.localAI[1] > 0.98f && order == desiredCount)
						desiredCenter = NPC.Center;
				}
				else
				{
					// Even when not outOfReach, continue to blend out any remaining idleWeight
					float t = order / (float)desiredCount; // [0,1]
					float envelope = (float)Math.Sin(Math.PI * t);
					float amplitude = MathHelper.Clamp(distance * 0.25f, 12f, 36f) * NPC.localAI[1];
					int sideSign = (target.Center.X >= NPC.Center.X) ? 1 : -1;
					float micro = (float)Math.Sin(Main.GlobalTimeWrappedHourly * 0.6f + t * 0.7f) * 0.3f * (1f - t) * NPC.localAI[1];
					float s = sideSign * (float)Math.Sin(2f * Math.PI * t) * amplitude * envelope + micro;
					Vector2 posStraight = anchor + dir * desiredOffset;
					Vector2 posIdle = anchor + dir * desiredOffset + normal * s;
					desiredCenter = Vector2.Lerp(posStraight, posIdle, NPC.localAI[1]);
				}

				seg.Center = desiredCenter;
				seg.velocity = Vector2.Zero;
				Vector2 tangent = desiredCenter - previousPoint;
				if (tangent.LengthSquared() < 0.0001f)
				{
					tangent = dir;
				}
				else
				{
					tangent.Normalize();
				}
				seg.direction = (tangent.X >= 0f) ? 1 : -1;
				seg.spriteDirection = seg.direction;
				// Rotate so the segment faces along the local curve direction, smoothing abrupt changes
				float targetRot = (float)Math.Atan2(tangent.Y, tangent.X) + 1.57f;
				float maxRotStep = MathHelper.Lerp(0.22f, 0.08f, NPC.localAI[1]); // fast when leaving idle, slow when nearly straight
				seg.rotation = AngleTowards(seg.rotation, targetRot, maxRotStep);
				seg.dontTakeDamage = true;
				seg.chaseable = false;
				seg.netUpdate = true;
				previousPoint = desiredCenter;
			}
		}

		public override void OnKill()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;
			for (int k = 0; k < Main.maxNPCs; k++)
			{
				NPC seg = Main.npc[k];
				if (seg.active && seg.realLife == NPC.whoAmI && (seg.type == ModContent.NPCType<ObsidianSerpentBody>() || seg.type == ModContent.NPCType<ObsidianSerpentTail>()))
				{
					seg.StrikeInstantKill();
				}
			}
		}

		public override bool? CanBeHitByProjectile(Projectile projectile)
		{
			// Ignore hits from our own invisible segments projectiles if any existed; also ignore friendly proj
			if (projectile != null && (projectile.hostile == false || (projectile.owner >= 0 && projectile.owner < 255 && Main.player[projectile.owner].active)))
				return false;
			return null;
		}

		public override void HitEffect(NPC.HitInfo hit)
		{
			if (NPC.life <= 0 && Main.netMode != NetmodeID.Server)
			{
				for (int i = 0; i < 10; i++)
				{
					Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Bone);
				}
			}
		}
	}
}


