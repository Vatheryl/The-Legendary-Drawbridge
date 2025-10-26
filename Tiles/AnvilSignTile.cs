using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class AnvilSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.AnvilSignItem>();
    }
}
