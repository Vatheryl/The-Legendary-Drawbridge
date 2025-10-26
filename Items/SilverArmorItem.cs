using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Drawbridge.Players;
using Terraria.DataStructures;
using Drawbridge;

namespace Drawbridge.Items
{
    public class SilverArmorItem : ModItem
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
            Item.value = Item.buyPrice(silver: 50);
            Item.rare = ItemRarityID.White;
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
                PacketHandler.SendPacket(MessageType.SyncHorseArmorSlot, player.whoAmI, modPlayer.horseArmorSlot.type);
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
                .AddIngredient(ItemID.SilverBar, 7)
                .AddIngredient(ItemID.Silk, 4)
                .AddIngredient(ModContent.ItemType<CopperArmorItem>(), 1)
                .AddTile(TileID.Anvils)
                .Register();

            CreateRecipe()
                .AddIngredient(ItemID.TungstenBar, 7)
                .AddIngredient(ItemID.Silk, 4)
                .AddIngredient(ModContent.ItemType<CopperArmorItem>(), 1)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
