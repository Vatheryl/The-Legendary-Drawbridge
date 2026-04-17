using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Drawbridge.Projectiles;

namespace Drawbridge.Tiles;

public class HookTileEntity : ModTileEntity
{
	public int StoredItemType;
	private int hookProjectileIndex = -1; // Track the associated projectile

    public override void SaveData(TagCompound tag) => tag[nameof(StoredItemType)] = StoredItemType;
    public override void LoadData(TagCompound tag) => StoredItemType = tag.GetInt(nameof(StoredItemType));

    public override void NetSend(BinaryWriter writer) => writer.Write(StoredItemType);
    public override void NetReceive(BinaryReader reader) => StoredItemType = reader.ReadInt32();

    public override void OnNetPlace() => SpawnHookProjectile();

    public override void Update()
	{
		// Ensure we have a hook projectile
		if (hookProjectileIndex == -1 || !Main.projectile[hookProjectileIndex].active || 
			Main.projectile[hookProjectileIndex].type != ModContent.ProjectileType<HookProjectile>())
		{
			SpawnHookProjectile();
		}
	}

	private void SpawnHookProjectile()
	{
		if (Main.netMode == NetmodeID.MultiplayerClient || Main.tile[Position].TileType != ModContent.TileType<HookTile>())
			return; // Only spawn on server or singleplayer

		// Kill any existing hook projectile
		if (hookProjectileIndex >= 0 && hookProjectileIndex < Main.maxProjectiles && Main.projectile[hookProjectileIndex].active)
		{
			Main.projectile[hookProjectileIndex].Kill();
		}

		// Spawn new hook projectile
		int projectileIndex = Projectile.NewProjectile(
			new EntitySource_TileEntity(this),
			Position.X * 16 + 16, // Center of the hook tile
			Position.Y * 16 + 16,
			0f, 0f, // No velocity
			ModContent.ProjectileType<HookProjectile>(),
			0, 0f, Main.myPlayer,
			Position.X, Position.Y // Pass tile coordinates in ai[0] and ai[1]
		);

		hookProjectileIndex = projectileIndex;
	}

	public override bool IsTileValidForEntity(int x, int y)
	{
		Tile tile = Main.tile[x, y];
		return tile.HasTile && (tile.TileType == ModContent.TileType<HookTile>() || tile.TileType == ModContent.TileType<HookTileNew>());
	}

	public override void OnKill()
	{
		// Kill the associated hook projectile
		if (Main.tile[Position].TileType == ModContent.TileType<HookTile>() && hookProjectileIndex >= 0 && hookProjectileIndex < Main.maxProjectiles)
		{
			if (Main.projectile[hookProjectileIndex].active)
				Main.projectile[hookProjectileIndex].Kill();
		}

		if (Main.netMode != NetmodeID.MultiplayerClient && StoredItemType != 0)
		{
			Item.NewItem(new EntitySource_TileBreak(Position.X, Position.Y), Position.X * 16, Position.Y * 16, 32, 32, StoredItemType);
			StoredItemType = 0;
		}
	}
}


