using Terraria.ModLoader;

namespace Drawbridge.Tiles
{
    public class Fish_Sign_Placed : BaseSignTile
    {
        protected override int DropItemType => ModContent.ItemType<Items.Fish_Sign_Item>();
    }
}
