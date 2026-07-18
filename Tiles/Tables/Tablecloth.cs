using Drawbridge.Systems.Synchronization;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;

namespace Drawbridge.Tiles.Tables;

#nullable enable

internal abstract class Tablecloth : ModItem
{
    /// <summary>
    /// Bruteforce handler for placing/removing tablecloth entities.
    /// </summary>
    public class TableclothPlaceHandler : Handler
    {
        public override MessageType Message => MessageType.PlaceTablecloth;

        public static void Send(bool clear, Point16 pos, int type)
        {
            ModPacket packet = PacketHandler.GetPacket(MessageType.PlaceTablecloth);
            packet.Write(clear);
            packet.Write(pos.X);
            packet.Write(pos.Y);

            if (!clear)
                packet.Write(type);

            packet.Send();
        }

        public override void Recieve(int whoAmI, BinaryReader reader, bool onServer)
        {
            bool clear = reader.ReadBoolean();
            Point16 pos = new(reader.ReadInt16(), reader.ReadInt16());

            if (clear)
            {
                ModContent.GetInstance<TablePlatform.TableEntity>().Kill(pos.X, pos.Y);

                if (Main.netMode == NetmodeID.Server)
                    Send(false, pos, 0);

                return;
            }

            int type = reader.ReadInt32();

            if (!TileEntity.ByPosition.TryGetValue(new Point16(pos.X, pos.Y), out TileEntity? te) || te is not TablePlatform.TableEntity table)
            {
                int id = ModContent.GetInstance<TablePlatform.TableEntity>().Place(pos.X, pos.Y);
                table = (TileEntity.ByID[id] as TablePlatform.TableEntity)!;
            }

            table.TableclothType = type;

            if (Main.netMode == NetmodeID.Server)
                Send(false, pos, type);
        }
    }

    public static Dictionary<int, Asset<Texture2D>> PlacedTexturesByType = [];

    public override void SetStaticDefaults()
    {
        PlacedTexturesByType.Add(Type, ModContent.Request<Texture2D>(Texture + "Placed"));
        Item.ResearchUnlockCount = 3;
    }

    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 22;
        Item.value = Item.buyPrice(0, 0, 0, 25);
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 10;
        Item.useAnimation = 10;
        Item.consumable = true;
        Item.rare = ItemRarityID.Blue;
        Item.maxStack = Item.CommonMaxStack;
    }

    public override void HoldItem(Player player)
    {
        if (CanPlaceAt(player, out _, out _))
        {
            player.cursorItemIconEnabled = true;
            player.cursorItemIconID = Type;
        }
    }

    public override bool? UseItem(Player player)
    {
        if (!CanPlaceAt(player, out Point pos, out Tile tile))
            return false;

        int offsetX = tile.TileFrameX % 54 / 18;
        pos.X -= offsetX;
        pos.X++;

        if (!TileEntity.ByPosition.TryGetValue(new Point16(pos.X, pos.Y), out TileEntity? te) || te is not TablePlatform.TableEntity table)
        {
            int id = ModContent.GetInstance<TablePlatform.TableEntity>().Place(pos.X, pos.Y);
            table = (TileEntity.ByID[id] as TablePlatform.TableEntity)!;
        }

        if (table.TableclothType == Type)
            return false;

        table.TableclothType = Type;

        if (Main.netMode == NetmodeID.MultiplayerClient)
            TableclothPlaceHandler.Send(false, new Point16(pos.X, pos.Y), Type);

        return true;
    }

    private static bool CanPlaceAt(Player player, out Point pos, out Tile tile)
    {
        pos = Point.Zero;
        tile = new();

        if (Main.myPlayer != player.whoAmI)
            return false;

        pos = new(Player.tileTargetX, Player.tileTargetY);
        tile = Main.tile[pos];

        if (!tile.HasTile || ModContent.GetModTile(tile.TileType) is not TablePlatform || !player.IsInTileInteractionRange(pos.X, pos.Y, TileReachCheckSettings.Simple))
            return false;

        return true;
    }
}
