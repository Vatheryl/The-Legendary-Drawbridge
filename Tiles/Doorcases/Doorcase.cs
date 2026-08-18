using Drawbridge.Common;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Drawbridge.Tiles.Doorcases;

internal abstract class Doorcase : ModTile
{
    internal class DoorcaseEntity : ModTileEntity, IClientSideTE
    {
        public float DoorProgress = 0;
        public bool DoorOpen { get; private set; }

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && ModContent.GetModTile(tile.TileType) is Doorcase;
        }

        public void OpenDoor(bool newValue)
        {
            DoorOpen = newValue;

            if (Main.netMode != NetmodeID.SinglePlayer)
                ForceSyncTEHandler.Send(ID);
        }

        public override void Update() => DoorProgress = MathHelper.Lerp(DoorProgress, DoorOpen ? 1 : 0, 0.1f);

        public override void NetSend(BinaryWriter writer) => writer.Write(DoorOpen);
        public override void NetReceive(BinaryReader reader) => DoorOpen = reader.ReadBoolean();

        public override void SaveData(TagCompound tag)
        {
            tag.Add("open", DoorOpen);
            tag.Add("prog", DoorProgress);
        }

        public override void LoadData(TagCompound tag)
        {
            DoorOpen = tag.GetBool("open");
            DoorProgress = tag.GetFloat("prog");
        }
    }

    private static readonly bool[] _teleportIsDoorcase = [false, false];

    public static bool BookcaseIsOpen(Tile tile) => tile.TileFrameX >= 54;

    public static bool TryGetEntity(ref int x, ref int y, out DoorcaseEntity entity)
    {
        Tile tile = Main.tile[x, y];
        x -= tile.TileFrameX % 54 / 18;
        y -= tile.TileFrameY % 72 / 18;

        if (TileEntity.ByPosition.TryGetValue(new Point16(x, y), out TileEntity ent) && ent is DoorcaseEntity doorcase)
        {
            entity = doorcase;
            return true;
        }

        entity = null;
        return false;
    }

    public static bool TryGetEntity(int x, int y, out DoorcaseEntity entity) => TryGetEntity(ref x, ref y, out entity);

    public override void Load()
    {
        IL_Wiring.Teleport += ModifyTeleport;
        On_Wiring.Teleport += UnsetFlags;
        On_Player.Teleport += On_Player_Teleport;
    }

    private void On_Player_Teleport(On_Player.orig_Teleport orig, Player self, Vector2 newPos, int Style, int extraInfo)
    {
        Point16 pos = newPos.ToTileCoordinates16();
        Tile tile = Main.tile[pos.X, pos.Y - 1];

        if (tile.HasTile && ModContent.GetModTile(tile.TileType) is Doorcase)
            newPos.Y -= 54;
        else
        {
            tile = Main.tile[pos.X + 1, pos.Y - 1];

            if (tile.HasTile && ModContent.GetModTile(tile.TileType) is Doorcase)
                newPos.Y -= 54;
        }

        orig(self, newPos, Style, extraInfo);
    }

    private void UnsetFlags(On_Wiring.orig_Teleport orig)
    {
        orig();
        _teleportIsDoorcase[0] = false;
        _teleportIsDoorcase[1] = false;
    }

    private void ModifyTeleport(ILContext il)
    {
        ILCursor c = new(il);

        if (!c.TryGotoNext(MoveType.After, x => x.MatchStloc(1)))
            return;

        c.Emit(OpCodes.Ldloca_S, (byte)0);
        c.EmitDelegate(ModifyHitboxArray);
    }

    public static void ModifyHitboxArray(ref Rectangle[] hitboxes)
    {
        if (_teleportIsDoorcase[0])
            ModifyHitbox(ref hitboxes[0]);

        if (_teleportIsDoorcase[1])
            ModifyHitbox(ref hitboxes[1]);
    }

    private static void ModifyHitbox(ref Rectangle rectangle)
    {
        rectangle.X -= 8;
        rectangle.Y -= 16;
        rectangle.Width -= 16;
        rectangle.Height += 64;
    }

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileSolidTop[Type] = false;

        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.IsAMechanism[Type] = true;
        TileID.Sets.IsATrigger[Type] = true;

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

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        if (TryGetEntity(i, j, out var entity))
            entity.Kill(i, j);
    }

    public override bool RightClick(int i, int j)
    {
        Activate(i, j);
        return true;
    }

    private static void Activate(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        Point original = new(i, j);

        if (!TryGetEntity(ref i, ref j, out DoorcaseEntity entity))
            return;

        if (!BookcaseIsOpen(tile))
        {
            ToggleDoorcaseFrames(i, j, null);
            entity.OpenDoor(true);
        }
        else
        {
            if (tile.TileFrameX % 54 == 0)
            {
                ToggleDoorcaseFrames(i, j, null);
                entity.OpenDoor(false);
            }
            else
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                    Wiring.TripWire(original.X, original.Y, 1, 1);
                else
                    SendTripWireHandler.Send(original.X, original.Y, 1, 1);
            }
        }
    }

    private static void TryTeleport(int i, int j)
    {
        Tile tile = Main.tile[i, j];

        if (tile.WallType == WallID.LihzahrdBrickUnsafe && !NPC.downedGolemBoss)
            return;

        if (Wiring._teleport[0].X == -1f)
        {
            Wiring._teleport[0].X = i;
            Wiring._teleport[0].Y = j;
            _teleportIsDoorcase[0] = true;
        }
        else if (Wiring._teleport[0].X != i || Wiring._teleport[0].Y != j)
        {
            Wiring._teleport[1].X = i;
            Wiring._teleport[1].Y = j;
            _teleportIsDoorcase[1] = true;
        }
    }

    public override void HitWire(int i, int j)
    {
        Tile tile = Main.tile[i, j];

        if (!BookcaseIsOpen(tile) && TryGetEntity(i, j, out DoorcaseEntity entity))
        {
            ToggleDoorcaseFrames(i, j, null);
            entity.OpenDoor(true);
        }

        i -= tile.TileFrameX % 54 / 18;
        TryTeleport(i + 1, j - 1);

        for (int x = i; x < i + 3; ++x)
            for (int y = j; y < j + 4; ++y)
                Wiring.SkipWire(x, y);
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
