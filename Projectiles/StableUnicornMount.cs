using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.DataStructures;
using System.Collections.Generic;
using Drawbridge;

namespace Drawbridge.Projectiles
{
    public class StableUnicornMount : ModProjectile
    {
        public override string Texture => "Terraria/Images/Mount_Unicorn";

        private const int TotalFrames = 16;
        private const int RunFrames = 9; // 0-8
        private const int FrameDelay = 12;
        private const float MoveSpeed = 0.25f; // even slower
        private const float StableWidth = 138f;
        private const float StableHeight = 84f;
        private const float EdgeOffset = 32f; // Distance from stable edge to unicorn center

        // localAI[0]: 0 = stationary left, 1 = moving right, 2 = stationary right, 3 = moving left
        // localAI[2]: 0 = facing right, 1 = facing left
        // localAI[3]: stable center X

        // Horse types
        private enum HorseType
        {
            Unicorn = 0,
            Painted = 1,
            Majestic = 2,
            Dark = 3
        }

        private float roamingWidth = StableWidth;
        private bool randomRoam = false;
        private float leftEdge;
        private float rightEdge;
        private float nextTargetX;
        private int pauseTicks = 0;
        private static readonly int PauseMin = 60; // 1s
        private static readonly int PauseMax = 180; // 3s

        // Static variable to track last known day/night state for the world
        private static float LastWorldDayTime = -1f;
        
        public override void SetDefaults()
        {
            Projectile.width = 96;
            Projectile.height = 64;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.netImportant = true;
            Projectile.hide = true; // This is important!
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            // Add this projectile to the list that draws behind NPCs and tiles
            behindNPCsAndTiles.Add(index);
        }

        public override void OnSpawn(IEntitySource source)
        {
            // Ensure localAI has enough slots
            if (Projectile.localAI.Length < 6)
            {
                float[] newLocalAI = new float[6];
                for (int i = 0; i < Projectile.localAI.Length; i++)
                    newLocalAI[i] = Projectile.localAI[i];
                Projectile.localAI = newLocalAI;
            }
            float stableCenterX = Projectile.ai[1];
            int horseTypeRaw = (int)Projectile.ai[2];
            if (horseTypeRaw >= 100)
            {
                randomRoam = true;
                horseTypeRaw -= 100;
                roamingWidth = Projectile.ai[0];
            }
            else
            {
                roamingWidth = StableWidth;
                randomRoam = false;
            }
            float leftX = stableCenterX - (roamingWidth / 2f) + EdgeOffset;
            float rightX = stableCenterX + (roamingWidth / 2f) - EdgeOffset;
            leftEdge = leftX;
            rightEdge = rightX;
            // Start at the actual spawn X (Projectile.Center.X), not always at leftEdge
            nextTargetX = Main.rand.NextFloat(leftEdge, rightEdge);
            Projectile.localAI[0] = 0f; // stationary left
            Projectile.localAI[2] = 0f; // facing right
            Projectile.localAI[3] = Main.dayTime ? 1f : 0f; // Initialize last known dayTime state
            // Assign a random poop time for this day (in ticks)
            Projectile.localAI[4] = 0f; // Not used anymore
            Projectile.localAI[5] = 0f; // Reset poop flag
            Projectile.ai[1] = stableCenterX;
            Projectile.ai[2] = horseTypeRaw;
        }

        public override void OnKill(int timeLeft)
        {
        }

        public override void AI()
        {
            // Ensure localAI has enough slots
            if (Projectile.localAI.Length < 6)
            {
                float[] newLocalAI = new float[6];
                for (int i = 0; i < Projectile.localAI.Length; i++)
                    newLocalAI[i] = Projectile.localAI[i];
                Projectile.localAI = newLocalAI;
            }
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 180;
            float y = Projectile.Center.Y;

            // --- BEGIN: Poop at world day/night transition logic ---
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (Terraria.ModLoader.ModContent.GetInstance<DrawbridgeConfig>().DisableHorseStablePooping)
                    return;
                float currentDayTime = Main.dayTime ? 1f : 0f;
                if (LastWorldDayTime != currentDayTime)
                {
                    // World day/night transition detected
                    LastWorldDayTime = currentDayTime;
                    // Loop through all projectiles and make all unicorn stables poop
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile proj = Main.projectile[i];
                        if (proj.active && proj.ModProjectile is StableUnicornMount)
                        {
                            StableUnicornMount mount = (StableUnicornMount)proj.ModProjectile;
                            mount.Poop();
                        }
                    }
                }
            }
            // --- END: Poop at world day/night transition logic ---

