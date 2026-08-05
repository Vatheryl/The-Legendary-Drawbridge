using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Drawbridge.Players;
using System.Collections.Generic;
using Terraria.Localization;

namespace Drawbridge.Items.HorseArmor;

/// <summary>
/// Defines the base class for horse armor.
/// </summary>
public abstract class HorseArmorItem : ModItem
{
    internal abstract int DefenseBonus { get; }
    internal abstract string OverlayPath { get; }

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;

    public sealed override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = 1;
        Item.useStyle = ItemUseStyleID.None;
        Item.useTime = 10;
        Item.useAnimation = 15;
        Item.consumable = false;
        Item.value = Item.buyPrice(silver: 25);
        Item.rare = ItemRarityID.Blue;
    }

    protected virtual void Defaults() { }

    public override bool CanRightClick() => true;

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        if (!Item.social)
            tooltips.Add(new TooltipLine(Mod, "HorseDefense", Language.GetTextValue("Mods.Drawbridge.Tooltips.HorseDefense", DefenseBonus)));
    }

    public override void RightClick(Player player)
    {
        var modPlayer = player.GetModPlayer<DrawbridgePlayer>();
        Item previous = modPlayer.horseArmorSlot != null && !modPlayer.horseArmorSlot.IsAir ? modPlayer.horseArmorSlot.Clone() : new Item();
        modPlayer.horseArmorSlot = Item.Clone();
        modPlayer.horseArmorSlot.stack = 1;
        SoundEngine.PlaySound(SoundID.Grab);

        if (Main.netMode != NetmodeID.SinglePlayer)
            player.GetModPlayer<DrawbridgePlayer>().SendSyncHorseArmor(-1, -1);

        if (previous != null && !previous.IsAir)
            player.QuickSpawnItem(player.GetSource_Misc("HorseArmorSwap"), previous);

        Item.stack--;

        if (Item.stack <= 0)
            Item.TurnToAir();
    }
}
