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

	public class DrawbridgeConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ServerSide;

		[Label("Disable Easter Egg")]
		[Tooltip("")]
		[DefaultValue(false)]
		public bool DisableEasterEgg { get; set; } = true;

		[Label("Mute Clock Sound")]
		[Tooltip("If enabled, the clock will not play its hourly sound.")]
		[DefaultValue(false)]
		public bool MuteClockSound { get; set; } = false;

		[Label("Mute Stable Noises")]
		[Tooltip("If enabled, horse stables will not play their stable noises.")]
		[DefaultValue(false)]
		public bool MuteStableNoises { get; set; } = false;
		[Label("Disable Horse Stable Pooping")]
		[Tooltip("If enabled, horses in stables will not drop Poo.")]
		[DefaultValue(false)]
		public bool DisableHorseStablePooping { get; set; } = false;
	}
}
