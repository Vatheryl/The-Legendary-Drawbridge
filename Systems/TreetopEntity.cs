using Drawbridge.Systems.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace Drawbridge.Systems;

internal class TreetopEntity : GlobalTile
{
    public enum TreeType : byte
    {
        Purity,
        Corruption,
        Jungle,
        Hallow,
        Snow,
        Crimson,
        UndergroundJungle,
        Mushroom,
    }

    private class Treetop
    {
        public short x = 0;
        public short y = 0;
        public int eyeCount = 0;
    }

    private static readonly Dictionary<Point16, Treetop> Treetops = [];

    public override void NearbyEffects(int i, int j, int type, bool closer)
    {
        if (!TileValidForTreetop(i, j))
            return;
        
        TryAddTreetop(i, j);
    }

    private static void TryAddTreetop(int i, int j) => Treetops.TryAdd(new Point16(i, j), new Treetop()
    {
        x = (short)i,
        y = (short)j,
        eyeCount = 0
    });

    private static bool TileValidForTreetop(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        return tile.TileType == TileID.Trees && tile.TileFrameX == 22 && tile.TileFrameY >= 198 
            && GetTreeType(i, j) is TreeType.Purity or TreeType.Corruption or TreeType.Crimson or TreeType.Snow;
    }

    private static TreeType GetTreeType(int i, int j)
    {
        Tile tile = Main.tile[i, j];

        while (tile.TileType == 5)
        {
            j++;
            tile = Main.tile[i, j];
        }

        return tile.TileType switch
        {
            TileID.Grass or TileID.GolfGrass => TreeType.Purity,
            TileID.SnowBlock => TreeType.Snow,
            TileID.JungleGrass => j > Main.worldSurface ? TreeType.UndergroundJungle : TreeType.Jungle,
            TileID.MushroomGrass => TreeType.Mushroom,
            TileID.CorruptGrass => TreeType.Corruption,
            TileID.CrimsonGrass => TreeType.Crimson,
            TileID.HallowedGrass or TileID.GolfGrassHallowed => TreeType.Hallow,
            _ => TreeType.Purity
        };
    }

    public override void RandomUpdate(int i, int j, int type)
    {
        if (!TileValidForTreetop(i, j))
            return;

        TryAddTreetop(i, j);
        Treetop top = Treetops[new Point16(i, j)];

        if (WorldGen.genRand.NextBool(10))
            top.eyeCount++;
    }

    public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch)
    {
        if (!Main.dayTime && !TileValidForTreetop(i, j) || !Treetops.TryGetValue(new Point16(i, j), out Treetop top))
            return;

        float opacity = 1;

        if (Main.time < 600)
            opacity = (float)Main.time / 600f;
        else if (Main.time > Main.dayLength - 600)
            opacity = (float)Utils.GetLerpValue(Main.dayLength, Main.dayLength - 600, Main.time);

        Vector2 position = this.DrawPosition(i - 1, j);
        ChatManager.DrawColorCodedString(spriteBatch, FontAssets.ItemStack.Value, top.eyeCount.ToString(), position, Color.White * opacity, 0f, Vector2.Zero, Vector2.One);
    }
}
