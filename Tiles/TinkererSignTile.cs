using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class TinkererSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.TinkererSignItem>();
    }
}
