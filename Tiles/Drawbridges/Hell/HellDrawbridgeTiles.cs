using Drawbridge.Items;
using Drawbridge.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace Drawbridge.Tiles.Drawbridges.Hell;

#nullable enable

public class HellDrawbridgeTile : BaseDrawbridgeTile
{
    protected override int ProjectileId => ModContent.ProjectileType<HellDrawbridgeAnimation>();
    protected override int ItemId => ModContent.ItemType<HellDrawbridgeItem>();

    public override void Load()
    {
        WheelRenderSystem.AddTexture("hellWheel", "Drawbridge/Tiles/Drawbridges/Hell/Hell_Wheel");
        WheelRenderSystem.AddTexture("hellWheelHorizontal", "Drawbridge/Tiles/Drawbridges/Hell/Hell_WheelHorizontalTile");
        WheelRenderSystem.AddTexture("hellGlowMask", "Drawbridge/Tiles/Drawbridges/Hell/Hell_Drawbridge-Glow_Mask");
        WheelRenderSystem.AddTexture("hellGlowMaskFlipped", "Drawbridge/Tiles/Drawbridges/Hell/Hell_Drawbridge-Glow_MaskFlipped");
        WheelRenderSystem.AddTexture("hellGlowMaskHorizontal", "Drawbridge/Tiles/Drawbridges/Hell/Hell_Drawbridge-Glow_MaskHorizontal");
        WheelRenderSystem.AddTexture("hellGlowMaskHorizontalFlipped", "Drawbridge/Tiles/Drawbridges/Hell/Hell_Drawbridge-Glow_MaskHorizontalFlipped");
        WheelRenderSystem.AddTexture("hellWheelGlowMask", "Drawbridge/Tiles/Drawbridges/Hell/Hell_Wheel-Glow_mask");
        WheelRenderSystem.AddTexture("hellWheelGlowMaskHorizontal", "Drawbridge/Tiles/Drawbridges/Hell/Hell_Wheel-Glow_maskHorizontal");
        WheelRenderSystem.AddTexture("hellCover", "Drawbridge/Tiles/Drawbridges/Hell/HellCover");
        WheelRenderSystem.AddTexture("hellFlippedCover", "Drawbridge/Tiles/Drawbridges/Hell/Hell_DrawbridgeFlipped_cover");
        WheelRenderSystem.AddTexture("hellHoriCover", "Drawbridge/Tiles/Drawbridges/Hell/Hell_DrawbridgeHorizontal_cover");
        WheelRenderSystem.AddTexture("hellHoriFlippedCover", "Drawbridge/Tiles/Drawbridges/Hell/Hell_DrawbridgeHorizontalFlipped_cover");
    }

    public override bool CanDraw(Tile tile, out WheelRenderSystem.DrawType drawType, out string mainTex, out string glowTex, ref Vector2 offset)
    {
        drawType = WheelRenderSystem.DrawType.Default;
        mainTex = "hellCover";
        glowTex = "hellGlowMask";
        return tile.TileFrameX == 16 && tile.TileFrameY == 224;
    }

    public override bool DrawWheel(Tile tile, out string textureName, out string glowName, out WheelRenderSystem.DrawType drawType)
    {
        drawType = WheelRenderSystem.DrawType.Default;
        textureName = "hellWheel";
        glowName = "hellWheelGlowMask";
        return tile.TileFrameX == 16 && tile.TileFrameY == 224;
    }
}

public class HellDrawbridgeVerticalFlippedTile : BaseDrawbridgeVerticalFlippedTile
{
    protected override int ProjectileId => ModContent.ProjectileType<HellDrawbridgeAnimation>();
    protected override int ItemId => ModContent.ItemType<HellDrawbridgeItem>();

    public override bool CanDraw(Tile tile, out WheelRenderSystem.DrawType drawType, out string mainTex, out string glowTex, ref Vector2 offset)
    {
        drawType = WheelRenderSystem.DrawType.Flipped;
        mainTex = "hellFlippedCover";
        glowTex = "hellGlowMaskFlipped";
        offset = new Vector2(8, 0);
        return tile.TileFrameX == 0 && tile.TileFrameY == 224;
    }

    public override bool DrawWheel(Tile tile, out string textureName, out string glowName, out WheelRenderSystem.DrawType drawType)
    {
        drawType = WheelRenderSystem.DrawType.Flipped;
        textureName = "hellWheel";
        glowName = "hellWheelGlowMask";
        return tile.TileFrameX == 0 && tile.TileFrameY == 224;
    }
}

