using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace Drawbridge.Common.Tiles;

internal interface IWaterfallTile
{
    public class WaterfallTileLoader : ILoadable
    {
        void ILoadable.Load(Mod mod)
        {
            IL_WaterfallManager.FindWaterfalls += WaterfallManager_FindWaterfalls;
        }

        private void WaterfallManager_FindWaterfalls(ILContext il)
        {
            ILCursor c = new(il);

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
                currentMax++;
            }
        }

        void ILoadable.Unload()
        {
        }
    }

    public bool HasWaterfall(ref int i, ref int j, Tile tile, ref int waterfallType);
}
