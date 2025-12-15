using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.ID;
using Drawbridge.Items.HorseArmor;

namespace Drawbridge.Systems;

public class EquineArmatureRenderSystem : ModSystem
{
    public override void PostDrawTiles()
    {
        // Start a fresh batch for world-space overlay drawing
        Main.spriteBatch.Begin(
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

                Point16 key = new(i, j);
                int overlayType = EquineArmatureWorld.GetType(key);
                if (overlayType == 0)
                    continue;

                Vector2 worldPos = new Vector2(i * 16, j * 16) - screenPos - new Vector2(4, 8);
                bool flipped = EquineArmatureWorld.GetFlipped(key);
                SpriteEffects fx = flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Texture2D overlay;

                if (ContentSamples.ItemsByType[overlayType].ModItem is HorseArmorItem horseArmor)
                    overlay = ModContent.Request<Texture2D>(horseArmor.OverlayPath).Value;
                else
                    return;

                if (!flipped)
                    worldPos.X -= 18;

                Vector2 pos = worldPos;

                // Apply lighting to the overlay, same as the mannequin base
                Color lightColor = Lighting.GetColor(i, j);
                Main.spriteBatch.Draw(overlay, pos, new Rectangle(0, 0, 126, 86), lightColor, 0f, Vector2.Zero, 1f, fx, 0f);
            }
        }

        Main.spriteBatch.End();
    }
}


