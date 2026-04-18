using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Drawbridge.NPCs.Haulox;

internal abstract class YokeItem : ModItem
{
    public class YokePlayer : ModPlayer
    {
        public int attachedNPC = -1;
    }

    public Haulox.Variants? storedVariant = null;
    public int tiedNPC = -1;

    public override void SetDefaults()
    {
        Item.Size = new Vector2(46, 38);
        Item.useTime = Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.rare = ItemRarityID.Green;
    }

    public override void RightClick(Player player)
    {
        if (storedVariant is null)
            return;
    }

    public override bool? UseItem(Player player)
    {
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.ModNPC is Haulox haulox)
            {
                storedVariant = haulox.Variant;
                player.GetModPlayer<YokePlayer>().attachedNPC = haulox.NPC.whoAmI;
            }
        }

        return false;
    }

    public override void SaveData(TagCompound tag) => tag.Add("variantIfAny", (short)((int?)storedVariant ?? -1));

    public override void LoadData(TagCompound tag)
    {
        if (tag.TryGet("variantIfAny", out short value))
        {
            if (value == -1)
                storedVariant = (Haulox.Variants)value;
            else
                storedVariant = null;
        }
    }
}