            if (randomRoam)
            {
                // Random roaming logic
                if (pauseTicks > 0)
                {
                    pauseTicks--;
                    Projectile.frame = 0;
                    return;
                }
                if (System.Math.Abs(Projectile.Center.X - nextTargetX) < 1f)
                {
                    // Arrived at target, pause
                    pauseTicks = Main.rand.Next(PauseMin, PauseMax);
                    // Pick new target
                    float minX = leftEdge;
                    float maxX = rightEdge;
                    nextTargetX = Main.rand.NextFloat(minX, maxX);
                    // Flip direction
                    Projectile.localAI[2] = (Projectile.Center.X < nextTargetX) ? 0f : 1f;
                    Projectile.localAI[0] = (Projectile.Center.X < nextTargetX) ? 1f : 3f;
                    return;
                }
                // Move towards target
                float moveDir = (nextTargetX > Projectile.Center.X) ? 1f : -1f;
                Projectile.localAI[2] = (moveDir > 0) ? 0f : 1f;
                Projectile.localAI[1]++;
                if (Projectile.localAI[1] >= FrameDelay)
                {
                    Projectile.localAI[1] = 0;
                    Projectile.frame++;
                    if (Projectile.frame >= RunFrames)
                        Projectile.frame = 0;
                }
                float newX = Projectile.Center.X + moveDir * MoveSpeed;
                if ((moveDir > 0 && newX > nextTargetX) || (moveDir < 0 && newX < nextTargetX))
                    newX = nextTargetX;
                Projectile.Center = new Vector2(newX, y);
                return;
            }
            // Movement state machine
            if (Projectile.localAI[0] == 0f) // stationary left
            {
                Projectile.Center = new Vector2(leftEdge + 10f, y);
                Projectile.localAI[1] = 0;
                Projectile.frame = 0;
            }
            else if (Projectile.localAI[0] == 1f) // moving right
            {
                Projectile.localAI[2] = 0f; // facing right
                Projectile.localAI[1]++;
                if (Projectile.localAI[1] >= FrameDelay)
                {
                    Projectile.localAI[1] = 0;
                    Projectile.frame++;
                    if (Projectile.frame >= RunFrames)
                        Projectile.frame = 0;
                }
                float newX = Projectile.Center.X + MoveSpeed;
                if (newX >= rightEdge)
                {
                    Projectile.Center = new Vector2(rightEdge, y);
                    Projectile.localAI[0] = 2f; // stationary right
                    Projectile.localAI[2] = 1f; // flip to face left immediately
                    Projectile.frame = 0;
                }
                else
                {
                    Projectile.Center = new Vector2(newX, y);
                }
            }
            else if (Projectile.localAI[0] == 2f) // stationary right
            {
                Projectile.Center = new Vector2(rightEdge - 10f, y);
                Projectile.localAI[1] = 0;
                Projectile.frame = 0;
            }
            else if (Projectile.localAI[0] == 3f) // moving left
            {
                Projectile.localAI[2] = 1f; // facing left
                Projectile.localAI[1]++;
                if (Projectile.localAI[1] >= FrameDelay)
                {
                    Projectile.localAI[1] = 0;
                    Projectile.frame++;
                    if (Projectile.frame >= RunFrames)
                        Projectile.frame = 0;
                }
                float newX = Projectile.Center.X - MoveSpeed;
                if (newX <= leftEdge)
                {
                    Projectile.Center = new Vector2(leftEdge, y);
                    Projectile.localAI[0] = 0f; // stationary left
                    Projectile.localAI[2] = 0f; // flip to face right immediately
                    Projectile.frame = 0;
                }
                else
                {
                    Projectile.Center = new Vector2(newX, y);
                }
            }
        }

        public void TriggerMove()
        {
            // Called by the stable tile on right-click
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.localAI[0] = 1f; // move right
                Projectile.frame = 1; // start with frame 1 when moving
            }
            else if (Projectile.localAI[0] == 2f)
            {
                Projectile.localAI[0] = 3f; // move left
                Projectile.frame = 1; // start with frame 1 when moving
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            HorseType horseType = (HorseType)(Projectile.ai.Length > 2 ? (int)Projectile.ai[2] : 0);
            Texture2D texture;
            int totalFrames = TotalFrames;
            switch (horseType)
            {
                case HorseType.Painted:
                    texture = Terraria.GameContent.TextureAssets.Extra[161].Value;
                    break;
                case HorseType.Majestic:
                    texture = Terraria.GameContent.TextureAssets.Extra[162].Value;
                    break;
                case HorseType.Dark:
                    texture = Terraria.GameContent.TextureAssets.Extra[163].Value;
                    break;
                default:
                    texture = Terraria.GameContent.TextureAssets.UnicornMount.Value;
                    break;
            }
            int frameHeight = texture.Height / totalFrames;
            Rectangle sourceRect = new Rectangle(0, frameHeight * Projectile.frame, texture.Width, frameHeight);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight - 8f);
            SpriteEffects flip = Projectile.localAI[2] == 1f ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            // Use Terraria's lighting at the projectile's position
            Color drawColor = Lighting.GetColor((int)(Projectile.Center.X / 16f), (int)(Projectile.Center.Y / 16f));
            Main.spriteBatch.Draw(texture, drawPos, sourceRect, drawColor, 0f, origin, 1f, flip, 0f);
            return false;
        }

        // Helper method to drop poop
        public void Poop()
        {
            float horizontalOffset = 32f;
            float direction = (Projectile.localAI[2] == 1f) ? 1f : -1f;
            Vector2 dropPos = Projectile.Center + new Vector2(horizontalOffset * direction, Projectile.height / 2f - 36f);
            int itemType = 5395; // Poo
            int itemIndex = Item.NewItem(Projectile.GetSource_FromAI(), (int)dropPos.X, (int)dropPos.Y, 16, 16, itemType, 1, false, 0, true);
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item16, dropPos);
            if (Main.netMode == NetmodeID.Server && itemIndex >= 0 && itemIndex < 400)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex, 1f);
            }
        }
    }
}