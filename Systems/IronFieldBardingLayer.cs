using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Drawbridge.Players;
using Drawbridge.Items;

namespace Drawbridge.Systems
{
    // Draws an armor overlay for horse and unicorn mounts using the same frame/position as the mount sprite
    public class IronFieldBardingLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition()
        {
            // Draw immediately after the mount "back" layer so it appears over the mount and behind the player
            return new AfterParent(PlayerDrawLayers.MountBack);
        }

        public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            if (!player.mount.Active)
                return false;

            int t = player.mount.Type;
            return t == MountID.Unicorn || t == 40 || t == 41 || t == 42; // Unicorn + 3 horse variants
        }

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            Player player = drawInfo.drawPlayer;
            int mountType = player.mount.Type;

            Texture2D baseMountTexture = GetBaseMountTexture(mountType);
            if (baseMountTexture == null)
                return;

            // Choose overlay texture based on item in the new dedicated slot
            Item slotItem = player.GetModPlayer<DrawbridgePlayer>().horseArmorSlot;
            if (slotItem == null || slotItem.IsAir)
                return;
            string overlayPath = null;
            if (slotItem.type == ModContent.ItemType<CopperArmorItem>())
                overlayPath = "Drawbridge/Textures/Copper_Amor";
            else if (slotItem.type == ModContent.ItemType<SilverArmorItem>())
                overlayPath = "Drawbridge/Textures/Silver_Armor";
            else if (slotItem.type == ModContent.ItemType<IronFieldBardingItem>())
                overlayPath = "Drawbridge/Textures/IronFieldBarding";
            else if (slotItem.type == ModContent.ItemType<GoldArmorItem>())
                overlayPath = "Drawbridge/Textures/Gold_Armor";
            else if (slotItem.type == ModContent.ItemType<MoltenArmorItem>())
                overlayPath = "Drawbridge/Textures/Molten_Horse_Armor";
            if (overlayPath == null)
                return;
            Texture2D overlayTexture = ModContent.Request<Texture2D>(overlayPath).Value;
            if (overlayTexture == null)
                return;

            // Find the mount's DrawData (back texture) that was just added to the cache and mirror its frame/transform
            for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--)
            {
                DrawData dd = drawInfo.DrawDataCache[i];
                if (dd.texture != baseMountTexture)
                    continue;

                DrawData overlay = new DrawData(
                    overlayTexture,
                    dd.position,
                    dd.sourceRect,
                    dd.color,
                    dd.rotation,
                    dd.origin,
                    dd.scale,
                    dd.effect
                );
                overlay.shader = Mount.currentShader; // respect mount dye
                overlay.ignorePlayerRotation = dd.ignorePlayerRotation;
                drawInfo.DrawDataCache.Add(overlay);
                break;
            }
        }

        private static Texture2D GetBaseMountTexture(int mountType)
        {
            switch (mountType)
            {
                case MountID.Unicorn:
                    return TextureAssets.UnicornMount.Value;
                case 40: // PaintedHorse
                    return TextureAssets.Extra[161].Value;
                case 41: // MajesticHorse
                    return TextureAssets.Extra[162].Value;
                case 42: // DarkHorse
                    return TextureAssets.Extra[163].Value;
                default:
                    return null;
            }
        }
    }
}


