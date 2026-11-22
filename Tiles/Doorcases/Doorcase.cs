using Drawbridge.Common;
using Drawbridge.Systems.Synchronization;
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

/// <summary>
/// Marker interface for doorcase tiles.
/// </summary>
internal interface IDoorcase
{
}

public static class DoorcaseHelper
{
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

    public static bool BookcaseIsOpen(Tile tile) => tile.TileFrameX >= 54;
}

public class DoorcaseEntity : ModTileEntity, IClientSideTE
{
    internal class DoorcaseSyncHandler : Handler
    {
        public override MessageType Message => MessageType.DoorcaseSync;

        public static void Send(int id, bool open)
        {
            ModPacket packet = PacketHandler.GetPacket(MessageType.DoorcaseSync);
            packet.Write((ushort)id);
            packet.Write(open);
            packet.Send();
        }

        public override void Recieve(int whoAmI, BinaryReader reader, bool onServer)
        {
            ushort id = reader.ReadUInt16();
            bool open = reader.ReadBoolean();

            if (ByID[id] is DoorcaseEntity ent)
            {
                ent.DoorOpen = open;
                Point16 pos = ent.Position;

                if (DoorcaseHelper.BookcaseIsOpen(Main.tile[pos]) != open)
                    DoorcaseHelper.ToggleDoorcaseFrames(pos.X, pos.Y, open);

                NetMessage.SendData(MessageID.TileEntitySharing, -1, -1, null, id);
            }
        }
    }

    public float DoorProgress = 0;
    public bool DoorOpen { get; private set; }

    public override bool IsTileValidForEntity(int x, int y)
    {
        Tile tile = Main.tile[x, y];
        return tile.HasTile && ModContent.GetModTile(tile.TileType) is IDoorcase;
    }

    public void OpenDoor(bool newValue)
    {
        DoorOpen = newValue;

        if (Main.netMode != NetmodeID.SinglePlayer)
            DoorcaseSyncHandler.Send(ID, DoorOpen);
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

internal abstract class DoorcaseItem<T> : ModItem where T : ModTile, IDoorcase
{
    public abstract int Material { get; }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<T>());
        Item.value = Item.buyPrice(silver: 50);
        Item.rare = ItemRarityID.Blue;
        Item.mech = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe().AddIngredient(ItemID.Book, 8).AddIngredient(ItemID.Wire, 8).AddIngredient(Material, 20).AddIngredient(ItemID.FallenStar, 3)
            .AddTile(TileID.HeavyWorkBench).Register();
    }
}

internal abstract class Doorcase<T> : ModTile, IDoorcase where T : ModItem
{
    private static readonly bool[] _teleportIsDoorcase = [false, false];

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

        if (tile.HasTile && ModContent.GetModTile(tile.TileType) is IDoorcase)
            newPos.Y -= 54;
        else
        {
            tile = Main.tile[pos.X + 1, pos.Y - 1];

            if (tile.HasTile && ModContent.GetModTile(tile.TileType) is IDoorcase)
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
        Main.tileSolidTop[Type] = true;

        TileID.Sets.DisableSmartCursor[Type] = true;
        TileID.Sets.IsAMechanism[Type] = true;
        TileID.Sets.IsATrigger[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
        TileObjectData.newTile.StyleWrapLimitVisualOverride = 1;
        TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<DoorcaseEntity>().Generic_HookPostPlaceMyPlayer;
        TileObjectData.addTile(Type);

        AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
        AddMapEntry(new Color(120, 85, 60), CreateMapEntryName());
        RegisterItemDrop(ModContent.ItemType<T>());

        AdjTiles = [TileID.Bookcases];
        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        if (DoorcaseHelper.TryGetEntity(i, j, out var entity))
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

        if (!DoorcaseHelper.TryGetEntity(ref i, ref j, out DoorcaseEntity entity))
            return;

        if (!DoorcaseHelper.BookcaseIsOpen(tile))
        {
            DoorcaseHelper.ToggleDoorcaseFrames(i, j, null);
            entity.OpenDoor(true);
        }
        else
        {
            if (tile.TileFrameX % 54 == 0)
            {
                DoorcaseHelper.ToggleDoorcaseFrames(i, j, null);
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

        if (!DoorcaseHelper.BookcaseIsOpen(tile) && DoorcaseHelper.TryGetEntity(i, j, out DoorcaseEntity entity))
        {
            DoorcaseHelper.ToggleDoorcaseFrames(i, j, null);
            entity.OpenDoor(true);
        }

        i -= tile.TileFrameX % 54 / 18;
        TryTeleport(i + 1, j - 1);

        for (int x = i; x < i + 3; ++x)
            for (int y = j; y < j + 4; ++y)
                Wiring.SkipWire(x, y);
    }

    public override void MouseOver(int i, int j)
    {
        Tile tile = Main.tile[i, j];
        bool book = !DoorcaseHelper.BookcaseIsOpen(tile) || tile.TileFrameX % 54 == 0;
        
        if (book)
        {
            Main.LocalPlayer.cursorItemIconEnabled = true;
            Main.LocalPlayer.cursorItemIconID = ItemID.Book;
        }
    }

    public override bool PreDrawPlacementPreview(int i, int j, SpriteBatch spriteBatch, ref Rectangle frame, ref Vector2 position, ref Color color, bool validPlacement, ref SpriteEffects fx)
    {
        frame.X += 18;
        return true;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        const int FrameHeight = 72;

        if (!DoorcaseHelper.TryGetEntity(i, j, out DoorcaseEntity entity))
            return false;
        
        Tile tile = Main.tile[i, j];
        Texture2D texture = TextureAssets.Tile[Type].Value;
        Vector2 position = new Vector2(i, j).ToWorldCoordinates(0, 0) + TileExtensions.TileOffset - Main.screenPosition;
        int frameX = tile.TileFrameX % 54 + 18;
        int frameY = tile.TileFrameY + (int)(entity.DoorProgress * 9) * FrameHeight;

        spriteBatch.Draw(texture, position, new Rectangle(frameX, frameY, 16, 16), Lighting.GetColor(i, j));
        return false;
    }
}
