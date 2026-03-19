using Drawbridge.Items;
using Drawbridge.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Drawbridge.Tiles.Drawbridges;

public class DrawbridgeTile : BaseDrawbridgeTile
{
    protected override int ProjectileId => ModContent.ProjectileType<DrawbridgeAnimation>();
    protected override int ItemId => ModContent.ItemType<DrawbridgeItem>();

    public override bool DrawWheel(Tile tile, out string textureName, out string glowName, out WheelRenderSystem.DrawType drawType)
    {
        drawType = WheelRenderSystem.DrawType.Default;
        textureName = "wheelV";
        glowName = "";
        return tile.TileFrameX == 16 && tile.TileFrameY == 224;
    }
}

public class DrawbridgeVerticalFlippedTile : BaseDrawbridgeVerticalFlippedTile
{
    protected override int ProjectileId => ModContent.ProjectileType<DrawbridgeAnimation>();
    protected override int ItemId => ModContent.ItemType<DrawbridgeItem>();

    public override bool DrawWheel(Tile tile, out string textureName, out string glowName, out WheelRenderSystem.DrawType drawType)
    {
        drawType = WheelRenderSystem.DrawType.Flipped;
        textureName = "wheelV";
        glowName = "";
        return tile.TileFrameX == 0 && tile.TileFrameY == 224;
    }
}

public class DrawbridgeHorizontalTile : BaseDrawbridgeHorizontalTile
{
    protected override int ProjectileId => ModContent.ProjectileType<DrawbridgeAnimation>();
    protected override int ItemId => ModContent.ItemType<DrawbridgeItem>();

    public override bool DrawChains(Tile tile, out WheelRenderSystem.DrawType drawType, ref Color chainColor)
    {
        drawType = WheelRenderSystem.DrawType.Default;
        return tile.TileFrameX == 224 && tile.TileFrameY == 16;
    }

    public override bool DrawWheel(Tile tile, out string textureName, out string glowName, out WheelRenderSystem.DrawType drawType)
    {
        drawType = WheelRenderSystem.DrawType.Default;
        textureName = "wheelV";
        glowName = "";
        return tile.TileFrameX == 224 && tile.TileFrameY == 16;
    }
}

public class DrawbridgeHorizontalFlippedTile : BaseDrawbridgeHorizontalFlippedTile
{
    protected override int ProjectileId => ModContent.ProjectileType<DrawbridgeAnimation>();
    protected override int ItemId => ModContent.ItemType<DrawbridgeItem>();

    public override bool DrawWheel(Tile tile, out string textureName, out string glowName, out WheelRenderSystem.DrawType drawType)
    {
        drawType = WheelRenderSystem.DrawType.Flipped;
        textureName = "wheelV";
        glowName = "";
        return tile.TileFrameX == 0 && tile.TileFrameY == 16;
    }

    public override bool DrawChains(Tile tile, out WheelRenderSystem.DrawType drawType, ref Color chainColor)
    {
        drawType = WheelRenderSystem.DrawType.Flipped;
        return tile.TileFrameX == 0 && tile.TileFrameY == 16;
    }
}

public class DrawbridgeAnimation : BaseDrawbridgeAnimation
{
    public override string Texture => "Drawbridge/Tiles/Drawbridges/DrawbridgeTile";

    protected override DrawbridgeData SetDrawbridgeData()
        => DrawbridgeData.Generic<DrawbridgeTile, DrawbridgeVerticalFlippedTile, DrawbridgeHorizontalTile, DrawbridgeHorizontalFlippedTile, DrawbridgeItem>(null);
}