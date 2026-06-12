using Drawbridge.Common;
using Drawbridge.Common.Tiles;
using Drawbridge.NPCs.Haulox;
using Drawbridge.Systems.Synchronization;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace Drawbridge.Tiles;

internal class StakeTile : ModTile, IAutoloadTileItem
{
    public class StakeTileEntity : ModTileEntity, IDrawableTE
    {
        public record struct HauloxData(Haulox.Variants Variant, int Yoke, int NPC);

        Point IDrawableTE.Size => new(16, 32);

        public bool HasData = false;

        private HauloxData Data = default;

        public ref HauloxData GetData(out bool value)
        {
            value = HasData;
            return ref Data;
        }

        public void SetData(HauloxData? data)
        {
            if (data is null)
            {
                HasData = false;
                return;
            }

            HasData = true;
            Data = data.Value;
        }

        public override void Update()
        {
            ref HauloxData data = ref GetData(out bool hasValue);

            if (hasValue)
            {
                if (data.NPC == -2)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        data.NPC = NPC.NewNPC(new EntitySource_SpawnNPC(), Position.X * 16, (Position.Y - 1) * 16, ModContent.NPCType<Haulox>(), 0, (float)data.Variant);
                        (Main.npc[data.NPC].ModNPC as Haulox).Variant = data.Variant;
                        SetHauloxStaked(data.Yoke, this, Main.npc[data.NPC]);

                        if (Main.netMode == NetmodeID.Server)
                            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, data.NPC);
                    }
                    else
                        return;
                }

                NPC npc = Main.npc[data.NPC];

                if (!npc.active || npc.ModNPC is not Haulox)
                    SetData(null);
            }
        }

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<StakeTile>();
        }

        public override void SaveData(TagCompound tag)
        {
            ref HauloxData data = ref GetData(out bool hasValue);

            if (hasValue)
            {
                TagCompound dataTag = [];
                dataTag.Add("variant", (byte)data.Variant);
                string value = SavingUtils.IOType<ModItem>(data.Yoke, ItemID.Count);
                dataTag.Add("yoke", value);
                tag.Add("data", dataTag);
            }
        }

        public override void LoadData(TagCompound tag)
        {
            if (tag.TryGet("data", out TagCompound data))
            {
                SetData(new((Haulox.Variants)data.GetByte("variant"), SavingUtils.GetTypeFromIO<ModItem>(data.GetString("yoke")), -2));
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(HasData);

            if (HasData)
            {
                writer.Write((byte)Data.Variant);
                writer.Write((ushort)Data.Yoke);
                writer.Write((short)Data.NPC);
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            HasData = reader.ReadBoolean();

            if (HasData)
                Data = new HauloxData((Haulox.Variants)reader.ReadByte(), reader.ReadUInt16(), reader.ReadInt16());
        }

        void IDrawableTE.Draw(SpriteBatch spriteBatch)
        {
            ref HauloxData data = ref GetData(out bool hasValue);

            if (!hasValue || data.NPC < 0)
                return;

            Vector2 origin = Position.ToWorldCoordinates(4, 12);
            List<DrawData> datas = YokeItem.YokeLayer.DrawYokeConnection(origin, Main.npc[data.NPC], ref origin);

            foreach (DrawData d in datas)
                d.Draw(spriteBatch);
        }
    }

    public class StakeHauloxHandler : Handler
    {
        public override MessageType Message => MessageType.StakeHaulox;

        public static void Send(short yokeType, short npcWho, Point16 location)
        {
            ModPacket packet = PacketHandler.GetPacket(MessageType.StakeHaulox, 9);
            packet.Write(yokeType);
            packet.Write(npcWho);
            packet.Write(location.X);
            packet.Write(location.Y);
            packet.Send();
        }

        public override void Recieve(int whoAmI, BinaryReader reader, bool onServer)
        {
            short yokeType = reader.ReadInt16();
            short npcWho = reader.ReadInt16();
            Point16 location = new(reader.ReadInt16(), reader.ReadInt16());

            SetHauloxStaked(yokeType, (StakeTileEntity)TileEntity.ByPosition[location], Main.npc[npcWho]);

            if (Main.dedServ)
                Send(yokeType, npcWho, location);
        }
    }

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileSolidTop[Type] = false;
        
        TileID.Sets.DisableSmartCursor[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.Width = 1;
        TileObjectData.newTile.Height = 2;
        TileObjectData.newTile.Origin = new Point16(0, 1); // top-center for hanging
        TileObjectData.newTile.CoordinateHeights = [16, 16]; 
        TileObjectData.newTile.HookPostPlaceMyPlayer = ModContent.GetInstance<StakeTileEntity>().Generic_HookPostPlaceMyPlayer;
        TileObjectData.newTile.DrawYOffset = 6;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(191, 142, 111), CreateMapEntryName());
        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;
    }

    public override bool RightClick(int i, int j)
    {
        Player p = Main.LocalPlayer;
        Point16 topLeft = TileObjectData.TopLeft(i, j);
        ref int attachedNPC = ref p.GetModPlayer<YokeItem.YokePlayer>().attachedNPC;
        
        if (p.HeldItem.ModItem is YokeItem y && y.attached && attachedNPC >= 0 && TileEntity.ByPosition[topLeft] is StakeTileEntity te)
        {
            NPC haul = Main.npc[attachedNPC];
            SetHauloxStaked(y.Type, te, haul);

            p.HeldItem.stack--;

            if (p.HeldItem.stack < 1)
                p.HeldItem.TurnToAir();

            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                StakeHauloxHandler.Send((short)y.Type, (short)haul.whoAmI, te.Position);
                YokeItem.YokePlayerHandler.SendDetatch();
            }

            attachedNPC = -1;
        }

        return false;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY) => ModContent.GetInstance<StakeTileEntity>().Kill(i, j);

    private static void SetHauloxStaked(int yokeType, StakeTileEntity te, NPC haul)
    {
        var ox = haul.ModNPC as Haulox;
        ox.GuidingPlayer = -1;
        ox.StakePosition = te.Position;
        ox.AttachedYokeId = -1;
        ox.SetState(Haulox.States.Staked);
        ox.Staked = true;
        te.SetData(new(ox.Variant, yokeType, haul.whoAmI));
    }
}
