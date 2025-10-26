using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class MerchantSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.MerchantSignItem>();
    }
}
