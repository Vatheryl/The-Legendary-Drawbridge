using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Drawbridge.Players;
using Terraria.DataStructures;
using Drawbridge;

namespace Drawbridge.Items
{
    public class MoltenArmorItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 1;
            Item.useStyle = ItemUseStyleID.None;
            Item.useTime = 10;
            Item.useAnimation = 15;
            Item.consumable = false;
            Item.value = Item.buyPrice(gold: 3);
            Item.rare = ItemRarityID.Orange;
        }

        public override bool CanRightClick()
        {
            return true;
        }

        public override void RightClick(Player player)
        {
            var modPlayer = player.GetModPlayer<DrawbridgePlayer>();
            Item previous = (modPlayer.horseArmorSlot != null && !modPlayer.horseArmorSlot.IsAir) ? modPlayer.horseArmorSlot.Clone() : new Item();
            modPlayer.horseArmorSlot = Item.Clone();
            modPlayer.horseArmorSlot.stack = 1;
            SoundEngine.PlaySound(SoundID.Grab);
            if (Main.netMode != NetmodeID.SinglePlayer)
            {
                PacketHandler.SendContextPacket(MessageType.SyncHorseArmorSlot, player.whoAmI, modPlayer.horseArmorSlot.type);
            }
            if (previous != null && !previous.IsAir)
            {
                player.QuickSpawnItem(player.GetSource_Misc("HorseArmorSwap"), previous);
            }
            Item.stack--;
            if (Item.stack <= 0)
            {
                Item.TurnToAir();
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.HellstoneBar, 12)
                .AddIngredient(ItemID.Obsidian, 5)
                .AddIngredient(ModContent.ItemType<GoldArmorItem>(), 1)
                .AddTile(TileID.Hellforge)
                .Register();
        }
    }
}