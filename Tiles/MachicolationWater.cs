using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace Drawbridge.Tiles;

internal class MachicolationWater : ModProjectile
{
    public ref float Timer => ref Projectile.ai[0];

    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(12, 12);
        Projectile.friendly = true;
    }

    public override bool? CanCutTiles() => false;

    public override void AI()
    {
        Timer++;

        if (Timer > 14)
        {
            Projectile.velocity.X *= 0.95f;
            Projectile.velocity.Y += 0.2f;
            Projectile.scale *= 0.95f;
            Projectile.Opacity *= 0.9f;

            if (Projectile.Opacity < 0.05f)
            {
                Projectile.Kill();
            }
        }

        if (Main.rand.NextBool(20))
        {
            float x = Projectile.velocity.X * 0.3f + Main.rand.NextFloat(-1, 1);
            Dust.NewDustPerfect(Main.rand.NextVector2FromRectangle(Projectile.Hitbox), DustID.Smoke, new Vector2(x, Main.rand.NextFloat(-1, 0)), 150);
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        const float SineSpeed = 0.5f;

        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Vector2 scale = new(MathF.Sin(Timer * SineSpeed) * 0.5f + 1f, MathF.Cos(Timer * SineSpeed) * 0.5f + 1f);
        byte alpha = (byte)MathHelper.Lerp(120, 255, Projectile.Opacity);
        Color color = (lightColor * Projectile.Opacity) with { A = alpha };
        
        Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, tex.Size() / 2f, scale * Projectile.scale, SpriteEffects.None, 0);
        return false;
    }
}
