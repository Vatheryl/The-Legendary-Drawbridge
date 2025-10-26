using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class TruffleSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.TruffleSignItem>();
    }
}
