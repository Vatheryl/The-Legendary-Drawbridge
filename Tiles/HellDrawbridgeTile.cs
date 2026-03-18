using Drawbridge.Items;
using Drawbridge.Projectiles;
using Drawbridge.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Drawbridge.Tiles;

#nullable enable

public class HellDrawbridgeTile : BaseDrawbridgeTile
{
    public override string Texture => "Drawbridge/Tiles/Hell_Drawbridge";

    protected override int ProjectileId => ModContent.ProjectileType<HellDrawbridgeAnimation>();
    protected override int ItemId => ModContent.ItemType<HellDrawbridgeItem>();

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