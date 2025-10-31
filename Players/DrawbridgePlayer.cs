using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Drawbridge.Tiles;
using Drawbridge.Items;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using System.Collections.Generic;
using Terraria.ModLoader.IO;
using Terraria.Localization;
using Drawbridge;
 

namespace Drawbridge.Players
{
    public class DrawbridgePlayer : ModPlayer
    {
        private Vector2 lastSafePosition;
        
        public bool hasGoldenResonance;
        public bool hasSpiritVeil;

        // Horse armor item equipped in the dedicated slot
        public Item horseArmorSlot = new Item();

        // Per-stable slot storage
        public class StableSlots
        {
            public Item BlessedApple = new Item();
            public Item BlackSaddle = new Item();
            public Item DustySaddle = new Item();
            public Item RoyalSaddle = new Item();
        }
        // Key: stable top-left position
        public Dictionary<Point16, StableSlots> stableSlotsPerStable = new();

        public override void OnEnterWorld()
        {
            lastSafePosition = Player.position;
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                PacketHandler.SendContextPacket(MessageType.ClearSolidTiles, 0);
                int itemType = (horseArmorSlot != null && !horseArmorSlot.IsAir) ? horseArmorSlot.type : 0;
                PacketHandler.SendContextPacket(MessageType.SyncHorseArmorSlot, Player.whoAmI, itemType);
            }
            else if (Main.netMode == NetmodeID.SinglePlayer)
            {
                PacketHandler.ClearSolidTiles();
            }
        }

        public override void PostUpdate()
        {
            // Get the player's tile position
            int tileX = (int)(Player.position.X + Player.width / 2) / 16;
            int tileY = (int)(Player.position.Y + Player.height / 2) / 16;

            var tile = Main.tile[tileX, tileY];
            if (tile != null && tile.HasTile && 
                (tile.TileType == ModContent.TileType<PortcullisDownTile>() || tile.TileType == ModContent.TileType<SpikedPallisadeTile>() || tile.TileType == ModContent.TileType<SpikedPallisadeFlippedTile>()))
            {
                if (tile.IsActuated)
                {
                    lastSafePosition = Player.position;
                    return;
                }
                bool block = false;
                bool touchedSolid = false;
                bool touchedDamage = false;
                string touchedType = "none"; // kept for potential future use
                if (tile.TileType == ModContent.TileType<SpikedPallisadeTile>() || tile.TileType == ModContent.TileType<SpikedPallisadeFlippedTile>())
                {
                    bool isFlipped = tile.TileType == ModContent.TileType<SpikedPallisadeFlippedTile>();
                    bool facingLeft;
                    if (isFlipped)
                    {
                        facingLeft = (tile.TileFrameX < 48);
                    }
                    else
                    {
                        facingLeft = (tile.TileFrameX >= 48);
                    }

                    int frameX = tile.TileFrameX / 16;
                    int frameY = tile.TileFrameY / 16;
                    int topLeftX = tileX - frameX;
                    int topLeftY = tileY - frameY;
                    int localX = (int)(Player.position.X + Player.width / 2) % 48;
                    int localY = (int)(Player.position.Y + Player.height / 2) % 96;
                    if (facingLeft) localX = 47 - localX;
                    int localTileX = localX / 16 + 1; // 1-based
                    int localTileY = localY / 16 + 1; // 1-based
                    // Solid: column 1, row 1, or 3x3 area top-left, plus [2,4] and [3,4]
                    bool isSolid = (localTileX == 1 || localTileY == 1 || (localTileX >= 1 && localTileX <= 3 && localTileY >= 1 && localTileY <= 3) ||
                        (localTileX == 2 && localTileY == 4) || (localTileX == 3 && localTileY == 4));
                    if (isSolid && !tile.IsActuated)
                        block = true;
                    // All solid tiles are also damaging, but only if not actuated
                    if (isSolid && !tile.IsActuated && Player.immuneTime == 0)
                    {
                        string deathReason = Terraria.Localization.Language.GetTextValue("Mods.Drawbridge.DeathReason.SpikedPallisade");
                        Player.Hurt(PlayerDeathReason.ByCustomReason(NetworkText.FromLiteral(deathReason)), 50, 0);
                        Player.AddBuff(BuffID.Bleeding, 600);
                    }
                    if (isSolid && !tile.IsActuated)
                        touchedType = "damage";
                    else if (touchedSolid)
                        touchedType = "solid";
                    else
                        touchedType = "none";
                    // Only show one message per contact event
                    // (debug chat removed)
                }
                else
                {
                    block = true;
                }
                if (block)
                {
                    Player.position = lastSafePosition;
                    Player.velocity = Vector2.Zero;
                }
                else
                {
                    lastSafePosition = Player.position;
                }
            }
            else
            {
                lastSafePosition = Player.position;
            }
        }

        public override void ResetEffects()
        {
            hasGoldenResonance = false;
            hasSpiritVeil = false;
        }

