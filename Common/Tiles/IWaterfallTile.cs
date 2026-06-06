using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using Terraria;

namespace Drawbridge.Common.Tiles;

internal interface IWaterfallTile
{
    public record struct WaterfallInstance(int X, int Y, Tile Tile, IWaterfallTile TileModifier);

    public class WaterfallTileLoader : ILoadable
    {
        public static Dictionary<int, WaterfallInstance> SpecialIndexes = [];

        private static int? CurrentIndex = null;

        void ILoadable.Load(Mod mod)
        {
            IL_WaterfallManager.FindWaterfalls += WaterfallManager_FindWaterfalls;
            IL_WaterfallManager.DrawWaterfall_int_float += AddSpecialDrawHook;
            On_WaterfallManager.DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects += ModifyWaterfallDrawing;
        }

        private void ModifyWaterfallDrawing(On_WaterfallManager.orig_DrawWaterfall_int_int_int_float_Vector2_Rectangle_Color_SpriteEffects orig, WaterfallManager self, 
            int waterfallType, int x, int y, float opacity, Vector2 position, Rectangle sourceRect, Color color, SpriteEffects effects)
        {
            if (CurrentIndex is { } value && SpecialIndexes.TryGetValue(value, out WaterfallInstance inst))
                inst.TileModifier.ModifyDrawing(inst, ref waterfallType, ref x, ref y, ref opacity, ref position, sourceRect, ref color, ref effects);

            orig(self, waterfallType, x, y, opacity, position, sourceRect, color, effects);
        }

        private void AddSpecialDrawHook(ILContext il)
        {
            ILCursor c = new(il);

            if (!c.TryGotoNext(x => x.MatchBr(out _)))
                return;

            int iterationLocal = -1;

            if (!c.TryGotoPrev(x => x.MatchStloc(out iterationLocal)))
                return;

            if (iterationLocal == -1)
                return;

            c.EmitLdloc(iterationLocal); // iteration variable
            c.EmitDelegate(ModifyWaterfallDrawingFunctionality);
        }

        private static void ModifyWaterfallDrawingFunctionality(int waterfallWho) => CurrentIndex = !SpecialIndexes.ContainsKey(waterfallWho) ? null : waterfallWho;

        private void WaterfallManager_FindWaterfalls(ILContext il)
        {
            ILCursor c = new(il);

            if (!c.TryGotoNext(MoveType.After, x => x.MatchLdcI4(0)))
                return;

            c.EmitDelegate(SpecialIndexes.Clear);

            if (!c.TryGotoNext(MoveType.After, x => x.MatchCall<Tile>("active")))
                return;

            c.Emit(OpCodes.Ldloc_S, (byte)4); //i
            c.Emit(OpCodes.Ldloc_S, (byte)5); //j
            c.Emit(OpCodes.Ldarg_0);
            c.Emit<WaterfallManager>(OpCodes.Ldfld, "waterfalls"); //WaterfallManager.waterfalls
            c.Emit(OpCodes.Ldarg_0);
            c.Emit<WaterfallManager>(OpCodes.Ldflda, "currentMax"); //WaterfallManager.currentMax
            c.Emit(OpCodes.Ldarg_0);
            c.Emit<WaterfallManager>(OpCodes.Ldfld, "qualityMax"); //WaterfallManager.qualityMax

            c.EmitDelegate(AddWaterFlowersFalls); //Call the adjustment method (which also sets currentMax)
        }

        private static void AddWaterFlowersFalls(int i, int j, WaterfallManager.WaterfallData[] data, ref int currentMax, int qualityMax)
        {
            if (currentMax >= qualityMax || data is null)
                return;

            Tile currentTile = Main.tile[i, j];
            int waterfallType = 1;

            if (currentTile.HasTile && ModContent.GetModTile(currentTile.TileType) is IWaterfallTile tile && tile.HasWaterfall(ref i, ref j, currentTile, ref waterfallType))
            {
                data[currentMax].x = i;
                data[currentMax].y = j;
                data[currentMax].type = waterfallType;

                SpecialIndexes.Add(currentMax, new WaterfallInstance(i, j, currentTile, tile));
                currentMax++;
            }
        }

        void ILoadable.Unload()
        {
        }
    }

    /// <summary>
    /// Unfinished, doesn't work correctly.
    /// </summary>
    public void ModifyDrawing(WaterfallInstance instance, ref int type, ref int x, ref int y, ref float opacity, ref Vector2 pos, Rectangle src, ref Color color, ref SpriteEffects fx)
    {
    }

    public bool HasWaterfall(ref int i, ref int j, Tile tile, ref int waterfallType);
}
