using Drawbridge.Systems;
using Terraria;

namespace Drawbridge.Common.Tiles;

internal interface IDrawbridgeWheels
{
    /// <summary>
    /// Draws drawbridge wheels. Can be used solely to obtain any one or more of the associated textures, using an invalid <paramref name="tile"/>.
    /// </summary>
    public bool DrawWheel(Tile tile, out string textureName, out string glowName, out WheelRenderSystem.DrawType drawType);
}
