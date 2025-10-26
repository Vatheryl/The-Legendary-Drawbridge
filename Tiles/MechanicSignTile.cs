using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class MechanicSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.MechanicSignItem>();
    }
}
