using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class DryadSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.DryadSignItem>();
    }
}
