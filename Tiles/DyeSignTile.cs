using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class DyeSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.DyeSignItem>();
    }
}
