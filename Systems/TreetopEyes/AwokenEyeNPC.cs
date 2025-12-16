using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Drawbridge.Systems.TreetopEyes;

internal class AwokenEyeNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    internal bool awoken = true;

    private bool _lastDayTime = false;

    public override bool IsLoadingEnabled(Mod mod) => false;
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type is NPCID.SleepyEye or NPCID.SleepyEye2;

    public override bool PreAI(NPC npc)
    {
        if (awoken)
        {
            _lastDayTime = Main.dayTime;
            Main.dayTime = false;
        }

        return true;
    }

    public override void PostAI(NPC npc)
    {
        if (awoken)
            Main.dayTime = _lastDayTime;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) => bitWriter.WriteBit(awoken);
    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader) => awoken = bitReader.ReadBit();
}
