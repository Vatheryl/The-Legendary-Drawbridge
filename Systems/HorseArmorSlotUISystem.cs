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

namespace Drawbridge.Systems;

public class HorseArmorSlotUISystem : ModSystem
{
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

        Vector2 slotPos = DetermineSlotPosition();
        Player player = Main.LocalPlayer;
        var modPlayer = player.GetModPlayer<DrawbridgePlayer>();
        Item inv = modPlayer.horseArmorSlot;

        // Handle mouse interactions manually with restrictions
        float equipScale = 0.85f;
        Rectangle slotRect = new((int)slotPos.X, (int)slotPos.Y, (int)(TextureAssets.InventoryBack.Width() * equipScale), (int)(TextureAssets.InventoryBack.Height() * equipScale));
        bool hovering = slotRect.Contains(Main.mouseX, Main.mouseY) && !PlayerInput.IgnoreMouseInterface;

        if (hovering)
        {
            Main.LocalPlayer.mouseInterface = true;
            // Left click to place/pick
            if (Main.mouseLeft && Main.mouseLeftRelease)
            {
                if (Main.mouseItem != null && !Main.mouseItem.IsAir)
                {
                    if (Main.mouseItem.ModItem is HorseArmorItem && (inv == null || inv.IsAir))
                    {
                        modPlayer.horseArmorSlot = Main.mouseItem.Clone();
                        modPlayer.horseArmorSlot.stack = 1;
                        Main.mouseItem.stack--;
                        if (Main.mouseItem.stack <= 0)
                            Main.mouseItem.TurnToAir();
                        Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
                        if (Main.netMode != NetmodeID.SinglePlayer)
                        {
                            int itemTypeSynced = (modPlayer.horseArmorSlot != null && !modPlayer.horseArmorSlot.IsAir) ? modPlayer.horseArmorSlot.type : 0;
                            PacketHandler.SendContextPacket(MessageType.SyncHorseArmorSlot, Main.myPlayer, itemTypeSynced);
                        }
                    }
                }
                else if ((inv != null && !inv.IsAir))
                {
                    Main.mouseItem = inv.Clone();
                    modPlayer.horseArmorSlot = new Item();
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        PacketHandler.SendContextPacket(MessageType.SyncHorseArmorSlot, Main.myPlayer, 0);
                    }
                }
            }

            // Right click to clear
            if (Main.mouseRight && Main.mouseRightRelease)
            {
                if (inv != null && !inv.IsAir && Main.mouseItem.IsAir)
                {
                    Main.mouseItem = inv.Clone();
                    modPlayer.horseArmorSlot = new Item();
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);
                    if (Main.netMode != NetmodeID.SinglePlayer)
                    {
                        PacketHandler.SendContextPacket(MessageType.SyncHorseArmorSlot, Main.myPlayer, 0);
                    }
                }
            }

            // Hover text
            Main.HoverItem = (inv != null && !inv.IsAir) ? inv.Clone() : new Item();
            if (Main.HoverItem.IsAir)
            {
                Main.hoverItemName = "Horse Armor";
            }
        }

        // Ensure local copy reflects any changes from manual interactions
        inv = modPlayer.horseArmorSlot;

        // Draw background exactly like equip slots (uses InventoryBack3) at equip scale
        float prevScale = Main.inventoryScale;
        Main.inventoryScale = equipScale;
        Texture2D backTex = TextureAssets.InventoryBack3.Value;
        spriteBatch.Draw(backTex, slotPos, null, Main.inventoryBack, 0f, Vector2.Zero, Main.inventoryScale, SpriteEffects.None, 0f);
        // Draw item without mount placeholder overlays
        ItemSlot.Draw(spriteBatch, ref inv, 14, slotPos);

        if (inv == null || inv.IsAir)
        {
            Texture2D ghostTex = TextureAssets.Item[ModContent.ItemType<IronFieldBardingItem>()].Value;
            Vector2 backSize = new Vector2(TextureAssets.InventoryBack.Width(), TextureAssets.InventoryBack.Height()) * Main.inventoryScale;
            Vector2 center = slotPos + backSize * 0.5f;
            float maxSide = Math.Max(ghostTex.Width, ghostTex.Height);
            float target = 44f * Main.inventoryScale; // padding inside slot, obey UI scale
            float ghostScale = target / maxSide;
            spriteBatch.Draw(ghostTex, center, null, Color.White * 0.35f, 0f, new Vector2(ghostTex.Width, ghostTex.Height) * 0.5f, ghostScale, SpriteEffects.None, 0f);
        }

        // Persist any changes back to the player's slot
        modPlayer.horseArmorSlot = inv;
        Main.inventoryScale = prevScale;
    }

    private static Vector2 DetermineSlotPosition()
    {
        // Compute the mount slot position as in vanilla, then place our slot just to the right
        int xOffset = Main.screenWidth - 92; // base X for misc equips
        int computedMH = (!Main.mapFullscreen && Main.mapEnabled && Main.mapStyle == 1) ? 256 : 0;
        int yOffset = computedMH + 174; // base Y for misc equips (roughly matches vanilla)
        Vector2 mountSlotTopLeft = new(xOffset, yOffset + 3 * 47);
        Vector2 slotPos = mountSlotTopLeft + new Vector2(47, 0); // Place our slot just to the RIGHT of the mount equip slot; vanilla uses 47px column spacing
        DrawbridgeConfig.HorseMountSlotPosition pos = ModContent.GetInstance<DrawbridgeConfig>().SlotPosition;

        if (pos == DrawbridgeConfig.HorseMountSlotPosition.DownOne)
            slotPos.Y += 48;
        else if (pos == DrawbridgeConfig.HorseMountSlotPosition.NextToBuffs)
        {
            slotPos.X -= 94;
            slotPos.Y += 94;
        }
        else if (pos == DrawbridgeConfig.HorseMountSlotPosition.LeftOfTop)
        {
            slotPos.X -= 47 * 3;
            slotPos.Y -= 47 * 3;
        }
        else if (pos == DrawbridgeConfig.HorseMountSlotPosition.LeftOfBottom)
        {
            slotPos.X -= 47 * 3;
            slotPos.Y += 47;
        }

        return slotPos;
    }
}
