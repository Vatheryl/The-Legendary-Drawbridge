using Drawbridge.Items;
using Drawbridge.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Drawbridge.Projectiles;

public class LionDrawbridgeAnimation : BaseDrawbridgeAnimation
{
    public override string Texture => "Drawbridge/Tiles/Lion_Drawbridge";

    protected override DrawbridgeData SetDrawbridgeData()
        => DrawbridgeData.Generic<LionDrawbridgeTile, LionDrawbridgeVerticalFlippedTile, LionDrawbridgeHorizontalTile, LionDrawbridgeHorizontalFlippedTile, LionDrawbridgeItem>(
            originalColor => Color.Lerp(originalColor, new Color(255, 215, 0), 0.3f));

    protected override void DrawDrawbridgeExtras(Vector2 drawPos, Color baseLight, float rot, float scale)
    {
        string coverTexturePath = GetLionCoverTexture();

        if (!string.IsNullOrEmpty(coverTexturePath))
        {
            Texture2D coverTex = ModContent.Request<Texture2D>(coverTexturePath).Value;
            Vector2 coverOrigin = GetLionCoverOrigin(coverTex);
            Main.spriteBatch.Draw(coverTex, drawPos, null, baseLight, rot, coverOrigin, scale, SpriteEffects.None, 0);
        }
    }

    private string GetLionCoverTexture() => Flipped ? "Drawbridge/Tiles/Lion_DrawbridgeFlipped_cover" : "Drawbridge/Tiles/Lion_Drawbridge_cover";

    private Vector2 GetLionCoverOrigin(Texture2D coverTex)
    {
        float halfWidth = coverTex.Width / 2f;

        if (Flipped)
            return new Vector2(halfWidth - 16f, coverTex.Height); // Adjust for left hinge
        else
            return new Vector2(16f + halfWidth, coverTex.Height); // Adjust for right hinge
    }
}