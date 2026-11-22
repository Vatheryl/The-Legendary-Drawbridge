using Terraria.ID;

namespace Drawbridge.Tiles.Doorcases;

internal class BlueDungeonDoorcase : Doorcase<BlueDungeonDoorcase.BlueDungeonDoorcaseItem>
{
    public class BlueDungeonDoorcaseItem : DoorcaseItem<BlueDungeonDoorcase>
    {
        public override int Material => ItemID.BlueBrick;
    }
}
