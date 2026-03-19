using Drawbridge.Systems;
using Microsoft.Xna.Framework;
using Terraria;

namespace Drawbridge.Common.Tiles;

#nullable enable

internal interface IDrawbridgeCover
{
    public bool CanDraw(Tile tile, out WheelRenderSystem.DrawType drawType, out string mainTex, out string glowTex, ref Vector2 offset);
}
