using Drawbridge.Common;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace Drawbridge.Tiles.Doorcases;

internal abstract class Doorcase : ModTile
{
    internal class DoorcaseEntity : ModTileEntity, IClientSideTE
    {
        public float DoorProgress = 0;
        public bool DoorOpen = false;

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && ModContent.GetModTile(tile.TileType) is Doorcase;
        }

        public override void Update() => DoorProgress = MathHelper.Lerp(DoorProgress, DoorOpen ? 1 : 0, 0.1f);

        public override void NetSend(BinaryWriter writer) => writer.Write(DoorOpen);
        public override void NetReceive(BinaryReader reader) => DoorOpen = reader.ReadBoolean();
    }

    public static bool BookcaseIsOpen(Tile tile) => tile.TileFrameX >= 54;

    public static bool TryGetEntity(int x, int y, out DoorcaseEntity entity)
    {
        Tile tile = Main.tile[x, y];
        int i = x - tile.TileFrameX % 54 / 18;
        int j = y - tile.TileFrameY % 72 / 18;

        if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out TileEntity ent) && ent is DoorcaseEntity doorcase)
        {
            entity = doorcase;
            return true;
        }

        entity = null;
        return false;
    }

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
        RegisterItemDrop(ModContent.ItemType<GothicDoorcase.GothicDoorcaseItem>());

        AdjTiles = [TileID.Bookcases];
        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;
    }

    public override bool RightClick(int i, int j)
    {
        Tile tile = Main.tile[i, j];

        if (!TryGetEntity(i, j, out DoorcaseEntity entity))
            return true;

        if (!BookcaseIsOpen(tile))
        {
            ToggleDoorcaseFrames(i, j, null);
            entity.DoorOpen = true;
        }
        else
        {
            if (tile.TileFrameX % 54 == 0)
            {
                ToggleDoorcaseFrames(i, j, null);
                entity.DoorOpen = false;
            }
            else
                TryTeleport(i, j);
        }

        return true;
    }

    private void TryTeleport(int i, int j)
    {
        if (!Wiring.CheckMech(i, j, 20))
            return;

    }

    public override void HitWire(int i, int j)
    {

    }

    public static void ToggleDoorcaseFrames(int i, int j, bool? open)
    {
        Tile tile = Main.tile[i, j];
        bool openDoor = false;

        if (open is null && TryGetEntity(i, j, out DoorcaseEntity door))
            openDoor = door.DoorOpen;

        i -= tile.TileFrameX % 54 / 18;
        j -= tile.TileFrameY % 72 / 18;

        for (int x = i; x < i + 3; ++x)
        {
            for (int y = j; y < j + 4; y++)
            {
                Tile subTile = Main.tile[x, y];
                subTile.TileFrameX += (short)(openDoor ? -54 : 54);
            }
        }

        if (Main.netMode != NetmodeID.Server)
            NetMessage.SendTileSquare(-1, i, j, 3, 4);
    }

    public override void MouseOver(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        bool book = !BookcaseIsOpen(tile) || tile.TileFrameX % 54 == 0;
        
        if (book)
        {
            Main.LocalPlayer.cursorItemIconEnabled = true;
            Main.LocalPlayer.cursorItemIconID = ItemID.Book;
        }
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        if (!TryGetEntity(i, j, out DoorcaseEntity entity))
            return false;
        
        Tile tile = Main.tile[i, j];
        Texture2D texture = TextureAssets.Tile[Type].Value;
        Vector2 position = new Vector2(i, j).ToWorldCoordinates(0, 0) + TileExtensions.TileOffset - Main.screenPosition;

        var sourceRectangle = new Rectangle(tile.TileFrameX % 54 / 18 * 16, tile.TileFrameY / 18 * 16 + (int)(entity.DoorProgress * 18) * 64, 16, 16);
        spriteBatch.Draw(texture, position, sourceRectangle, Lighting.GetColor(i, j));
        return false;
    }
}
