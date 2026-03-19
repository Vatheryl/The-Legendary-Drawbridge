using Drawbridge.Systems;
using Microsoft.Xna.Framework;
using Terraria;

namespace Drawbridge.Common.Tiles;

internal interface IDrawbridgeChains
{
    public bool DrawChains(Tile tile, out WheelRenderSystem.DrawType drawType, ref Color chainColor);
}
