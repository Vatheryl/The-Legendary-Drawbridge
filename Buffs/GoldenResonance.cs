using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace Drawbridge.Buffs
{
    public class GoldenResonance : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            player.lifeRegen += 2; // +1 HP/sec
            player.moveSpeed += 0.10f;
            player.GetModPlayer<Players.DrawbridgePlayer>().hasGoldenResonance = true;
        }
    }
} 