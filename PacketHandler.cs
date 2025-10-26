using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Drawbridge.Tiles;
using Terraria.DataStructures;
using Drawbridge.Players;

namespace Drawbridge
{
    internal enum MessageType : byte
    {
        ToggleDrawbridge,
        ClearSolidTiles,
        ToggleGoldBell,
        ToggleGraveyardBell,
        ToggleSpikedPallisade,
        SyncEquineArmatureArmor,
        SyncEquineArmatureFlip,
        SyncHorseArmorSlot,
        HookFishInteract
    }

    // This is a static class for handling packets.
    // It's a central place for all packet logic.
    internal static class PacketHandler
    {
        internal static void ClearSolidTiles()
        {
            int solidTileType = ModContent.TileType<DrawbridgeSolidTile>();
            for (int x = 0; x < Main.maxTilesX; x++)
            {
                for (int y = 0; y < Main.maxTilesY; y++)
                {
                    if (Main.tile[x, y].HasTile && Main.tile[x, y].TileType == solidTileType)
                    {
                        WorldGen.KillTile(x, y, noItem: true);
                    }
                }
            }
        }

        // The main method for sending packets.
        public static void SendPacket(MessageType msg, int context, int context2 = 0)
        {
            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                return; // No need to send packets in single player.
            }

            ModPacket packet = ModContent.GetInstance<Drawbridge>().GetPacket();
            packet.Write((byte)msg);
            packet.Write(context);
            packet.Write(context2);
            packet.Send();
        }

