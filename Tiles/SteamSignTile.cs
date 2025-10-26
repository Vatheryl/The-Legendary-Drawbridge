using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class SteamSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.SteamSignItem>();
    }
}
