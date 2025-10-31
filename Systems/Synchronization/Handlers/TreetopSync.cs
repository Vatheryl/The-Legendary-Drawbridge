using Drawbridge.Systems.TreetopEyes;
using System.IO;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace Drawbridge.Systems.Synchronization.Handlers;

internal class TreetopSync : Handler
{
    // Used to ask for the treetop entities
    private class TreetopSyncPlayer : ModPlayer
    {
        public override void OnEnterWorld() => Send();
    }

    public override MessageType Message => MessageType.RequestTreetops;

    public static void Send()
    {
        var packet = PacketHandler.GetPacket(MessageType.RequestTreetops, 1);
        packet.Send();
    }

    public override void Recieve(int whoAmI, BinaryReader reader, bool onServer)
    {
        if (onServer)
        {
            var packet = PacketHandler.GetPacket(MessageType.RequestTreetops);
            packet.Write((short)TreetopEntity.Treetops.Count);
            
            foreach (var treetop in TreetopEntity.Treetops)
            {
                packet.Write(treetop.Key.X);
                packet.Write(treetop.Key.Y);
                packet.Write(treetop.Value.eyeCount);
                packet.Write(treetop.Value.variants);
            }

            packet.Send(whoAmI);
        }
        else
        {
            TreetopEntity.Treetops.Clear();

            short count = reader.ReadInt16();

            for (int i = 0; i < count; ++i)
            {
                short x = reader.ReadInt16();
                short y = reader.ReadInt16();
                byte eyeCount = reader.ReadByte();
                byte[] variants = reader.ReadBytes(TreetopEntity.Treetop.MaxEyes);

                TreetopEntity.Treetops.Add(new Point16(x, y), new TreetopEntity.Treetop()
                {
                    x = x,
                    y = y,
                    eyeCount = eyeCount,
                    variants = variants
                });
            }
        }
    }
}
