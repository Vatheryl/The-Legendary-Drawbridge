using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drawbridge.Items
{
    // Base class for all sign items
    public abstract class BaseSignItem : ModItem
    {
        protected abstract int TileType { get; }
        public virtual int SignValue => Item.buyPrice(copper: 50);

        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 52;
            Item.maxStack = 99;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = TileType;
            Item.placeStyle = 0;
            Item.value = SignValue;
            Item.rare = ItemRarityID.White;
        }
    }

    public class Fish_Sign_Item : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.Fish_Sign_Placed>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 8)
                .AddIngredient(ItemID.Bass, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class BlankSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.BlankSignTile>();

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 8)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class AnvilSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.AnvilSignTile>();
        // Sold by Merchant
    }

    public class ClothierSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.ClothierSignTile>();
        // Sold by Clothier
    }

    public class CyberSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.CyberSignTile>();
        // Sold by Cyborg
    }

    public class DemolitionistSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.DemolitionistSignTile>();
        // Sold by Demolitionist
    }

    public class DryadSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.DryadSignTile>();
        // Sold by Dryad

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 8)
                .AddIngredient(ItemID.DaybloomSeeds, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class DyeSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.DyeSignTile>();
        // Sold by Dye Trader
    }

    public class GolfSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.GolfSignTile>();
        // Sold by Golfer
    }

    public class GunSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.GunSignTile>();
        // Sold by Arms Dealer
    }

    public class MechanicSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.MechanicSignTile>();
        // Sold by Mechanic
    }

    public class MerchantSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.MerchantSignTile>();
        // Sold by Merchant
    }

    public class PirateSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.PirateSignTile>();
        // Sold by Pirate
    }

    public class PrincessSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.PrincessSignTile>();
        // Princess doesn't have a shop - make it craftable

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 8)
                .AddIngredient(ItemID.PinkGel, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class SteamSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.SteamSignTile>();
        // Sold by Steampunker
    }

    public class StylistSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.StylistSignTile>();
        // Stylist doesn't have a shop - make it craftable

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.Wood, 8)
                .AddIngredient(ItemID.Silk, 1)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }

    public class TinkererSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.TinkererSignTile>();
        // Sold by Goblin Tinkerer
    }

    public class TruffleSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.TruffleSignTile>();
        // Sold by Truffle
    }

    public class ZooSignItem : BaseSignItem
    {
        protected override int TileType => ModContent.TileType<Tiles.ZooSignTile>();
        // Sold by Zoologist
    }
}
