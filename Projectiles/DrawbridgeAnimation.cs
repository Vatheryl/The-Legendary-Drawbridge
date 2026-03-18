using Drawbridge.Items;
using Drawbridge.Tiles;

namespace Drawbridge.Projectiles;

public class DrawbridgeAnimation : BaseDrawbridgeAnimation
{
    public override string Texture => "Drawbridge/Tiles/DrawbridgeTile";

    protected override DrawbridgeData SetDrawbridgeData() 
        => DrawbridgeData.Generic<DrawbridgeTile, DrawbridgeVerticalFlippedTile, DrawbridgeHorizontalTile, DrawbridgeHorizontalFlippedTile, DrawbridgeItem>(null);
}