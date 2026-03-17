using Drawbridge.Items;
using Drawbridge.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Drawbridge.Projectiles;

public class HellDrawbridgeAnimation : BaseDrawbridgeAnimation
{
    public override string Texture => "Drawbridge/Tiles/Hell_Drawbridge";

    protected override DrawbridgeData SetDrawbridgeData() 
        => DrawbridgeData.Generic<HellDrawbridgeTile, HellDrawbridgeVerticalFlippedTile, HellDrawbridgeHorizontalTile, HellDrawbridgeHorizontalFlippedTile, HellDrawbridgeItem>(Color.Lerp(baseLight, new Color(20, 40, 120), 0.8f));

    protected override void DrawDrawbridgeExtras(Vector2 drawPos, Color baseLight, float rot, float scale)
    {
        string coverTexturePath = GetHellCoverTexture();

        if (!string.IsNullOrEmpty(coverTexturePath))
        {
            Texture2D coverTex = ModContent.Request<Texture2D>(coverTexturePath).Value;
            Vector2 coverOrigin = GetHellCoverOrigin(coverTex);
            // Don't flip the cover texture - it's already oriented correctly
            Main.spriteBatch.Draw(coverTex, drawPos, null, baseLight, rot, coverOrigin, scale, SpriteEffects.None, 0);

            // Draw glow mask overlay
            string glowMaskTexturePath = GetHellGlowMaskTexture();

            if (!string.IsNullOrEmpty(glowMaskTexturePath))
            {
                Texture2D glowMaskTex = ModContent.Request<Texture2D>(glowMaskTexturePath).Value;
                // Use same origin, position, rotation, and scale as the cover with pulsing effect
                float glowPulse = 0.6f + 0.4f * (float)Math.Sin(Main.GameUpdateCount * 0.03f);
                Main.spriteBatch.Draw(glowMaskTex, drawPos, null, (Color.White * glowPulse) with { A = 0 }, rot, coverOrigin, scale, SpriteEffects.None, 0);
            }
        }
    }

    // Always use vertical covers during animation - only use horizontal when fully horizontal and stationary
    private string GetHellCoverTexture() => Flipped ? "Drawbridge/Tiles/Hell_DrawbridgeFlipped_cover" : "Drawbridge/Tiles/Hell_Drawbridge_cover";

    private string GetHellGlowMaskTexture() => Flipped ? "Drawbridge/Tiles/Hell_Drawbridge-Glow_MaskFlipped" : "Drawbridge/Tiles/Hell_Drawbridge-Glow_Mask";

    private Vector2 GetHellCoverOrigin(Texture2D coverTex)
    {
        // Adjust offsets for animation: non-flipped 6px left, flipped 6px right
        if (Flipped)
            return new Vector2(coverTex.Width - 46f - 4f, coverTex.Height); // Move 6px right by subtracting 6
        else
            return new Vector2(30f + 16f + 4f, coverTex.Height); // Move 6px left by adding 6 to origin
    }
}