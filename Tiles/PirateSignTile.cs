using Terraria.ModLoader;

namespace Drawbridge.Tiles;

public class PirateSignTile : BaseSignTile
{
    protected override int DropItemType => ModContent.ItemType<Items.PirateSignItem>();
}
