using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using System.IO;
 

namespace Drawbridge
{
	// Please read https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-Guide#mod-skeleton-contents for more information about the various files in a mod.
	public class Drawbridge : Mod
	{
		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			PacketHandler.HandlePacket(reader, whoAmI);
		}

        // Register HorseStableTile and HorseStableItem if needed (tModLoader usually auto-registers ModTile/ModItem)

        // Force-load our PlayerDrawLayer so tModLoader discovers it at runtime
        public override void Load() => _ = typeof(Systems.IronFieldBardingLayer);

        public override void Unload()
		{
		}
	}
}
