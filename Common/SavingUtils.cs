namespace Drawbridge.Common;

internal static class SavingUtils
{
    /// <summary>
    /// Converts a type to an IO-safe string, in the format <c>Terraria/0</c> or <c>ModName/ContentName</c>.
    /// </summary>
    public static string IOType<T>(int type, int max) where T : ModType => type < max ? "Terraria/" + type : ModContent.GetInstance<T>().FullName;

    /// <summary>
    /// Converts an IO-safe string into a runtime ID.
    /// </summary>
    public static int GetTypeFromIO<T>(string type) where T : ModType
    {
        if (type.StartsWith("Terraria/"))
            return int.Parse(type.Replace("Terraria/", ""));
        else
        {
            ModType instance = ModContent.GetInstance<T>();
            return GetModTypeType(instance);
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
}
