using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.DataStructures;
using Drawbridge.Tiles;

namespace Drawbridge.Projectiles;

public class HookProjectile : ModProjectile
{
    public override string Texture => "Drawbridge/Tiles/HookTile"; // Use the existing hook tile texture

    // Store the tile entity position that this projectile represents
    private int TileX => (int)Projectile.ai[0];
    private int TileY => (int)Projectile.ai[1];

    public override void SetDefaults()
    {
        Projectile.width = 32;
        Projectile.height = 32;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 36000; // Long-lived projectile
        Projectile.netImportant = true;
    }

    public override void AI()
    {
        // Keep the projectile alive and positioned at the hook tile
        Projectile.velocity = Vector2.Zero;
        Projectile.timeLeft = 2;
        
        // Position the projectile at the hook tile center
        Projectile.Center = new Vector2(TileX * 16 + 16, TileY * 16 + 16);

        // Check if the hook tile still exists
        if (!WorldGen.InWorld(TileX, TileY))
        {
            Projectile.Kill();
            return;
        }

        Tile tile = Main.tile[TileX, TileY];
        if (!tile.HasTile || tile.TileType != ModContent.TileType<HookTile>())
        {
            Projectile.Kill();
            return;
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        // Get the hook tile entity to check for stored fish
        if (!TileEntity.TryGet(TileX, TileY, out HookTileEntity hookTE))
            return false;

        Color baseLight = Lighting.GetColor(TileX, TileY);
        
        // Draw the hook itself first (so fish appear on top)
        Texture2D hookTexture = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 hookDrawPos = Projectile.Center - Main.screenPosition;
        Vector2 hookOrigin = new(hookTexture.Width * 0.5f, hookTexture.Height * 0.5f);
        Tile tile = Main.tile[TileX, TileY];

        if (tile.HasTile && tile.TileType == ModContent.TileType<HookTileNew>())
            hookDrawPos.X -= 9;
        
        Main.EntitySpriteDraw(
            hookTexture,
            hookDrawPos,
            null,
            baseLight,
            0f,
            hookOrigin,
            1f,
            SpriteEffects.None,
            0
        );

        // If there's a stored fish, draw it on top of the hook
        TryDrawFish(TileX, TileY, hookTE, tile);

        return false; // We handled all drawing ourselves
    }

    internal static void TryDrawFish(int x, int y, HookTileEntity hookTE, Tile tile, Vector2? offset = null)
    {
        if (hookTE.StoredItemType <= 0)
            return;

        Texture2D fishTexture = TextureAssets.Item[hookTE.StoredItemType].Value;

        // Position the fish at the bottom of the hook, 6px to the right, 4px lower
        Vector2 fishWorldPos = new Vector2(x * 16 + 16 - 4 + 6, y * 16 + 32 + 4) + offset ?? Vector2.Zero;
        Vector2 fishDrawPos = fishWorldPos - Main.screenPosition;
        Vector2 fishOrigin = new Vector2(fishTexture.Width * 0.5f, fishTexture.Height * 0.5f);
        float fishRotation = MathHelper.ToRadians(135f); // 135 degrees clockwise

        if (tile.HasTile && tile.TileType == ModContent.TileType<HookTileNew>())
            fishDrawPos.X -= 9;

        float fishScale = 1f;
        Color fishColor = Lighting.GetColor(x + 1, y + 1);

        Main.EntitySpriteDraw(fishTexture, fishDrawPos, null, fishColor, fishRotation, fishOrigin, fishScale, SpriteEffects.None, 0);
    }
}