public class HellDrawbridgeHorizontalTile : BaseDrawbridgeHorizontalTile
{
    protected override int ProjectileId => ModContent.ProjectileType<HellDrawbridgeAnimation>();
    protected override int ItemId => ModContent.ItemType<HellDrawbridgeItem>();

    public override bool DrawChains(Tile tile, out WheelRenderSystem.DrawType drawType, ref Color chainColor)
    {
        drawType = WheelRenderSystem.DrawType.Default;
        chainColor = Color.Lerp(chainColor, new Color(20, 40, 120), 0.8f);
        return tile.TileFrameX == 224 && tile.TileFrameY == 16;
    }

    public override bool DrawWheel(Tile tile, out string textureName, out string glowName, out WheelRenderSystem.DrawType drawType)
    {
        drawType = WheelRenderSystem.DrawType.Default;
        textureName = "hellWheelHorizontal";
        glowName = "hellWheelGlowMaskHorizontal";
        return tile.TileFrameX == 224 && tile.TileFrameY == 16;
    }

    public override bool CanDraw(Tile tile, out WheelRenderSystem.DrawType drawType, out string mainTex, out string glowTex, ref Vector2 offset)
    {
        drawType = WheelRenderSystem.DrawType.Horizontal;
        mainTex = "hellHoriCover";
        glowTex = "hellGlowMaskHorizontal";
        return tile.TileFrameX == 224 && tile.TileFrameY == 16;
    }
}

public class HellDrawbridgeHorizontalFlippedTile : BaseDrawbridgeHorizontalFlippedTile
{
    protected override int ProjectileId => ModContent.ProjectileType<HellDrawbridgeAnimation>();
    protected override int ItemId => ModContent.ItemType<HellDrawbridgeItem>();

    public override bool DrawWheel(Tile tile, out string textureName, out string glowName, out WheelRenderSystem.DrawType drawType)
    {
        drawType = WheelRenderSystem.DrawType.Flipped;
        textureName = "hellWheelHorizontal";
        glowName = "hellWheelGlowMaskHorizontal";
        return tile.TileFrameX == 0 && tile.TileFrameY == 16;
    }

    public override bool DrawChains(Tile tile, out WheelRenderSystem.DrawType drawType, ref Color chainColor)
    {
        drawType = WheelRenderSystem.DrawType.Flipped;
        chainColor = Color.Lerp(chainColor, new Color(20, 40, 120), 0.8f);
        return tile.TileFrameX == 0 && tile.TileFrameY == 16;
    }

    public override bool CanDraw(Tile tile, out WheelRenderSystem.DrawType drawType, out string mainTex, out string glowTex, ref Vector2 offset)
    {
        drawType = WheelRenderSystem.DrawType.HorizontalFlipped;
        mainTex = "hellHoriFlippedCover";
        glowTex = "hellGlowMaskHorizontalFlipped";
        return tile.TileFrameX == 0 && tile.TileFrameY == 16;
    }
}

public class HellDrawbridgeAnimation : BaseDrawbridgeAnimation
{
    const string PathStart = "Drawbridge/Tiles/Drawbridges/Hell/";

    public override string Texture => PathStart + "HellDrawbridgeTile";

    protected override DrawbridgeData SetDrawbridgeData()
        => DrawbridgeData.Generic<HellDrawbridgeTile, HellDrawbridgeVerticalFlippedTile, HellDrawbridgeHorizontalTile, HellDrawbridgeHorizontalFlippedTile, HellDrawbridgeItem>(
            (originalColor) => Color.Lerp(originalColor, new Color(20, 40, 120), 0.8f));

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
    private string GetHellCoverTexture() => PathStart + (Flipped ? "Hell_DrawbridgeFlipped_cover" : "HellCover");

    private string GetHellGlowMaskTexture() => PathStart + (Flipped ? "Hell_Drawbridge-Glow_MaskFlipped" : "Hell_Drawbridge-Glow_Mask");

    private Vector2 GetHellCoverOrigin(Texture2D coverTex)
    {
        // Adjust offsets for animation: non-flipped 6px left, flipped 6px right
        if (Flipped)
            return new Vector2(coverTex.Width - 46f - 4f, coverTex.Height); // Move 6px right by subtracting 6
        else
            return new Vector2(30f + 16f + 4f, coverTex.Height); // Move 6px left by adding 6 to origin
    }
}