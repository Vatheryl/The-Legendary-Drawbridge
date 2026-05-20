namespace Drawbridge.Common;

internal class CrossmodContent : ILoadable
{
    public readonly record struct ModInstance
    {
        public readonly bool Enabled;

        private readonly Mod Mod;

        public ModInstance(string name)
        {
            if (ModLoader.TryGetMod(name, out Mod))
                Enabled = true;
            else
                Enabled = false;
        }

        public readonly T Find<T>(string name) where T : ModType => Mod.Find<T>(name);

        public readonly bool TryFind<T>(string name, out T value) where T : ModType
        {
            value = default;

            if (!Enabled)
                return false;

            return Mod.TryFind(name, out value);
        }

        /// <summary>
        /// Searches for a crossmod <see cref="ModType"/>, and compares <paramref name="type"/> to the <see cref="ModType"/>'s type.
        /// Returns true when the <see cref="ModType"/> is found, and who's Type is equal to <paramref name="type"/>.
        /// </summary>
        public readonly bool EquateType<T>(string name, int type) where T : ModType
        {
            if (!TryFind(name, out T value))
            {
                return false;
            }

            return SavingUtils.GetModTypeType(value) == type;
        }
    }

    public static ModInstance Reforged { get; private set; }

    public void Load(Mod mod) => Reforged = new ModInstance("SpiritReforged");

    public void Unload() { }
}
