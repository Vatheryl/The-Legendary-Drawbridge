using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class DemolitionistSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.DemolitionistSignItem>();
    }
}
