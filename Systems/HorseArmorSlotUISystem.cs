using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Drawbridge.Players;
using Drawbridge.Items.HorseArmor;
using Terraria.Audio;
using Terraria.Localization;

namespace Drawbridge.Systems;

public class HorseArmorSlotUISystem : ModSystem
{
    private static Asset<Texture2D> HorseSlotIcon = ModContent.Request<Texture2D>("Drawbridge/Textures/HorseSlotIcon");

	public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
	{
		int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		if (mouseTextIndex != -1)
		{
			layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
				"Drawbridge: Horse Armor Slot",
				delegate
				{
					DrawHorseArmorSlot(Main.spriteBatch);
					return true;
				},
				InterfaceScaleType.UI)
			);
		}
	}

	private static void DrawHorseArmorSlot(SpriteBatch spriteBatch)
    {
        if (!Main.playerInventory || Main.EquipPage != 2)
            return;

        Vector2 slotPos = DetermineSlotPosition(0);
        Vector2 vanityPos = DetermineSlotPosition(1);
        Vector2 dyePos = DetermineSlotPosition(2);
        Player player = Main.LocalPlayer;
        var modPlayer = player.GetModPlayer<DrawbridgePlayer>();
        Item horseArmorSlot = modPlayer.horseArmorSlot;
        Item vanityArmorSlot = modPlayer.horseArmorVanity;
        Item dyeArmorSlot = modPlayer.horseArmorDye;

        // Handle mouse interactions manually with restrictions
        float equipScale = 0.85f;
        Vector2 slotSize = TextureAssets.InventoryBack.Size();
        Rectangle slotRect = new((int)slotPos.X, (int)slotPos.Y, (int)(slotSize.X * equipScale), (int)(slotSize.Y * equipScale));
        Rectangle vanityRect = new((int)vanityPos.X, (int)vanityPos.Y, (int)(slotSize.X * equipScale), (int)(slotSize.Y * equipScale));
        Rectangle dyeRect = new((int)dyePos.X, (int)dyePos.Y, (int)(slotSize.X * equipScale), (int)(slotSize.Y * equipScale));
        bool hovering = slotRect.Contains(Main.mouseX, Main.mouseY) && !PlayerInput.IgnoreMouseInterface;
        bool vanityHovering = vanityRect.Contains(Main.mouseX, Main.mouseY) && !PlayerInput.IgnoreMouseInterface;
        bool dyeHovering = dyeRect.Contains(Main.mouseX, Main.mouseY) && !PlayerInput.IgnoreMouseInterface;

        if (hovering)
            CustomItemHandler(ref horseArmorSlot, ref modPlayer.horseArmorSlot, "HorseArmor", 0);

        if (vanityHovering)
            CustomItemHandler(ref vanityArmorSlot, ref modPlayer.horseArmorVanity, "HorseVanity", 1);

        if (dyeHovering)
            CustomItemHandler(ref dyeArmorSlot, ref modPlayer.horseArmorDye, "HorseDye", 2);

        // Ensure local copy reflects any changes from manual interactions
        horseArmorSlot = modPlayer.horseArmorSlot;

        // Draw background exactly like equip slots (uses InventoryBack3) at equip scale
        float prevScale = Main.inventoryScale;
        Main.inventoryScale = equipScale;
        Texture2D backTex = TextureAssets.InventoryBack3.Value;
        spriteBatch.Draw(backTex, slotPos, null, Main.inventoryBack, 0f, Vector2.Zero, Main.inventoryScale, SpriteEffects.None, 0f);
        
        spriteBatch.Draw(TextureAssets.InventoryBack12.Value, dyePos, null, Main.inventoryBack, 0f, Vector2.Zero, Main.inventoryScale, SpriteEffects.None, 0f);
        spriteBatch.Draw(TextureAssets.InventoryBack8.Value, vanityPos, null, Main.inventoryBack, 0f, Vector2.Zero, Main.inventoryScale, SpriteEffects.None, 0f);

        // Draw item without mount placeholder overlays
        ItemSlot.Draw(spriteBatch, ref horseArmorSlot, 14, slotPos);
        ItemSlot.Draw(spriteBatch, ref vanityArmorSlot, 14, vanityPos);
        ItemSlot.Draw(spriteBatch, ref dyeArmorSlot, 14, dyePos);

        if (horseArmorSlot == null || horseArmorSlot.IsAir)
            DrawIcon(spriteBatch, slotPos, HorseSlotIcon.Value, null);

        if (vanityArmorSlot == null || vanityArmorSlot.IsAir)
            DrawIcon(spriteBatch, vanityPos, TextureAssets.Extra[ExtrasID.EquipIcons].Value, new Rectangle(66, 0, 36, 36));

        if (dyeArmorSlot == null || dyeArmorSlot.IsAir)
            DrawIcon(spriteBatch, dyePos, TextureAssets.Extra[ExtrasID.EquipIcons].Value, new Rectangle(32, -2, 34, 34));

        // Persist any changes back to the player's slot
        modPlayer.horseArmorSlot = horseArmorSlot;
        Main.inventoryScale = prevScale;
    }

    private static void DrawIcon(SpriteBatch spriteBatch, Vector2 slotPos, Texture2D ghostTex, Rectangle? src)
    {
        Vector2 backSize = new Vector2(TextureAssets.InventoryBack.Width(), TextureAssets.InventoryBack.Height()) * Main.inventoryScale;
        Vector2 center = slotPos + backSize * 0.5f;
        src ??= ghostTex.Bounds;
        float maxSide = Math.Max(src.Value.Width, src.Value.Height);
        float target = 34f * Main.inventoryScale; // padding inside slot, obey UI scale
        float ghostScale = target / maxSide;
        spriteBatch.Draw(ghostTex, center, src, Color.White * 0.35f, 0f, new Vector2(src.Value.Width, src.Value.Height) * 0.5f, ghostScale, SpriteEffects.None, 0f);
    }

    private static void CustomItemHandler(ref Item horseArmorSlot, ref Item placedSlot, string hoverKey, int slotType)
    {
        Main.LocalPlayer.mouseInterface = true;

        // Left click to place/pick
        if (Main.mouseLeft && Main.mouseLeftRelease)
        {
            if (Main.mouseItem != null && !Main.mouseItem.IsAir)
            {
                bool validForSlot = slotType == 2 ? Main.mouseItem.dye > 0 : Main.mouseItem.ModItem is HorseArmorItem;
                if (validForSlot && (horseArmorSlot == null || horseArmorSlot.IsAir))
                {
                    placedSlot = Main.mouseItem.Clone();
                    placedSlot.stack = 1;
                    Main.mouseItem.stack--;
                    
                    if (Main.mouseItem.stack <= 0)
                        Main.mouseItem.TurnToAir();
                    
                    SoundEngine.PlaySound(SoundID.Grab);

                    if (Main.netMode != NetmodeID.SinglePlayer)
                        Main.LocalPlayer.GetModPlayer<DrawbridgePlayer>().SendSyncHorseArmor(-1, -1);
                }
            }
            else if ((horseArmorSlot != null && !horseArmorSlot.IsAir))
            {
                Main.mouseItem = horseArmorSlot.Clone();
                placedSlot = new Item();
                SoundEngine.PlaySound(SoundID.Grab);

                if (Main.netMode != NetmodeID.SinglePlayer)
                    Main.LocalPlayer.GetModPlayer<DrawbridgePlayer>().SendSyncHorseArmor(-1, -1);
            }
        }

        // Right click to clear
        if (Main.mouseRight && Main.mouseRightRelease)
        {
            if (horseArmorSlot != null && !horseArmorSlot.IsAir && Main.mouseItem.IsAir)
            {
                Main.mouseItem = horseArmorSlot.Clone();
                placedSlot = new Item();
                SoundEngine.PlaySound(SoundID.Grab);
                
                if (Main.netMode != NetmodeID.SinglePlayer)
                    Main.LocalPlayer.GetModPlayer<DrawbridgePlayer>().SendSyncHorseArmor(-1, -1);
            }
        }

        // Hover text
        Main.HoverItem = (horseArmorSlot != null && !horseArmorSlot.IsAir) ? horseArmorSlot.Clone() : new Item();
        Main.HoverItem.social = true;

        if (Main.HoverItem.IsAir)
            Main.hoverItemName = Language.GetTextValue("Mods.Drawbridge.Slots." + hoverKey);
        else if (slotType == 1)
            Main.hoverItemName = Lang.tip[0].Value;
    }

    private static Vector2 DetermineSlotPosition(int slotType)
    {
        const int SlotSize = 47;

        // Compute the mount slot position as in vanilla, then place our slot just to the right
        int xOffset = Main.screenWidth - 92; // base X for misc equips
        int computedMH = (!Main.mapFullscreen && Main.mapEnabled && Main.mapStyle == 1) ? 256 : 0;
        int yOffset = computedMH + 174; // base Y for misc equips (roughly matches vanilla)
        Vector2 mountSlotTopLeft = new(xOffset, yOffset + 3 * SlotSize);
        Vector2 slotPos = mountSlotTopLeft + new Vector2(SlotSize, 0); // Place our slot just to the RIGHT of the mount equip slot; vanilla uses 47px column spacing
        DrawbridgeConfig.HorseMountSlotPosition pos = ModContent.GetInstance<DrawbridgeConfig>().SlotPosition;

        if (pos == DrawbridgeConfig.HorseMountSlotPosition.DownOne)
            slotPos.Y += SlotSize;
        else if (pos == DrawbridgeConfig.HorseMountSlotPosition.NextToBuffs)
        {
            slotPos.X -= SlotSize * 2;
            slotPos.Y += SlotSize * 2;
        }
        else if (pos == DrawbridgeConfig.HorseMountSlotPosition.LeftOfTop)
        {
            slotPos.X -= SlotSize * 3;
            slotPos.Y -= SlotSize * 3;
        }
        else if (pos == DrawbridgeConfig.HorseMountSlotPosition.LeftOfBottom)
        {
            slotPos.X -= SlotSize * 3;
            slotPos.Y += SlotSize;
        }

        if (slotType == 1)
        {
            if (pos is DrawbridgeConfig.HorseMountSlotPosition.DownOne or DrawbridgeConfig.HorseMountSlotPosition.Default)
                slotPos.Y += SlotSize;
            else
                slotPos.X -= SlotSize;
        }
        else if (slotType == 2)
        {
            if (pos is DrawbridgeConfig.HorseMountSlotPosition.DownOne or DrawbridgeConfig.HorseMountSlotPosition.Default)
                slotPos.Y += SlotSize * 2;
            else
                slotPos.X -= SlotSize * 2;
        }

        return slotPos;
    }
}
