using Terraria.ID;

namespace Drawbridge.Tiles.Doorcases;

internal class PinkDungeonDoorcase : Doorcase<PinkDungeonDoorcase.PinkDungeonDoorcaseItem>
{
    public class PinkDungeonDoorcaseItem : DoorcaseItem<PinkDungeonDoorcase>
    {
        public override int Material => ItemID.PinkBrick;
    }
}
