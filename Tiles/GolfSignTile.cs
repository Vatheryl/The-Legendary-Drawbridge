using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class GolfSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.GolfSignItem>();
    }
}
