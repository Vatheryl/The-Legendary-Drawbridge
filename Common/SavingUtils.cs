using System;

namespace Drawbridge.Common;

internal static class SavingUtils
{
    /// <summary>
    /// Converts a type to an IO-safe string, in the format <c>Terraria/0</c> or <c>ModName/ContentName</c>.
    /// </summary>
    public static string IOType<T>(int type, int max) where T : ModType => type < max ? "Terraria/" + type : GetModTypeFullNameFromType<T>(type);

    /// <summary>
    /// Converts an IO-safe string into a runtime ID.
    /// </summary>
    public static int GetTypeFromIO<T>(string type) where T : ModType
    {
        if (type.StartsWith("Terraria/"))
            return int.Parse(type.Replace("Terraria/", ""));
        else
        {
            if (ModContent.TryFind(type, out T value))
                return GetModTypeType(value);

            return -1;
        }
    }

    internal static int GetModTypeType<T>(T value) where T : ModType => value switch
    {
        ModItem item => item.Type,
        ModTile tile => tile.Type,
        ModNPC npc => npc.Type,
        ModProjectile projectile => projectile.Type,
        ModGore gore => gore.Type,
        ModDust dust => dust.Type,
        _ => -1,
    };

    internal static string GetModTypeFullNameFromType<T>(int type) where T : ModType
    {
        Type tType = typeof(T);

        if (tType == typeof(ModItem))
            return ModContent.GetModItem(type).FullName;
        else if (tType == typeof(ModProjectile))
            return ModContent.GetModProjectile(type).FullName;
        else if (tType == typeof(ModTile))
            return ModContent.GetModTile(type).FullName;
        else if (tType == typeof(ModNPC))
            return ModContent.GetModNPC(type).FullName;
        else if (tType == typeof(ModDust))
            return ModContent.GetModDust(type).FullName;

        return null;
    }
}
