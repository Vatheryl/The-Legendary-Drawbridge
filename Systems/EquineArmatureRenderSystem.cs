using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;

namespace Drawbridge.Systems
{
    // Draw the IronFieldBarding overlay for EquineArmatureTile on top of tiles
    public class EquineArmatureRenderSystem : ModSystem
    {
        public override void PostDrawTiles()
        {
            SpriteBatch sb = Main.spriteBatch;
            // Start a fresh batch for world-space overlay drawing
            sb.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null,
                Main.GameViewMatrix.ZoomMatrix
            );

            // Choose overlay texture based on stored type
            Vector2 screenPos = Main.screenPosition;

            int iStart = (int)(screenPos.X / 16f) - 2;
            int iEnd = (int)((screenPos.X + Main.screenWidth) / 16f) + 2;
            int jStart = (int)(screenPos.Y / 16f) - 2;
            int jEnd = (int)((screenPos.Y + Main.screenHeight) / 16f) + 2;

            for (int i = iStart; i <= iEnd; i++)
            {
                for (int j = jStart; j <= jEnd; j++)
                {
                    if (!WorldGen.InWorld(i, j))
                        continue;
                    Tile tile = Main.tile[i, j];
                    if (tile == null || !tile.HasTile)
                        continue;
                    if (tile.TileType != ModContent.TileType<Tiles.EquineArmatureTile>())
                        continue;
                    // Only draw from the origin cell (top-left)
                    if (tile.TileFrameX != 0 || tile.TileFrameY != 0)
                        continue;

                    int left = i;
                    int top = j;
                    Point16 key = new Point16(left, top);
                    int overlayType = EquineArmatureWorld.GetType(key);
                    if (overlayType == 0)
                        continue;

                    // Move overlay 12 tiles up and left relative to the previously offset position
                    Vector2 worldPos = new Vector2(left * 16, top * 16) - screenPos;
                    bool flipped = EquineArmatureWorld.GetFlipped(key);
                    SpriteEffects fx = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    string overlayPath = overlayType == 5 ? "Drawbridge/Textures/SilverArmor_Overlay" : (overlayType == 4 ? "Drawbridge/Textures/CopperArmor_Overlay" : (overlayType == 3 ? "Drawbridge/Textures/MoltenArmor_Overlay" : (overlayType == 2 ? "Drawbridge/Textures/GoldArmor_Overlay" : "Drawbridge/Textures/IronFieldBardingOverlay")));
                    Texture2D overlay = ModContent.Request<Texture2D>(overlayPath).Value;

                    Vector2 pos = worldPos;
                    if (overlayType == 2)
                    {
                        // Gold overlay requires slight horizontal alignment adjustments
                        if (flipped)
                            pos.X += 14f; // facing left
                        else
                            pos.X -= 6f; // facing right (normal)
                            pos.Y += 2f;
                    }
                    else if (overlayType == 3)
                    {
                        // Molten overlay alignment (add to previous offsets):
                        // previous was: flipped +10 X, else -8 X; +2 Y
                        if (flipped)
                            pos.X += 14f; // 10 + 4
                        else
                            pos.X -= 4f;  // -8 + 4
                        pos.Y += 8f;     // 2 + 8 - 2 (moved 2px higher)
                    }
                    else if (overlayType == 4)
                    {
                        // Copper overlay alignment - adjusted positioning
                        if (flipped)
                        {
                            pos.X += 8f + 2f + 2f + 2f; // previous 8f + original adjustment 2f + previous adjustment 2f right + new adjustment 3f right
                            pos.Y += 1f - 2f + 1f; // previous 1f + original adjustment -2f + new adjustment 1f higher
                        }
                        else
                        {
                            pos.X += -2f + 2f; // previous -2f + adjustment 2f (no change)
                            pos.Y += 1f - 2f + 1f; // previous 1f + original adjustment -2f + new adjustment 1f higher
                        }
                    }
                    else if (overlayType == 5)
                    {
                        // Silver overlay alignment - adjusted positioning
                        if (flipped)
                        {
                            pos.X += 12f + 4f - 2f; // previous 12f + original adjustment 4f + adjustment -2f left
                            pos.Y += 1.5f + 3f + 2f - 1f; // previous 1.5f + original adjustment 3f + adjustment 2f lower + new adjustment -1f higher
                        }
                        else
                        {
                            pos.X += -4f + 4f; // previous -4f + adjustment 4f (no change)
                            pos.Y += 1.5f + 3f + 2f - 1f; // previous 1.5f + original adjustment 3f + adjustment 2f lower + new adjustment -1f higher
                        }
                    }

                    // Apply lighting to the overlay, same as the mannequin base
                    Color lightColor = Lighting.GetColor(left, top);
                    sb.Draw(overlay, pos, null, lightColor, 0f, Vector2.Zero, 1f, fx, 0f);
                }
            }

            sb.End();
        }
    }
}


