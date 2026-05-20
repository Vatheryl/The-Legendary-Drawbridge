using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;

namespace Drawbridge.Common.Tiles;

internal interface IDrawableTE
{
    internal class TileEntityHooks : ILoadable
    {
        public void Load(Mod mod) => On_Main.DrawNPCs += DrawTEs;

        void ILoadable.Unload() { }

        private void DrawTEs(On_Main.orig_DrawNPCs orig, Main self, bool behind)
        {
            if (behind)
            {
                orig(self, behind);
                return;
            }

            foreach (var item in TileEntity.ByID)
            {
                if (item.Value is IDrawableTE te && te.CanDraw(item.Value.Position.ToWorldCoordinates(0, 0)))
                    te.Draw(Main.spriteBatch);
            }

            orig(self, behind);
        }
    }

    protected Point Size { get; }

    internal void Draw(SpriteBatch spriteBatch);

    public virtual bool CanDraw(Vector2 world) => OnScreen(new Rectangle((int)(world.X - Main.screenPosition.X), (int)(world.Y - Main.screenPosition.Y), Size.X, Size.Y));

    public static bool OnScreen(Rectangle rect) => rect.Intersects(new Rectangle(0, 0, Main.screenWidth, Main.screenHeight));
}
