using Terraria.ID;

namespace Drawbridge.Tiles.Doorcases;

internal class WoodenDoorcase : Doorcase<WoodenDoorcase.WoodenDoorcaseItem>
{
    [LegacyName("DoorcaseItem", "GothicDoorcaseItem")]
    public class WoodenDoorcaseItem : DoorcaseItem<WoodenDoorcase>
    {
        public override int Material => ItemID.Wood;
    }
}
