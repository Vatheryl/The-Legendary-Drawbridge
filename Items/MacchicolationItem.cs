using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items;

public class MacchicolationItem : ModItem
{
    public override string Texture => "Drawbridge/Items/Seige_Cauldron_Inv";

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 99;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.createTile = ModContent.TileType<Tiles.MacchicolationTile>();
        Item.placeStyle = 0;
        Item.value = Item.buyPrice(gold: 1);
        Item.rare = ItemRarityID.Blue;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Cauldron)
            .AddIngredient(ItemID.WaterBucket)
            .AddIngredient(ItemID.Campfire)
            .AddIngredient(ItemID.Wood, 20)
            .AddIngredient(ItemID.StoneBlock, 30)
            .AddTile(TileID.HeavyWorkBench)
            .Register();

        CreateRecipe()
            .AddIngredient(ItemID.CookingPot)
            .AddIngredient(ItemID.WaterBucket)
            .AddIngredient(ItemID.Campfire)
            .AddIngredient(ItemID.Wood, 20)
            .AddIngredient(ItemID.StoneBlock, 30)
            .AddTile(TileID.HeavyWorkBench)
            .Register();
    }
}

