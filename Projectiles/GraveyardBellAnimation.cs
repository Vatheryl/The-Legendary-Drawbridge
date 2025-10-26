using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Projectiles
{
    public class GraveyardBellAnimation : ModProjectile
    {
        private const int AnimationDuration = 300;
        private const int StillDuration = 30;
        private static readonly float MaxBellSwing = MathHelper.ToRadians(10f);
        private static readonly float MaxClapperSwing = MathHelper.ToRadians(30f);

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = AnimationDuration + StillDuration + 5;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 2;

            if (Projectile.ai[0] < AnimationDuration + StillDuration)
            {
                Projectile.ai[0] += 1f;
                Projectile.netUpdate = true;
            }
            else
            {
                Projectile.Kill();
            }

            if (Projectile.ai[0] == AnimationDuration)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int topLeftX = (int)Projectile.ai[1];
                    int topLeftY = (int)Projectile.ai[2];
                    int originX = topLeftX + 1;
                    int originY = topLeftY + 1;
                    WorldGen.PlaceObject(originX, originY, ModContent.TileType<Tiles.GraveyardBellTile>(), true);
                    if (Main.netMode == NetmodeID.Server)
                    {
                        NetMessage.SendTileSquare(-1, topLeftX, topLeftY, 3, 3);
                    }
                    float bellBottomCenterX = topLeftX * 16f + 24f;
                    float bellBottomCenterY = topLeftY * 16f + 48f;
                    float clapperCenterY = bellBottomCenterY - 8f;
                    Vector2 clapperCenter = new Vector2(bellBottomCenterX, clapperCenterY);
                    int clapperIndex = Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        clapperCenter,
                        Vector2.Zero,
                        ModContent.ProjectileType<GraveyardBellClapper>(),
                        0,
                        0,
                        Projectile.owner
                    );
                    if (Main.netMode == NetmodeID.Server && clapperIndex >= 0)
                    {
                        NetMessage.SendData(Terraria.ID.MessageID.SyncProjectile, -1, -1, null, clapperIndex);
                    }
                }
            }

            // Play sound immediately at start, but only once per client
            if (Projectile.localAI[0] == 0f)
            {
                if (Main.netMode != NetmodeID.Server)
                {
                    SoundEngine.PlaySound(new SoundStyle("Drawbridge/Sounds/GraveyardBellTone") { Volume = 1f }, Projectile.position);
                }
                int buffType = ModContent.BuffType<Buffs.SpiritVeil>();
                int buffDuration = 36000;
                float radius = 1000f;
                for (int i = 0; i < Main.maxPlayers; i++)
                {
                    Player player = Main.player[i];
                    if (player.active && !player.dead && Vector2.DistanceSquared(player.Center, Projectile.Center) < radius * radius)
                    {
                        player.AddBuff(buffType, buffDuration);
                    }
                }
                Projectile.localAI[0] = 1f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float progress = Projectile.ai[0] / AnimationDuration;
            float bellRotation = 0f;
            float clapperRotation = 0f;
            if (Projectile.ai[0] < AnimationDuration)
            {
                float damping = (float)System.Math.Exp(-1.5f * progress);
                float omega0 = MathHelper.TwoPi * 3f;
                float phase = omega0 * (progress - (progress * progress) / 2f);
                bellRotation = MaxBellSwing * damping * (float)System.Math.Sin(phase);
                float clapperPhase = omega0 * ((progress + 0.1f) - ((progress + 0.1f) * (progress + 0.1f) / 2f));
                clapperRotation = MaxClapperSwing * damping * (float)System.Math.Sin(clapperPhase);
            }

            Texture2D bellTexture = ModContent.Request<Texture2D>("Drawbridge/Tiles/GraveyardBellTile").Value;
            Texture2D clapperTexture = ModContent.Request<Texture2D>("Drawbridge/Tiles/GraveyardBellClapper").Value;

            Vector2 attachmentPoint = Projectile.Center - new Vector2(0, 24f);

            Vector2 clapperOffset = new Vector2(0, 24f).RotatedBy(bellRotation);
            Vector2 clapperDrawPos = attachmentPoint + clapperOffset - Main.screenPosition;
            Vector2 clapperOrigin = new Vector2(clapperTexture.Width / 2f, 0f);
            Main.EntitySpriteDraw(clapperTexture, clapperDrawPos, null, lightColor, bellRotation + clapperRotation, clapperOrigin, 1f, SpriteEffects.None, 0);

            Vector2 bellDrawPos = attachmentPoint - Main.screenPosition;
            Vector2 bellOrigin = new Vector2(bellTexture.Width / 2f, 0f);
            Main.EntitySpriteDraw(bellTexture, bellDrawPos, null, lightColor, bellRotation, bellOrigin, 1f, SpriteEffects.None, 0);

            return false;
        }
    }
} 