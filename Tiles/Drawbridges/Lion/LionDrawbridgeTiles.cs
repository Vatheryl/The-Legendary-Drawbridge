using Drawbridge.Items;
using Drawbridge.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace Drawbridge.Tiles.Drawbridges.Lion;

#nullable enable

public class LionDrawbridgeTile : BaseDrawbridgeTile
{
    protected override int ProjectileId => ModContent.ProjectileType<LionDrawbridgeAnimation>();
    protected override int ItemId => ModContent.ItemType<LionDrawbridgeItem>();

    public override void Load()
    {
        WheelRenderSystem.AddTexture("lionWheel", "Drawbridge/Tiles/Drawbridges/Lion/Lion_Wheel");
        WheelRenderSystem.AddTexture("lionWheelHorizontal", "Drawbridge/Tiles/Drawbridges/Lion/Lion_WheelHorizontal");
        WheelRenderSystem.AddTexture("lionCover", "Drawbridge/Tiles/Drawbridges/Lion/Lion_Drawbridge_cover");
        WheelRenderSystem.AddTexture("lionCoverFlipped", "Drawbridge/Tiles/Drawbridges/Lion/Lion_DrawbridgeFlipped_cover");
        WheelRenderSystem.AddTexture("lionCoverHorizontal", "Drawbridge/Tiles/Drawbridges/Lion/Lion_DrawbridgeHorizontal_cover");
        WheelRenderSystem.AddTexture("lionCoverHorizontalFlipped", "Drawbridge/Tiles/Drawbridges/Lion/Lion_DrawbridgeHorizontalFlipped_cover");
    }

    public override bool CanDraw(Tile tile, out WheelRenderSystem.DrawType drawType, out string mainTex, out string glowTex, ref Vector2 offset)
    {
        drawType = WheelRenderSystem.DrawType.Flipped;
        mainTex = "lionCover";
        glowTex = "";
        offset = new Vector2(-4, 0);
        return tile.TileFrameX == 16 && tile.TileFrameY == 224;
    }

    public override bool DrawWheel(Tile tile, out string textureName, out string glowName, out WheelRenderSystem.DrawType drawType)
    {
        drawType = WheelRenderSystem.DrawType.Default;
        textureName = "lionWheel";
        glowName = "";
        return tile.TileFrameX == 16 && tile.TileFrameY == 224;
    }
}

public class LionDrawbridgeVerticalFlippedTile : BaseDrawbridgeVerticalFlippedTile
{
    protected override int ProjectileId => ModContent.ProjectileType<LionDrawbridgeAnimation>();
    protected override int ItemId => ModContent.ItemType<LionDrawbridgeItem>();

    public override bool CanDraw(Tile tile, out WheelRenderSystem.DrawType drawType, out string mainTex, out string glowTex, ref Vector2 offset)
    {
        drawType = WheelRenderSystem.DrawType.Flipped;
        mainTex = "lionCoverFlipped";
        glowTex = "";
        offset = new Vector2(12, 0);
        return tile.TileFrameX == 0 && tile.TileFrameY == 224;
    }

    public override bool DrawWheel(Tile tile, out string textureName, out string glowName, out WheelRenderSystem.DrawType drawType)
    {
        drawType = WheelRenderSystem.DrawType.Flipped;
        textureName = "lionWheel";
        glowName = "";
        return tile.TileFrameX == 0 && tile.TileFrameY == 224;
    }
}

public class LionDrawbridgeHorizontalTile : BaseDrawbridgeHorizontalTile
{
    protected override int ProjectileId => ModContent.ProjectileType<LionDrawbridgeAnimation>();
    protected override int ItemId => ModContent.ItemType<LionDrawbridgeItem>();

    public override bool DrawChains(Tile tile, out WheelRenderSystem.DrawType drawType, ref Color chainColor)
    {
        drawType = WheelRenderSystem.DrawType.Default;
        return tile.TileFrameX == 224 && tile.TileFrameY == 16;
    }

    public override bool DrawWheel(Tile tile, out string textureName, out string glowName, out WheelRenderSystem.DrawType drawType)
    {
        drawType = WheelRenderSystem.DrawType.Default;
        textureName = "wheelH";
        glowName = "";
        return tile.TileFrameX == 224 && tile.TileFrameY == 16;
    }

    public override bool CanDraw(Tile tile, out WheelRenderSystem.DrawType drawType, out string mainTex, out string glowTex, ref Vector2 offset)
    {
        drawType = WheelRenderSystem.DrawType.Horizontal;
        mainTex = "lionCoverHorizontal";
        glowTex = "";
        offset = new Vector2(0, 4);
        return tile.TileFrameX == 224 && tile.TileFrameY == 16;
    }
}

public class LionDrawbridgeHorizontalFlippedTile : BaseDrawbridgeHorizontalFlippedTile
{
    protected override int ProjectileId => ModContent.ProjectileType<LionDrawbridgeAnimation>();
    protected override int ItemId => ModContent.ItemType<LionDrawbridgeItem>();

    public override bool DrawWheel(Tile tile, out string textureName, out string glowName, out WheelRenderSystem.DrawType drawType)
    {
        drawType = WheelRenderSystem.DrawType.Flipped;
        textureName = "lionWheelHorizontal";
        glowName = "";
        return tile.TileFrameX == 0 && tile.TileFrameY == 16;
    }

    public override bool DrawChains(Tile tile, out WheelRenderSystem.DrawType drawType, ref Color chainColor)
    {
        drawType = WheelRenderSystem.DrawType.Flipped;
        return tile.TileFrameX == 0 && tile.TileFrameY == 16;
    }

    public override bool CanDraw(Tile tile, out WheelRenderSystem.DrawType drawType, out string mainTex, out string glowTex, ref Vector2 offset)
    {
        drawType = WheelRenderSystem.DrawType.HorizontalFlipped;
        mainTex = "lionCoverHorizontalFlipped";
        glowTex = "";
        offset = new Vector2(0, 4);
        return tile.TileFrameX == 0 && tile.TileFrameY == 16;
    }
}

public class LionDrawbridgeAnimation : BaseDrawbridgeAnimation
{
    public override string Texture => "Drawbridge/Tiles/Drawbridges/Lion/LionDrawbridgeTile";

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

    private string GetLionCoverTexture() => Flipped ? "Drawbridge/Tiles/Drawbridges/Lion/Lion_DrawbridgeFlipped_cover" : "Drawbridge/Tiles/Drawbridges/Lion/Lion_Drawbridge_cover";

    private Vector2 GetLionCoverOrigin(Texture2D coverTex)
    {
        float halfWidth = coverTex.Width / 2f;

        if (Flipped)
            return new Vector2(halfWidth - 16f, coverTex.Height); // Adjust for left hinge
        else
            return new Vector2(16f + halfWidth, coverTex.Height); // Adjust for right hinge
    }
}