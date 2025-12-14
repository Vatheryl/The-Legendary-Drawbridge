using Drawbridge.Items.HorseArmor;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Drawbridge.Systems;

// Stores per-placed EquineArmatureTile overlay state
public class EquineArmatureWorld : ModSystem
{
    /// <summary>
    /// Overlay stored as item ID.
    /// </summary>
    private static readonly Dictionary<Point16, int> OverlayTypeByTopLeft = new();
    private static readonly Dictionary<Point16, bool> FlippedByTopLeft = new();

    public static int GetType(Point16 topLeft) => OverlayTypeByTopLeft.TryGetValue(topLeft, out int v) ? v : 0;

    public static bool TryGetType(Point16 topLeft, out int value) => OverlayTypeByTopLeft.TryGetValue(topLeft, out value);

    public static void SetType(Point16 topLeft, int overlayType)
    {
        if (overlayType != 0)
            OverlayTypeByTopLeft[topLeft] = overlayType;
        else
            OverlayTypeByTopLeft.Remove(topLeft);
    }

    public static bool GetFlipped(Point16 topLeft) => FlippedByTopLeft.TryGetValue(topLeft, out bool v) && v;

    public static void SetFlipped(Point16 topLeft, bool flipped)
    {
        if (flipped)
            FlippedByTopLeft[topLeft] = true;
        else
            FlippedByTopLeft.Remove(topLeft);
    }

    public static void SetAndSyncFlipped(Point16 topLeft, bool flipped)
    {
        SetFlipped(topLeft, flipped);

        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            ModPacket p = ModContent.GetInstance<Drawbridge>().GetPacket();
            p.Write((byte)MessageType.SyncEquineArmatureFlip);
            p.Write((int)topLeft.X);
            p.Write((int)topLeft.Y);
            p.Write(flipped);
            p.Send();
        }
    }

    public static void SetAndSyncType(Point16 topLeft, int overlayType)
    {
        SetType(topLeft, overlayType);

        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            ModPacket p = ModContent.GetInstance<Drawbridge>().GetPacket();
            p.Write((byte)MessageType.SyncEquineArmatureArmor);
            p.Write((int)topLeft.X);
            p.Write((int)topLeft.Y);
            p.Write(overlayType);
            p.Send();
        }
    }

    public override void SaveWorldData(TagCompound tag)
    {
        var list = new List<TagCompound>();
        foreach (var kv in OverlayTypeByTopLeft)
        {
            list.Add(new TagCompound
            {
                ["x"] = kv.Key.X,
                ["y"] = kv.Key.Y,
                ["type"] = kv.Value,
            });
        }

        tag["EquineArmatureEquips"] = list;

        var listFlip = new List<TagCompound>();
        foreach (var kv in FlippedByTopLeft)
        {
            listFlip.Add(new TagCompound
            {
                ["x"] = kv.Key.X,
                ["y"] = kv.Key.Y,
                ["fl"] = kv.Value,
            });
        }

        tag["EquineArmatureFlip"] = listFlip;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        OverlayTypeByTopLeft.Clear();
        FlippedByTopLeft.Clear();

        if (tag.ContainsKey("EquineArmatureEquips"))
        {
            var list = tag.GetList<TagCompound>("EquineArmatureEquips");

            foreach (var t in list)
            {
                var pt = new Point16(t.GetShort("x"), t.GetShort("y"));
                int type;

                if (t.ContainsKey("type"))
                {
                    type = t.GetInt("type");

                    // Load legacy IDs from old, terrible system
                    if (type == 1)
                        type = ModContent.ItemType<IronFieldBardingItem>();
                    else if (type == 2)
                        type = ModContent.ItemType<GoldArmorItem>();
                    else if (type == 3)
                        type = ModContent.ItemType<MoltenArmorItem>();
                    else if (type == 4)
                        type = ModContent.ItemType<CopperArmorItem>();
                    else if (type == 5)
                        type = ModContent.ItemType<SilverArmorItem>();
                }
                else
                {
                    // Backward compat with boolean save
                    bool eq = t.GetBool("eq");
                    type = eq ? 1 : 0;
                }

                if (type != 0)
                    OverlayTypeByTopLeft[pt] = type;
            }
        }

        if (tag.ContainsKey("EquineArmatureFlip"))
        {
            var listFlip = tag.GetList<TagCompound>("EquineArmatureFlip");
            foreach (var t in listFlip)
            {
                var pt = new Point16(t.GetShort("x"), t.GetShort("y"));
                bool fl = t.GetBool("fl");

                if (fl)
                    FlippedByTopLeft[pt] = true;
            }
        }
    }

    public override void NetSend(BinaryWriter writer)
    {
        // Overlay types
        writer.Write(OverlayTypeByTopLeft.Count);
        foreach (var kv in OverlayTypeByTopLeft)
        {
            writer.Write(kv.Key.X);
            writer.Write(kv.Key.Y);
            writer.Write(kv.Value);
        }

        // Flip states
        writer.Write(FlippedByTopLeft.Count);
        foreach (var kv in FlippedByTopLeft)
        {
            writer.Write(kv.Key.X);
            writer.Write(kv.Key.Y);
            writer.Write(kv.Value);
        }
    }

    public override void NetReceive(BinaryReader reader)
    {
        OverlayTypeByTopLeft.Clear();
        FlippedByTopLeft.Clear();

        int overlayCount = reader.ReadInt32();
        for (int n = 0; n < overlayCount; n++)
        {
            short x = reader.ReadInt16();
            short y = reader.ReadInt16();
            int type = reader.ReadInt32();

            if (type != 0)
                OverlayTypeByTopLeft[new Point16(x, y)] = type;
        }

        int flipCount = reader.ReadInt32();
        for (int n = 0; n < flipCount; n++)
        {
            short x = reader.ReadInt16();
            short y = reader.ReadInt16();
            bool flipped = reader.ReadBoolean();

            if (flipped)
                FlippedByTopLeft[new Point16(x, y)] = true;
        }
    }
}



