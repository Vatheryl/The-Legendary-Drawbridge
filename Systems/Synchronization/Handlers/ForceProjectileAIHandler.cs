using System.IO;
using System.Linq;
using Terraria;

namespace Drawbridge.Systems.Synchronization.Handlers;

/// <summary>
/// Used to forcefully set <see cref="Projectile.ai"/>, <see cref="Projectile.localAI"/> and/or <see cref="Terraria.ModLoader.ModProjectile.SendExtraAI(BinaryWriter)"/>
/// values from the client.
/// </summary>
internal class ForceProjectileAIHandler : Handler
{
    public override MessageType Message => MessageType.ForceProjectileAI;

    public static void Send(Projectile projectile, bool includeLocal = false, bool netWrite = false)
    {
        var packet = PacketHandler.GetPacket(MessageType.ForceProjectileAI, netWrite ? 255 : 1 + sizeof(float) * (includeLocal ? 3 : 6));
        packet.Write((short)projectile.identity);
        packet.Write(new BitsByte(includeLocal, netWrite));
        
        for (int i = 0; i < projectile.ai.Length; ++i)
            packet.Write(projectile.ai[i]);

        if (includeLocal)
        {
            for (int i = 0; i < projectile.localAI.Length; ++i)
                packet.Write(projectile.localAI[i]);
        }

        if (netWrite && projectile.ModProjectile is { } modProj)
            modProj.SendExtraAI(packet);

        packet.Send();
    }

    public override void Recieve(int whoAmI, BinaryReader reader, bool onServer)
    {
        short id = reader.ReadInt16();
        BitsByte locals = reader.ReadByte();

        Projectile projectile = Main.projectile.FirstOrDefault(x => x.identity == id);

        if (projectile is null)
            return;

        for (int i = 0; i < projectile.ai.Length; ++i)
            projectile.ai[i] = reader.ReadSingle();

        if (locals[0])
        {
            for (int i = 0; i < projectile.localAI.Length; ++i)
                projectile.localAI[i] = reader.ReadSingle();
        }

        if (locals[1] && projectile.ModProjectile is { } modProj)
            modProj.ReceiveExtraAI(reader);

        projectile.netUpdate = true;
    }
}
