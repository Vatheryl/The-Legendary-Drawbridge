using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class CyberSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.CyberSignItem>();
    }
}
