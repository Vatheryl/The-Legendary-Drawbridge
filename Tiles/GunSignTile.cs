using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class GunSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.GunSignItem>();
    }
}
