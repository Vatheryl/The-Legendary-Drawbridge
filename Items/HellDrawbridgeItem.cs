using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items;

public class HellDrawbridgeItem : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 99;
        Item.useTurn = true;
        Item.autoReuse = false;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.value = Item.sellPrice(gold: 1);
        Item.rare = ItemRarityID.Orange; // Hellstone-themed rarity
    }

    public override void HoldItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            Item.createTile = player.direction == 1
                ? ModContent.TileType<Tiles.Drawbridges.Hell.HellDrawbridgeVerticalFlippedTile>()
                : ModContent.TileType<Tiles.Drawbridges.Hell.HellDrawbridgeTile>();
        }
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Obsidian, 30)
            .AddIngredient(ItemID.Chain, 10)
            .AddIngredient(ItemID.HellstoneBrick, 15)
            .AddTile(TileID.HeavyWorkBench)
            .Register();
    }
}