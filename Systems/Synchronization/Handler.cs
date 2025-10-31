using System.Collections.Generic;
using System.IO;
using Terraria.ModLoader;

namespace Drawbridge.Systems.Synchronization;

internal abstract class Handler : ILoadable
{
    public static Dictionary<MessageType, Handler> HandlerByType = [];

    public abstract MessageType Message { get; }

    public void Load(Mod mod)
    {
        HandlerByType.Add(Message, this);

        OnLoad(mod);
    }

    protected virtual void OnLoad(Mod mod) { }

    public virtual void Unload() => HandlerByType.Remove(Message);

    public abstract void Recieve(int whoAmI, BinaryReader reader, bool onServer);
}
