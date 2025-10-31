using Drawbridge.Systems.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Drawbridge.Systems.TreetopEyes;

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

    internal class Treetop
    {
        public const int MaxEyes = 4;

        public short x = 0;
        public short y = 0;
        public byte eyeCount = 0;

        /// <summary>
        /// Variants of the treetop. Always 4 (<see cref="MaxEyes"/>) long.
        /// </summary>
        public byte[] variants = new byte[MaxEyes];
    }

    private class TreetopIO : ModSystem
    {
        public override void SaveWorldData(TagCompound tag)
        {
            tag.Add("count", Treetops.Count);
            int id = 0;

            foreach (var treetop in Treetops.Values)
            {
                TagCompound treetopTag = [];
                treetopTag.Add("x", treetop.x);
                treetopTag.Add("y", treetop.y);
                treetopTag.Add("eye", treetop.eyeCount);
                treetopTag.Add("var", treetop.variants);
                tag.Add("treetop" + id, treetopTag);

                id++;
            }
        }

        public override void LoadWorldData(TagCompound tag)
        {
            int count = tag.GetInt("count");

            Treetops.Clear();

            for (int i = 0; i < count; i++)
            {
                TagCompound treetopTag = tag.GetCompound("treetop" + i);

                Treetop top = new()
                {
                    x = treetopTag.GetShort("x"),
                    y = treetopTag.GetShort("y"),
                    eyeCount = treetopTag.GetByte("eye"),
                    variants = treetopTag.GetByteArray("var"),
                };

                Treetops.Add(new Point16(top.x, top.y), top);
            }
        }
    }

    internal static readonly Dictionary<Point16, Treetop> Treetops = [];

    private static Vector2[] Offsets => [new Vector2(-16, -18), new(-4, -60), new(8, -4), new(6, -32), new(18, -20), new(10, -46)];

    private static Asset<Texture2D> HangingEye = null;

    public override void Load()
    {
        HangingEye = ModContent.Request<Texture2D>("Drawbridge/Systems/TreetopEyes/HangingEye");

        IL_TileDrawing.DrawTrees += AddPostDrawTreetopHook;
    }

    private void AddPostDrawTreetopHook(ILContext il)
    {
        ILCursor c = new(il);

        int treeTopIndex = 0;
        int xIndex = 0;
        int yIndex = 0;

        // Anchor to the x/y locals
        if (!c.TryGotoNext(x => x.MatchLdfld<Point>(nameof(Point.X))))
            return;

        // Get x
        if (!c.TryGotoNext(x => x.MatchStloc(out xIndex)))
            return;

        // Get y
        if (!c.TryGotoNext(x => x.MatchStloc(out yIndex)))
            return;

        // Move to treetoptexture call
        if (!c.TryGotoNext(x => x.MatchCall<TileDrawing>("GetTreeTopTexture")))
            return;

        // Get treetoptexture index
        if (!c.TryGotoNext(x => x.MatchStloc(out treeTopIndex)))
            return;
         
        // Find treetop texture usage
        if (!c.TryGotoNext(x => x.MatchLdloc(treeTopIndex)))
            return;

        // Match the "exit" label
        if (!c.TryGotoNext(x => x.MatchBneUn(out _)))
            return;

        c.Emit(OpCodes.Ldloc_S, (byte)xIndex);
        c.Emit(OpCodes.Ldloc_S, (byte)yIndex);
        c.EmitDelegate(PostDrawTreetop);
    }

    private static void PostDrawTreetop(int x, int y) => CheckDrawEyeballs(x, y, Main.spriteBatch, true);

    private static void TryAddTreetop(int i, int j) => Treetops.TryAdd(new Point16(i, j), new Treetop()
    {
        x = (short)i,
        y = (short)j,
        eyeCount = 0,
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

        if (WorldGen.genRand.NextBool(25) && top.eyeCount < Treetop.MaxEyes)
        {
            top.variants[top.eyeCount] = (byte)Main.rand.Next(Treetop.MaxEyes);
            top.eyeCount++;
        }
    }

    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        bool hasTop = Treetops.TryGetValue(new Point16(i, j), out Treetop top);

        if (hasTop)
            Treetops.Remove(new Point16(i, j));

        if (fail || !Main.dayTime || !TileValidForTreetop(i, j) || !hasTop)
            return;

        for (int k = 0; k < top.eyeCount; ++k)
        {
            int offset = GetOffsetFromPositionAndIteration(i, j, k);
            Vector2 pos = new Vector2(i, j).ToWorldCoordinates(8, 24) + Offsets[offset];
            int npc = NPC.NewNPC(new EntitySource_TileBreak(i, j), (int)pos.X, (int)pos.Y, Main.rand.NextBool() ? NPCID.SleepyEye : NPCID.SleepyEye2);
            Main.npc[npc].GetGlobalNPC<AwokenEyeNPC>().awoken = true;
        }
    }

    public override void PostDraw(int i, int j, int type, SpriteBatch spriteBatch) => CheckDrawEyeballs(i, j, spriteBatch, false);

    private static void CheckDrawEyeballs(int i, int j, SpriteBatch spriteBatch, bool front)
    {
        if (!Main.dayTime && !TileValidForTreetop(i, j) || !Treetops.TryGetValue(new Point16(i, j), out Treetop top))
            return;

        float opacity = 1;

        if (Main.time < 600)
            opacity = (float)Main.time / 600f;
        else if (Main.time > Main.dayLength - 600)
            opacity = (float)Utils.GetLerpValue(Main.dayLength, Main.dayLength - 600, Main.time);

        for (int k = 0; k < top.eyeCount; ++k)
        {
            int offset = GetOffsetFromPositionAndIteration(i, j, k);
            bool needsFront = offset % 3 != 1;

            if (needsFront != front)
                continue;

            Vector2 off = Offsets[offset];
            Vector2 position = TileExtensions.DrawPosition(i, j, off, front);
            Rectangle source = new(34 * top.variants[k], 0, 36, 36);

            if (top.variants[k] == 3)
            {
                source.Y = (int)(Main.GameUpdateCount * 0.07f) % 5 * 38;
            }

            float wind = Main.instance.TilesRenderer.GetWindCycle(i, j, Main.instance.TilesRenderer._treeWindCounter + offset * 0.04f);
            spriteBatch.Draw(HangingEye.Value, position, source, Lighting.GetColor(i, j) * opacity, wind, new Vector2(11, 0), 1f, SpriteEffects.None, 0);
        }
    }

    private static int GetOffsetFromPositionAndIteration(int i, int j, int k) => (int)(k * 2.3f + i + j * 1.667f + MathF.Sin(i + j * 5)) % Offsets.Length;
}
