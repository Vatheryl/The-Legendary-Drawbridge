using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class ClothierSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.ClothierSignItem>();
    }
}
