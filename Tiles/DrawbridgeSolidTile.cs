using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Tiles;

/// <summary>
/// Tile used solely to fake collision.
/// </summary>
public class DrawbridgeSolidTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = false;

        TileID.Sets.DrawsWalls[Type] = true;
    }

    // Ensure no item drops when this temporary tile is killed.
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => noItem = true;
}
 