        // The main method for receiving and handling packets.
        public static void HandlePacket(BinaryReader reader, int whoAmI)
        {
            MessageType msg = (MessageType)reader.ReadByte();
            int context = reader.ReadInt32();
            int context2 = reader.ReadInt32();

            switch (msg)
            {
                case MessageType.ToggleDrawbridge:
                    int i = context;
                    int j = context2;
                    Tile tile = Main.tile[i, j];
                    if (tile.HasTile)
                    {
                        // Because we are on the server, we can now call Toggle.
                        // We check which tile type it is and call the appropriate Toggle method.
                        if (tile.TileType == ModContent.TileType<Tiles.DrawbridgeHorizontalTile>())
                        {
                            ModContent.GetInstance<Tiles.DrawbridgeHorizontalTile>().Toggle(i, j);
                        }
                        else if (tile.TileType == ModContent.TileType<Tiles.DrawbridgeTile>())
                        {
                            ModContent.GetInstance<Tiles.DrawbridgeTile>().Toggle(i, j);
                        }
                        else if (tile.TileType == ModContent.TileType<Tiles.PortcullisUpTile>())
                        {
                            ModContent.GetInstance<Tiles.PortcullisUpTile>().Toggle(i, j);
                        }
                        else if (tile.TileType == ModContent.TileType<Tiles.PortcullisDownTile>())
                        {
                            ModContent.GetInstance<Tiles.PortcullisDownTile>().Toggle(i, j);
                        }
                        else if (tile.TileType == ModContent.TileType<Tiles.DrawbridgeHorizontalFlippedTile>())
                        {
                            ModContent.GetInstance<Tiles.DrawbridgeHorizontalFlippedTile>().Toggle(i, j);
                        }
                        else if (tile.TileType == ModContent.TileType<Tiles.DrawbridgeVerticalFlippedTile>())
                        {
                            ModContent.GetInstance<Tiles.DrawbridgeVerticalFlippedTile>().Toggle(i, j);
                        }
                        else if (tile.TileType == ModContent.TileType<Tiles.HellDrawbridgeTile>())
                        {
                            ModContent.GetInstance<Tiles.HellDrawbridgeTile>().Toggle(i, j);
                        }
                        else if (tile.TileType == ModContent.TileType<Tiles.HellDrawbridgeHorizontalTile>())
                        {
                            ModContent.GetInstance<Tiles.HellDrawbridgeHorizontalTile>().Toggle(i, j);
                        }
                        else if (tile.TileType == ModContent.TileType<Tiles.HellDrawbridgeHorizontalFlippedTile>())
                        {
                            ModContent.GetInstance<Tiles.HellDrawbridgeHorizontalFlippedTile>().Toggle(i, j);
                        }
                        else if (tile.TileType == ModContent.TileType<Tiles.HellDrawbridgeVerticalFlippedTile>())
                        {
                            ModContent.GetInstance<Tiles.HellDrawbridgeVerticalFlippedTile>().Toggle(i, j);
                        }
                        else if (tile.TileType == ModContent.TileType<Tiles.LionDrawbridgeTile>())
                        {
                            ModContent.GetInstance<Tiles.LionDrawbridgeTile>().Toggle(i, j);
                        }
                        else if (tile.TileType == ModContent.TileType<Tiles.LionDrawbridgeHorizontalTile>())
                        {
                            ModContent.GetInstance<Tiles.LionDrawbridgeHorizontalTile>().Toggle(i, j);
                        }
                        else if (tile.TileType == ModContent.TileType<Tiles.LionDrawbridgeHorizontalFlippedTile>())
                        {
                            ModContent.GetInstance<Tiles.LionDrawbridgeHorizontalFlippedTile>().Toggle(i, j);
                        }
                        else if (tile.TileType == ModContent.TileType<Tiles.LionDrawbridgeVerticalFlippedTile>())
                        {
                            ModContent.GetInstance<Tiles.LionDrawbridgeVerticalFlippedTile>().Toggle(i, j);
                        }
                    }
                    break;
                case MessageType.ToggleSpikedPallisade:
                    {
                        int pi = context;
                        int pj = context2;
                        Tile ptile = Main.tile[pi, pj];
                        if (ptile.HasTile)
                        {
                            if (ptile.TileType == ModContent.TileType<Tiles.SpikedPallisadeTile>())
                                ModContent.GetInstance<Tiles.SpikedPallisadeTile>().Toggle(pi, pj);
                            else if (ptile.TileType == ModContent.TileType<Tiles.SpikedPallisadeFlippedTile>())
                                ModContent.GetInstance<Tiles.SpikedPallisadeFlippedTile>().Toggle(pi, pj);
                        }
                    }
                    break;
                case MessageType.ToggleGoldBell:
                {
                    int goldBellI = context;
                    int goldBellJ = context2;
                    Tile goldBellTile = Main.tile[goldBellI, goldBellJ];
                    if (goldBellTile.HasTile && goldBellTile.TileType == ModContent.TileType<Tiles.GoldBellTile>())
                    {
                        ModContent.GetInstance<Tiles.GoldBellTile>().TriggerAnimation(goldBellI, goldBellJ);
                    }
                    break;
                }
                case MessageType.ToggleGraveyardBell:
                {
                    int graveBellI = context;
                    int graveBellJ = context2;
                    Tile graveBellTile = Main.tile[graveBellI, graveBellJ];
                    if (graveBellTile.HasTile && graveBellTile.TileType == ModContent.TileType<Tiles.GraveyardBellTile>())
                    {
                        ModContent.GetInstance<Tiles.GraveyardBellTile>().TriggerAnimation(graveBellI, graveBellJ);
                    }
                    break;
                }
                case MessageType.ClearSolidTiles:
                    ClearSolidTiles();
                    break;
                case MessageType.SyncEquineArmatureArmor:
                {
                    int newType = reader.ReadInt32();
                    var pt = new Terraria.DataStructures.Point16(context, context2);
                    int oldType = Systems.EquineArmatureWorld.GetType(pt);
                    Systems.EquineArmatureWorld.SetType(pt, newType);
                    if (Main.netMode == NetmodeID.Server)
                    {
                        Player sender = Main.player[whoAmI];
                        // Handle item consumption/spawn and replacement
                        if (newType == 0)
                        {
                            // Removed overlay: drop the previous item
                            int oldItemType = oldType == 1 ? ModContent.ItemType<Items.IronFieldBardingItem>() : (oldType == 2 ? ModContent.ItemType<Items.GoldArmorItem>() : (oldType == 3 ? ModContent.ItemType<Items.MoltenArmorItem>() : 0));
                            if (oldItemType != 0)
                                Item.NewItem(new EntitySource_TileInteraction(sender, pt.X, pt.Y), pt.X * 16, pt.Y * 16, 16 * 6, 16 * 5, oldItemType);
                        }
                        else
                        {
                            // Equipping/replacing overlay
                            if (oldType != 0)
                            {
                                int oldItemType = oldType == 1 ? ModContent.ItemType<Items.IronFieldBardingItem>() : (oldType == 2 ? ModContent.ItemType<Items.GoldArmorItem>() : (oldType == 3 ? ModContent.ItemType<Items.MoltenArmorItem>() : 0));
                                if (oldItemType != 0)
                                    Item.NewItem(new EntitySource_TileInteraction(sender, pt.X, pt.Y), pt.X * 16, pt.Y * 16, 16 * 6, 16 * 5, oldItemType);
                            }
                            int consumeType = newType == 1 ? ModContent.ItemType<Items.IronFieldBardingItem>() : (newType == 2 ? ModContent.ItemType<Items.GoldArmorItem>() : (newType == 3 ? ModContent.ItemType<Items.MoltenArmorItem>() : 0));
                            if (consumeType != 0)
                                sender.ConsumeItem(consumeType);
                        }

                        ModPacket p = ModContent.GetInstance<Drawbridge>().GetPacket();
                        p.Write((byte)MessageType.SyncEquineArmatureArmor);
                        p.Write(context);
                        p.Write(context2);
                        p.Write(newType);
                        p.Send(-1, whoAmI);
                    }
                    break;
                }
                case MessageType.SyncEquineArmatureFlip:
                {
                    bool flipped = reader.ReadBoolean();
                    var pt = new Terraria.DataStructures.Point16(context, context2);
                    Systems.EquineArmatureWorld.SetFlipped(pt, flipped);
                    if (Main.netMode == NetmodeID.Server)
                    {
                        ModPacket p = ModContent.GetInstance<Drawbridge>().GetPacket();
                        p.Write((byte)MessageType.SyncEquineArmatureFlip);
                        p.Write(context);
                        p.Write(context2);
                        p.Write(flipped);
                        p.Send(-1, whoAmI);
                    }
                    break;
                }
                case MessageType.SyncHorseArmorSlot:
                {
                    int playerId = (Main.netMode == NetmodeID.Server) ? whoAmI : context;
                    int itemType = context2;
                    if (playerId >= 0 && playerId < Main.maxPlayers)
                    {
                        Player target = Main.player[playerId];
                        if (target != null)
                        {
                            var modPl = target.GetModPlayer<DrawbridgePlayer>();
                            Item newItem = new Item();
                            if (itemType != 0)
                            {
                                newItem.SetDefaults(itemType);
                                newItem.stack = 1;
                            }
                            modPl.horseArmorSlot = newItem;

                            if (Main.netMode == NetmodeID.Server)
                            {
                                ModPacket p = ModContent.GetInstance<Drawbridge>().GetPacket();
                                p.Write((byte)MessageType.SyncHorseArmorSlot);
                                p.Write(playerId);
                                p.Write(itemType);
                                p.Send(-1, whoAmI);
                            }
                        }
                    }
                    break;
                }
                case MessageType.HookFishInteract:
                {
                    if (Main.netMode == NetmodeID.Server)
                    {
                        int hi = context;
                        int hj = context2;
                        if (TileEntity.TryGet(hi, hj, out Tiles.HookTileEntity hookTE))
                        {
                            Player player = Main.player[whoAmI];
                            if (hookTE.StoredItemType == 0)
                            {
                                Item held = player.HeldItem;
                                if (held != null && !held.IsAir && Tiles.HookTile.IsFishItemType(held.type))
                                {
                                    hookTE.StoredItemType = held.type;
                                    held.stack--;
                                    if (held.stack <= 0)
                                        held.TurnToAir();
                                }
                            }
                            else
                            {
                                player.QuickSpawnItem(player.GetSource_TileInteraction(hi, hj), hookTE.StoredItemType, 1);
                                hookTE.StoredItemType = 0;
                            }

                            NetMessage.SendData(MessageID.TileEntitySharing, number: hookTE.ID, number2: hookTE.Position.X, number3: hookTE.Position.Y);
                        }
                    }
                    break;
                }
            }
        }
    }
} 