        public override void PostUpdateEquips()
        {
            // Add defense bonus when mounted on horse/unicorn with armor equipped
            if (Player.mount.Active && horseArmorSlot != null && !horseArmorSlot.IsAir)
            {
                int mountType = Player.mount.Type;
                // Check if mounted on horse/unicorn (same types as in IronFieldBardingLayer)
                if (mountType == MountID.Unicorn || mountType == 40 || mountType == 41 || mountType == 42)
                {
                    int defenseBonus = 0;
                    if (horseArmorSlot.type == ModContent.ItemType<IronFieldBardingItem>())
                        defenseBonus = 3; // Iron armor: +3 defense
                    else if (horseArmorSlot.type == ModContent.ItemType<GoldArmorItem>())
                        defenseBonus = 5; // Gold armor: +5 defense
                    else if (horseArmorSlot.type == ModContent.ItemType<MoltenArmorItem>())
                        defenseBonus = 8; // Molten armor: +8 defense

                    Player.statDefense += defenseBonus;
                }
            }
        }

        

        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            if (hasSpiritVeil && modifiers.DamageSource.SourceNPCIndex != -1)
            {
                NPC npc = Main.npc[modifiers.DamageSource.SourceNPCIndex];
                if (npc.type == NPCID.Zombie || npc.type == NPCID.Skeleton)
                {
                    modifiers.FinalDamage *= 0.9f;
                }
            }
        }

        public override void Initialize()
        {
            lastSafePosition = Player.position;
            stableSlotsPerStable = new();
        }

        public override void SaveData(Terraria.ModLoader.IO.TagCompound tag)
        {
            var stableData = new List<Terraria.ModLoader.IO.TagCompound>();
            foreach (var kvp in stableSlotsPerStable)
            {
                var entry = new Terraria.ModLoader.IO.TagCompound
                {
                    ["pos"] = kvp.Key,
                    ["BlessedApple"] = kvp.Value.BlessedApple,
                    ["BlackSaddle"] = kvp.Value.BlackSaddle,
                    ["DustySaddle"] = kvp.Value.DustySaddle,
                    ["RoyalSaddle"] = kvp.Value.RoyalSaddle
                };
                stableData.Add(entry);
            }
            tag["stableSlotsPerStable"] = stableData;
            if (horseArmorSlot != null && !horseArmorSlot.IsAir)
            {
                tag["horseArmorItemType"] = horseArmorSlot.type;
            }
        }

        public override void LoadData(Terraria.ModLoader.IO.TagCompound tag)
        {
            stableSlotsPerStable = new();
            if (tag.ContainsKey("stableSlotsPerStable"))
            {
                var stableData = tag.Get<List<Terraria.ModLoader.IO.TagCompound>>("stableSlotsPerStable");
                foreach (var entry in stableData)
                {
                    var pos = entry.Get<Point16>("pos");
                    var slots = new StableSlots
                    {
                        BlessedApple = entry.Get<Item>("BlessedApple"),
                        BlackSaddle = entry.Get<Item>("BlackSaddle"),
                        DustySaddle = entry.Get<Item>("DustySaddle"),
                        RoyalSaddle = entry.Get<Item>("RoyalSaddle")
                    };
                    stableSlotsPerStable[pos] = slots;
                }
            }
            if (tag.ContainsKey("horseArmorItemType"))
            {
                int t = tag.GetInt("horseArmorItemType");
                horseArmorSlot = new Item();
                if (t != 0)
                {
                    horseArmorSlot.SetDefaults(t);
                    horseArmorSlot.stack = 1;
                }
            }
            else
            {
                horseArmorSlot = new Item();
            }
        }

        public override void CopyClientState(ModPlayer targetCopy)
        {
            var clone = (DrawbridgePlayer)targetCopy;
            if (horseArmorSlot != null && !horseArmorSlot.IsAir)
            {
                if (clone.horseArmorSlot == null)
                    clone.horseArmorSlot = new Item();
                horseArmorSlot.CopyNetStateTo(clone.horseArmorSlot);
            }
            else
            {
                if (clone.horseArmorSlot == null)
                    clone.horseArmorSlot = new Item();
                clone.horseArmorSlot.TurnToAir();
            }
        }

        public override void SendClientChanges(ModPlayer clientPlayer)
        {
            var old = (DrawbridgePlayer)clientPlayer;
            int oldType = (old.horseArmorSlot != null && !old.horseArmorSlot.IsAir) ? old.horseArmorSlot.type : 0;
            int newType = (horseArmorSlot != null && !horseArmorSlot.IsAir) ? horseArmorSlot.type : 0;
            if (oldType != newType && Main.netMode != NetmodeID.SinglePlayer)
            {
                PacketHandler.SendContextPacket(MessageType.SyncHorseArmorSlot, Player.whoAmI, newType);
            }
        }

        public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
        {
            if (Main.netMode == NetmodeID.Server)
            {
                int type = (horseArmorSlot != null && !horseArmorSlot.IsAir) ? horseArmorSlot.type : 0;
                ModPacket p = ModContent.GetInstance<Drawbridge>().GetPacket();
                p.Write((byte)MessageType.SyncHorseArmorSlot);
                p.Write(Player.whoAmI);
                p.Write(type);
                p.Send(toWho, fromWho);
            }
        }
    }
}