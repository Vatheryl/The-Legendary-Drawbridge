using Terraria;
using Terraria.ObjectData;

// Copied & culled from the same implementation in Spirit Reforged
namespace Drawbridge.Common;

public static class TileExtensions
{
	/// <summary> Gets the top left tile in a multitile using the given coordinates. Useful for things like tile entities whos data is stored only in a single tile. <br/>
	/// This method relies on tileFrame to get the tile and may not work depending on how those variables are used. </summary>
	public static void GetTopLeft(ref int i, ref int j)
	{
		var tile = Framing.GetTileSafely(i, j);
		var data = TileObjectData.GetTileData(tile);

		if (data is null)
			return;

		(i, j) = (i - tile.TileFrameX % data.CoordinateFullWidth / 18, j - tile.TileFrameY % data.CoordinateFullHeight / 18);
	}
}