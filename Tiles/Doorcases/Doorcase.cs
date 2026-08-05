using Drawbridge.Common;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace Drawbridge.Tiles.Doorcases;

internal class Doorcase : ModTile
{
    internal class DoorcaseEntity : ModTileEntity, IClientSideTE
    {
        public float DoorProgress = 0;
        public bool DoorOpen = true;

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && ModContent.GetModTile(tile.TileType) is Doorcase;
        }

        public override void Update()
        {
            DoorProgress = MathHelper.Lerp(DoorProgress, DoorOpen ? 1 : 0, 0.1f);
        }

        public override void NetSend(BinaryWriter writer) => writer.Write(DoorOpen);
        public override void NetReceive(BinaryReader reader) => DoorOpen = reader.ReadBoolean();
    }

    public static bool BookcaseIsOpen(Tile tile) => tile.TileFrameX >= 54;
    public static DoorcaseEntity GetEntity(int x, int y) => TileEntity.ByPosition[new Point16(x, y)] as DoorcaseEntity;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileSolidTop[Type] = false;

        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
        TileObjectData.newTile.StyleWrapLimitVisualOverride = 1;
        TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<DoorcaseEntity>().Generic_HookPostPlaceMyPlayer;
        TileObjectData.addTile(Type);

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
        AddMapEntry(new Color(120, 85, 60), CreateMapEntryName());

        AdjTiles = [TileID.Bookcases];
        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;
    }

    public override bool RightClick(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        DoorcaseEntity entity = GetEntity(i, j);

        if (BookcaseIsOpen(tile))
        {
            ToggleDoorcaseFrames(i, j, true);
            entity.DoorOpen = true;
        }
        else
        {
            if (tile.TileFrameX == 0)
            {
                ToggleDoorcaseFrames(i, j, false);
                entity.DoorOpen = false;
            }
        }

        return true;
    }

    public override void HitWire(int i, int j)
    {

    }

    public static void ToggleDoorcaseFrames(int i, int j, bool? open)
    {
        Tile tile = Main.tile[i, j];
        i -= tile.TileFrameX % 54;
        j -= tile.TileFrameY % 74;
        bool openDoor = false;

        if (open.HasValue && open.Value)
            openDoor = true;
        else if (!open.HasValue && !BookcaseIsOpen(tile))
            openDoor = true;

        for (int x = i; x < i + 3; ++i)
        {
            for (int y = j; y < j + 3; y++)
            {
                Tile subTile = Main.tile[x, y];
                subTile.TileFrameX += (short)(openDoor ? 54 : -54);
            }
        }

        if (Main.netMode != NetmodeID.Server)
            NetMessage.SendTileSquare(-1, i, j, 3, 4);
    }

    public override void MouseOver(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        bool book = !BookcaseIsOpen(tile) || tile.TileFrameX == 0;
        
        if (book)
        {
            Main.LocalPlayer.cursorItemIconEnabled = true;
            Main.LocalPlayer.cursorItemIconID = ItemID.Book;
        }
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Tile tile = Main.tile[i, j];
        Texture2D texture = TextureAssets.Tile[Type].Value;
        Vector2 position = new Vector2(i, j).ToWorldCoordinates(0, 0) + TileExtensions.TileOffset;
        DoorcaseEntity entity = GetEntity(i, j);
        
        spriteBatch.Draw(texture, position, new Rectangle(tile.TileFrameX, tile.TileFrameY + (int)(entity.DoorProgress * 6) * 54, 16, 16), Lighting.GetColor(i, j));
        return false;
    }
}
