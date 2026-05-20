using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.DataStructures;
using Drawbridge.Projectiles;
using Terraria.ID;
using Drawbridge.Tiles.Drawbridges;

namespace Drawbridge.Systems;

public class AnimationPersistenceSystem : ModSystem
{
	private const string TagKey = "DrawbridgeAnimations";

	public override void SaveWorldData(TagCompound tag)
	{
		var list = new List<TagCompound>();
		foreach (Projectile proj in Main.projectile)
		{
			if (!proj.active || proj.ModProjectile == null)
				continue;
			if (proj.ModProjectile is BaseDrawbridgeAnimation || proj.ModProjectile is PortcullisAnimation || proj.ModProjectile is GoldBellClapper || proj.ModProjectile is GraveyardBellClapper || proj.ModProjectile is ClockHand)
			{
				int timerValue = 0;
				if (proj.ModProjectile is BaseDrawbridgeAnimation da)
					timerValue = da.AnimationTimer_IO;
				else if (proj.ModProjectile is PortcullisAnimation pa)
					timerValue = (int)pa.AnimationTimer;

				// For GoldBellClapper, timer is unused
				list.Add(new TagCompound
				{
					["projType"] = proj.type,
					["posX"] = proj.Center.X,
					["posY"] = proj.Center.Y,
					["ai0"] = proj.ai[0],
					["ai1"] = proj.ai[1],
					["timer"] = timerValue,
				});
			}
		}

		tag[TagKey] = list;
	}

	public override void LoadWorldData(TagCompound tag)
	{
		if (Main.netMode == NetmodeID.MultiplayerClient)
			return;

		if (tag.ContainsKey(TagKey))
		{
			var list = tag.GetList<TagCompound>(TagKey);
			foreach (TagCompound tc in list)
			{
				int type = tc.GetInt("projType");
				Vector2 pos = new Vector2(tc.GetFloat("posX"), tc.GetFloat("posY"));
				float ai0 = tc.GetFloat("ai0");
				float ai1 = tc.GetFloat("ai1");
				int timerValue = tc.GetInt("timer");

				var source = new EntitySource_Misc("AnimationPersistenceSystem.LoadWorldData");
				int index = Projectile.NewProjectile(source, pos, Vector2.Zero, type, 0, 0, Main.myPlayer, ai0, ai1);
				if (index >= 0 && index < Main.maxProjectiles && Main.projectile[index].active)
				{
					var mp = Main.projectile[index].ModProjectile;
					if (mp is BaseDrawbridgeAnimation da)
						da.AnimationTimer_IO = timerValue;
					else if (mp is PortcullisAnimation pa)
						pa.AnimationTimer = timerValue;
					// Check for GoldBellClapper
					else if (mp is GoldBellClapper)
					{
						// Calculate expected top-left tile position
						float bellBottomCenterX = pos.X;
						float bellBottomCenterY = pos.Y + 8f;
						int topLeftX = (int)((bellBottomCenterX - 24f) / 16f);
						int topLeftY = (int)((bellBottomCenterY - 48f) / 16f);
						// Check if the bell tile still exists
						Tile tile = Main.tile[topLeftX, topLeftY];
						if (!tile.HasTile || tile.TileType != ModContent.TileType<Tiles.GoldBellTile>())
						{
							Main.projectile[index].Kill();
						}
					}
					// Add GraveyardBellClapper handling
					else if (mp is GraveyardBellClapper)
					{
						// Calculate expected top-left tile position
						float bellBottomCenterX = pos.X;
						float bellBottomCenterY = pos.Y + 8f;
						int topLeftX = (int)((bellBottomCenterX - 24f) / 16f);
						int topLeftY = (int)((bellBottomCenterY - 48f) / 16f);
						// Check if the bell tile still exists
						Tile tile = Main.tile[topLeftX, topLeftY];
						if (!tile.HasTile || tile.TileType != ModContent.TileType<Tiles.GraveyardBellTile>())
						{
							Main.projectile[index].Kill();
						}
					}
					// Check for ClockHand
					else if (mp is ClockHand)
					{
						// Calculate expected top-left tile position for 5x5 clock
						float centerX = pos.X;
						float centerY = pos.Y;
						int topLeftX = (int)((centerX - 40f) / 16f);
						int topLeftY = (int)((centerY - 40f) / 16f);
						Tile tile = Main.tile[topLeftX, topLeftY];
						if (tile == null || !tile.HasTile || tile.TileType != ModContent.TileType<Tiles.ClockTile>())
						{
							Main.projectile[index].Kill();
						}
					}
				}
			}
		}

		// Ensure every placed clock has hands (spawn if missing)
		int ensureClockType = ModContent.TileType<Tiles.ClockTile>();
		var ensureSource = new EntitySource_Misc("AnimationPersistenceSystem.EnsureClockHands");
		for (int i = 0; i < Main.maxTilesX; i++)
		{
			for (int j = 0; j < Main.maxTilesY; j++)
			{
				Tile tile = Main.tile[i, j];
				if (tile == null || !tile.HasTile || tile.TileType != ensureClockType)
					continue;
				if (tile.TileFrameX != 0 || tile.TileFrameY != 0)
					continue; // only top-left
				float centerX = i * 16f + 40f;
				float centerY = j * 16f + 40f;
				Vector2 center = new Vector2(centerX, centerY);
				bool hasHand = false;
				for (int p = 0; p < Main.maxProjectiles; p++)
				{
					Projectile proj = Main.projectile[p];
					if (proj != null && proj.active && proj.type == ModContent.ProjectileType<ClockHand>())
					{
						if (Vector2.DistanceSquared(proj.Center, center) < 16 * 16)
						{
							hasHand = true;
							break;
						}
					}
				}

				if (!hasHand)
				{
					Projectile.NewProjectile(ensureSource, center, Vector2.Zero, ModContent.ProjectileType<ClockHand>(), 0, 0, Main.myPlayer, centerX, centerY);
				}
			}
		}
	}
}