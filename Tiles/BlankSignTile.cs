using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class BlankSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.BlankSignItem>();
    }
}
