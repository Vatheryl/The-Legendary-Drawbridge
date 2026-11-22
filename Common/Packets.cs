using Drawbridge.Systems.Synchronization;
using System;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;

namespace Drawbridge.Common;

internal class SendTripWireHandler : Handler
{
    public override MessageType Message => MessageType.SendTripWire;

    public static void Send(int x, int y, int width, int height)
    {
        ModPacket packet = PacketHandler.GetPacket(MessageType.SendTripWire);
        packet.Write((ushort)x);
        packet.Write((ushort)y);
        packet.Write(checked((byte)width));
        packet.Write(checked((byte)height));
        packet.Send();
    }

    public override void Recieve(int whoAmI, BinaryReader reader, bool onServer)
    {
        int x = reader.ReadUInt16();
        int y = reader.ReadUInt16();
        int width = reader.ReadByte();
        int height = reader.ReadByte();

        Wiring.TripWire(x, y, width, height);
    }
}

[Obsolete("This doesn't work stably.")]
internal class ForceSyncTEHandler : Handler
{
    public override MessageType Message => MessageType.ForceSyncTileEntity;

    public static void Send(int id)
    {
        ModPacket packet = PacketHandler.GetPacket(MessageType.ForceSyncTileEntity);
        packet.Write((ushort)id);

        using MemoryStream mem = new();
        using BinaryWriter writer = new(mem);
        ((ModTileEntity)TileEntity.ByID[id]).NetSend(writer);
        byte[] buffer = mem.GetBuffer();
        packet.Write((ushort)buffer.Length);
        packet.Write(buffer);

        packet.Send();
    }

    public override void Recieve(int whoAmI, BinaryReader reader, bool onServer)
    {
        int id = reader.ReadUInt16();
        int len = reader.ReadUInt16();
        byte[] bytes = reader.ReadBytes(len);

        using MemoryStream stream = new(bytes);
        using BinaryReader newReader = new(stream);

        ((ModTileEntity)TileEntity.ByID[id]).NetReceive(newReader);
        NetMessage.SendData(MessageID.TileEntitySharing, -1, whoAmI, null, id);
    }
}
