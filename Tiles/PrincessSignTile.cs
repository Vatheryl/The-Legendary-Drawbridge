using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class PrincessSignTile : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.PrincessSignItem>();
    }
}
