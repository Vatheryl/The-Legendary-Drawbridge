using Drawbridge.Mounts;
using Terraria;
using Terraria.ModLoader;

namespace Drawbridge.Buffs
{
	public class WightPalfreyBuff : ModBuff
	{
		public override void SetStaticDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.mount.SetMount(ModContent.MountType<WightPalfreyMount>(), player);
			player.buffTime[buffIndex] = 10;
		}
	}
}
