using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace Drawbridge.Buffs
{
    public class SpiritVeil : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoTimeDisplay[Type] = false;
            Main.debuff[Type] = false;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (!Main.dayTime || player.ZoneGraveyard)
            {
                player.statDefense += 8;
            }
            player.buffImmune[BuffID.Cursed] = true;
            player.buffImmune[BuffID.Darkness] = true;
            player.GetModPlayer<Players.DrawbridgePlayer>().hasSpiritVeil = true;
        }
    }
} 