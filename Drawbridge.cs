global using Terraria.ModLoader;
global using ReLogic.Content;

using System.IO;

namespace Drawbridge;

public class Drawbridge : Mod
{
    public override void HandlePacket(BinaryReader reader, int whoAmI) => PacketHandler.HandlePacket(reader, whoAmI);

    // Force-load our PlayerDrawLayer so tModLoader discovers it at runtime
    public override void Load()
    {
        _ = typeof(Systems.IronFieldBardingLayer);

        NPCUtils.NPCUtils.TryLoadBestiaryHelper(this);
    }

    public override void Unload()
	{
	}
}
