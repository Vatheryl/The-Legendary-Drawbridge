using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.ID;

namespace Drawbridge.NPCs.Haulox;

internal abstract class HauloxArmor : ModItem
{
    public static Dictionary<int, Asset<Texture2D>> ArmorSheetsById = [];

    public override void SetStaticDefaults() => ArmorSheetsById.Add(Type, ModContent.Request<Texture2D>(Texture + "Sheet"));

    public override void SetDefaults()
    {
        Item.width = 60;
        Item.height = 36;
        Item.noUseGraphic = true;
        Item.damage = 0;
        Item.consumable = true;
        Item.noMelee = true;
        Item.rare = ItemRarityID.Blue;
        Item.useTime = Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.Swing;
    }

    public override bool? UseItem(Player player)
    {
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.ModNPC is Haulox haulox && npc.Hitbox.Contains(Main.MouseWorld.ToPoint()) && npc.DistanceSQ(player.Center) < 120 * 120)
            {
                haulox.ArmorId = Type;
                Item.stack--;

                if (Item.stack <= 0)
                    Item.TurnToAir();

                return true;
            }
        }

        return false;
    }

    public override void HoldItem(Player player)
    {
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.ModNPC is Haulox && npc.Hitbox.Contains(Main.MouseWorld.ToPoint()) && npc.DistanceSQ(player.Center) < 120 * 120)
            {
                player.cursorItemIconEnabled = true;
                player.cursorItemIconID = Type;
                break;
            }
        }
    }
}

internal class IronHauloxArmor : HauloxArmor
{
    public override void SetDefaults()
    {
        base.SetDefaults();

        Item.defense = 30;
        Item.value = Item.buyPrice(0, 0, 30, 0);
    }

    public override void AddRecipes() => CreateRecipe().AddRecipeGroup(RecipeGroupID.IronBar, 20).AddTile(TileID.HeavyWorkBench).Register();
}