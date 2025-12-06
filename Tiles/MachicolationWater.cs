using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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

        if (Timer > 8)
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

        if (Main.rand.NextBool(30))
        {
            float x = Projectile.velocity.X * 0.3f + Main.rand.NextFloat(-1, 1);
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, x, Main.rand.NextFloat(-1, 0), 220);
        }
    }
}
