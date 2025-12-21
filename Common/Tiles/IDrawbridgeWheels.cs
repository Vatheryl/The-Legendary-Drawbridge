using Drawbridge.Systems;
using Terraria;

namespace Drawbridge.Common.Tiles;

internal interface IDrawbridgeWheels
{
    public bool DrawWheel(Tile tile, out string textureName, out string glowName, out WheelRenderSystem.DrawType drawType);
}
