using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using System;

namespace Drawbridge.Projectiles
{
    public class ClockHand : ModProjectile
    {
        // Private fields for animation state (not persisted)
        private float moveProgress = 1f; // 1 = idle
        private float shakeTimer = -1f; // -1 = not shaking

        public override string Texture => "Drawbridge/Tiles/ClockHand";

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 24;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 36000;
            Projectile.netImportant = true;
            Projectile.netUpdate = true;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 2;

            // ai[0] = anchor X (in world px), ai[1] = anchor Y (in world px)
            float anchorX = Projectile.ai[0];
            float anchorY = Projectile.ai[1];
            Projectile.Center = new Vector2(anchorX, anchorY);

            // Map Terraria time to analog clock face
            double analogHour;
            if (Main.dayTime) {
                // Day: 0..54000 ticks, 4.5..19.5 hours
                analogHour = 4.5 + (Main.time / 54000.0) * 15.0;
            } else {
                // Night: 0..32400 ticks, 19.5..28.5 (wraps to 4.5)
                analogHour = 19.5 + (Main.time / 32400.0) * 9.0;
                if (analogHour >= 24.0) analogHour -= 24.0;
            }
            
            // Calculate hour hand angle (discrete jumps)
            int hourInt = (int)Math.Floor(analogHour + 3.0) % 12;
            float targetAngle = (float)(MathHelper.TwoPi * (hourInt / 12.0) - MathHelper.PiOver2);
            
            // Calculate minute hand angle (smooth movement)
            // Extract the fractional part of the hour for minutes (without the +3.0 offset)
            double fractionalHour = analogHour % 1.0; // Get minutes within current hour
            float minuteAngle = (float)(MathHelper.TwoPi * fractionalHour);

            // Store minute hand angle for smooth rendering (no animation needed)
            Projectile.localAI[2] = minuteAngle;

            // --- Movement and shake state for hour hand ---
            float currentAngle = Projectile.localAI[0];
            float lastTargetAngle = Projectile.localAI[1];
            const float moveDuration = 30f; // frames for movement (0.5s)
            const float shakeDuration = 18f; // frames for shake (0.3s)
            const float shakeMagnitude = 0.08f; // radians (about 4.5 degrees)

            // Detect hour change
            if (lastTargetAngle != targetAngle)
            {
                Projectile.localAI[1] = targetAngle;
                moveProgress = 0f; // reset progress
                shakeTimer = -1f; // reset shake
                // Start from current angle
                Projectile.localAI[0] = currentAngle;
            }

            // If not at target, animate with ease-in
            if (moveProgress < 1f)
            {
                moveProgress += 1f / moveDuration;
                if (moveProgress > 1f) moveProgress = 1f;
                // Ease-in: cubic (t^3)
                float t = moveProgress;
                float ease = t * t * t;
                float angleDiff = MathHelper.WrapAngle(targetAngle - currentAngle);
                float newAngle = MathHelper.WrapAngle(currentAngle + angleDiff * ease);
                Projectile.localAI[0] = newAngle;
                Projectile.rotation = newAngle;
                if (moveProgress >= 1f)
                {
                    shakeTimer = 0f; // start shake next frame
                }
            }
            else if (shakeTimer >= 0f) // shake phase
            {
                shakeTimer += 1f;
                float shakeProgress = shakeTimer / shakeDuration;
                float shake = (float)Math.Sin(shakeProgress * MathHelper.TwoPi * 2) * (1f - shakeProgress) * shakeMagnitude;
                Projectile.rotation = MathHelper.WrapAngle(targetAngle + shake);
                if (shakeTimer > shakeDuration)
                {
                    shakeTimer = -1f; // done shaking
                    Projectile.rotation = targetAngle;
                    Projectile.localAI[0] = targetAngle;
                }
            }
            else // idle
            {
                Projectile.rotation = targetAngle;
                Projectile.localAI[0] = targetAngle;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Color lightingColor = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f));
            
            // Draw minute hand (ClockHand.png, 8x24, rotates smoothly)
            Texture2D minuteTexture = ModContent.Request<Texture2D>("Drawbridge/Tiles/ClockHand").Value;
            Vector2 minuteOrigin = new Vector2(minuteTexture.Width / 2f, minuteTexture.Height); // bottom center
            Main.EntitySpriteDraw(
                minuteTexture,
                Projectile.Center - Main.screenPosition,
                null,
                lightingColor,
                Projectile.localAI[2], // smooth minute hand rotation
                minuteOrigin,
                1f,
                SpriteEffects.None,
                0
            );

            // Draw hour hand (ClockHandHour.png, 8x16, rotates with animation and shake)
            Texture2D hourTexture = ModContent.Request<Texture2D>("Drawbridge/Tiles/ClockHandHour").Value;
            Vector2 hourOrigin = new Vector2(hourTexture.Width / 2f, hourTexture.Height); // bottom center
            Main.EntitySpriteDraw(
                hourTexture,
                Projectile.Center - Main.screenPosition,
                null,
                lightingColor,
                Projectile.rotation, // animated hour hand rotation
                hourOrigin,
                1f,
                SpriteEffects.None,
                0
            );
            return false;
        }
    }
} 