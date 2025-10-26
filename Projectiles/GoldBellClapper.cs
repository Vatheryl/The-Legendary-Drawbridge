using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Drawbridge.Projectiles
{
    public class GoldBellClapper : ModProjectile
    {
        public override string Texture => "Drawbridge/Tiles/GoldBellClapper";

        private const int AnimationDuration = 60;
        private static readonly float MaxSwingAngle = MathHelper.ToRadians(30f);

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 32;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 36000; // Initial long duration
            Projectile.netImportant = true;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 2;

            if (Projectile.ai[0] == 1f)
            {
                Projectile.ai[0] = 0f;
                Projectile.ai[1] = 1f; // Start animation timer
                Projectile.netUpdate = true;
            }

            if (Projectile.ai[1] > 0f)
            {
                Projectile.ai[1] += 1f;
                if (Projectile.ai[1] >= AnimationDuration)
                {
                    Projectile.ai[1] = 0f;
                    Projectile.rotation = 0f;
                }
                else
                {
                    float progress = Projectile.ai[1] / AnimationDuration;
                    // Simple sine wave swing: start at 0, swing to max, back to -max/2, settle to 0
                    if (progress < 0.5f)
                    {
                        Projectile.rotation = MaxSwingAngle * (progress * 2f);
                    }
                    else
                    {
                        Projectile.rotation = MaxSwingAngle * (1f - (progress - 0.5f) * 2f) * 0.5f;
                    }
                    // Play sound at peak
                    if (Projectile.ai[1] == AnimationDuration / 2)
                    {
                        Terraria.Audio.SoundEngine.PlaySound(Terraria.ID.SoundID.Item29, Projectile.position); // Bell sound
                    }
                }
                Projectile.netUpdate = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle sourceRect = new Rectangle(0, 24, 16, 8); // Bottom 8 px
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + new Vector2(0, 8f); // Shift down to position bottom part correctly, adjusted 8px higher
            Vector2 origin = new Vector2(8f, 0f); // Center horizontally, top of partial sprite
            Color color = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f));
            Main.EntitySpriteDraw(texture, drawPosition, sourceRect, color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
} 