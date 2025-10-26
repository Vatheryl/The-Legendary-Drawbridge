using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace Drawbridge.Tiles
{
    public class BlackCulvertWall : ModWall
    {
        public override string Texture => "Terraria/Images/Wall_94";
        public override void SetStaticDefaults()
        {
            Main.wallHouse[Type] = true;
            DustType = DustID.Stone;
            AddMapEntry(new Color(50, 50, 50), CreateMapEntryName());
            // Use vanilla Wall_94 texture (Black Culvert)
            // No custom texture needed, tModLoader will use Wall_94 if no texture is provided
        }
    }
}