using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Drawbridge;

public class DrawbridgeConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    public enum HorseMountSlotPosition : byte
    {
        Default,
        DownOne,
        NextToBuffs,
        LeftOfTop,
        LeftOfBottom
    }

    [DefaultValue(false)]
    public bool MuteClockSound { get; set; } = false;

    [DefaultValue(false)]
    public bool MuteStableNoises { get; set; } = false;

    [DefaultValue(false)]
    public bool DisableHorseStablePooping { get; set; } = false;

    [DefaultValue(1f)]
    [Range(0.8f, 5f)]
    public float PortcullisSpeed { get; set; }

    [DefaultValue(HorseMountSlotPosition.Default)]
    public HorseMountSlotPosition SlotPosition { get; set; }
}