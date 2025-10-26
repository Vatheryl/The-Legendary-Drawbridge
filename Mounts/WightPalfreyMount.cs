using System;
using Drawbridge.Buffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Drawbridge.Mounts
{
	public class WightPalfreyMount : ModMount
	{
		public override void SetStaticDefaults()
		{
			MountData.spawnDust = 3;
			MountData.buff = ModContent.BuffType<WightPalfreyBuff>();
			MountData.heightBoost = 34;
			MountData.flightTimeMax = 0;
			MountData.fallDamage = 0.5f;
			MountData.runSpeed = 3f;
			MountData.dashSpeed = 9f;
			MountData.acceleration = 0.25f;
			MountData.jumpHeight = 6;
			MountData.jumpSpeed = 7.01f;
			MountData.totalFrames = 16;

			int[] playerYOffsets = new int[MountData.totalFrames];
			for (int i = 0; i < playerYOffsets.Length; i++)
			{
				playerYOffsets[i] = 28;
			}
			playerYOffsets[3] += 2;
			playerYOffsets[4] += 2;
			playerYOffsets[7] += 2;
			playerYOffsets[8] += 2;
			playerYOffsets[12] += 2;
			playerYOffsets[13] += 2;
			playerYOffsets[15] += 4;
			MountData.playerYOffsets = playerYOffsets;

			MountData.xOffset = 5;
			MountData.bodyFrame = 3;
			MountData.yOffset = 1;
			MountData.playerHeadOffset = 34;

			MountData.standingFrameCount = 1;
			MountData.standingFrameDelay = 12;
			MountData.standingFrameStart = 0;

			MountData.runningFrameCount = 7;
			MountData.runningFrameDelay = 15;
			MountData.runningFrameStart = 1;

			MountData.dashingFrameCount = 6;
			MountData.dashingFrameDelay = 40;
			MountData.dashingFrameStart = 9;
			MountData.flyingFrameCount = 6;
			MountData.flyingFrameDelay = 6;
			MountData.flyingFrameStart = 1;

			MountData.inAirFrameCount = 1;
			MountData.inAirFrameDelay = 12;
			MountData.inAirFrameStart = 15;

			MountData.idleFrameCount = 0;
			MountData.idleFrameDelay = 0;
			MountData.idleFrameStart = 0;
			MountData.idleFrameLoop = false;

			MountData.swimFrameCount = MountData.inAirFrameCount;
			MountData.swimFrameDelay = MountData.inAirFrameDelay;
			MountData.swimFrameStart = MountData.inAirFrameStart;

		if (!Main.dedServ)
		{
			MountData.textureWidth = MountData.backTexture.Width();
			MountData.textureHeight = MountData.backTexture.Height();
		}
	}

	public override bool UpdateFrame(Player mountedPlayer, int state, Vector2 velocity)
	{
		bool isDashing = Math.Abs(velocity.X) > mountedPlayer.mount.DashSpeed - mountedPlayer.mount.RunSpeed / 2f;
		
		if (isDashing)
		{
			mountedPlayer.mount._frameState = 5;
			mountedPlayer.mount._frameExtra++;
			
			float frameSpeed = Math.Abs(velocity.X);
			mountedPlayer.mount._frameCounter += frameSpeed;
			
			if (mountedPlayer.mount._frameCounter > MountData.dashingFrameDelay)
			{
				mountedPlayer.mount._frameCounter -= MountData.dashingFrameDelay;
				mountedPlayer.mount._frame++;
			}
			
			if (mountedPlayer.mount._frame < MountData.dashingFrameStart || 
			    mountedPlayer.mount._frame >= MountData.dashingFrameStart + MountData.dashingFrameCount)
			{
				mountedPlayer.mount._frame = MountData.dashingFrameStart;
			}
			
			return false;
		}
		else if (state == 1)
		{
			mountedPlayer.mount._frameExtra = 0;
		}
		
		return true;
	}

	public override void UpdateEffects(Player player)
	{
		if (System.Math.Abs(player.velocity.X) > player.mount.DashSpeed - player.mount.RunSpeed / 2f)
		{
			player.noKnockback = true;
		}
	}
	}
}
