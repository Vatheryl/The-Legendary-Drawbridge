using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class StylistSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.StylistSignItem>();
    }
}
