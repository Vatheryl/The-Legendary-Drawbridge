using Terraria.ID;

namespace Drawbridge.Tiles.Doorcases;

internal class GreenDungeonDoorcase : Doorcase<GreenDungeonDoorcase.GreenDungeonDoorcaseItem>
{
    public class GreenDungeonDoorcaseItem : DoorcaseItem<GreenDungeonDoorcase>
    {
        public override int Material => ItemID.GreenBrick;
    }
}
