using Terraria;
using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
	public class DrawbridgeSolidTile : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = false;
			// No texture, no map entry, no item drop. Purely for collision.
		}

		// Ensure no item drops when this temporary tile is killed.
		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			noItem = true;
		}
	}
} 