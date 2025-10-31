using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace Drawbridge.Systems.Extensions;

internal static class TileExtensions
{
    public static Vector2 TileOffset => Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
    public static Vector2 DrawPosition(int i, int j, Vector2 off = default, bool noOffset = false) 
        => new Vector2(i, j) * 16 - Main.screenPosition + off + (noOffset ? Vector2.Zero : TileOffset);
    public static Vector2 DrawPosition(this ModTile tile, int i, int j, Vector2 off = default, bool noOffset = false) 
        => new Vector2(i, j) * 16 - Main.screenPosition + off + (noOffset ? Vector2.Zero : TileOffset);
    public static Vector2 DrawPosition(this GlobalTile tile, int i, int j, Vector2 off = default, bool noOffset = false) 
        => new Vector2(i, j) * 16 - Main.screenPosition + off + (noOffset ? Vector2.Zero : TileOffset);
}
