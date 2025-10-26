using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class ZooSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.ZooSignItem>();
    }
}
