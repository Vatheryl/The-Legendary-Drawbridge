using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Drawbridge.Players;
using Drawbridge.Items.HorseArmor;

namespace Drawbridge.Systems;

// Draws an armor overlay for horse and unicorn mounts using the same frame/position as the mount sprite
public class HorseArmorDrawLayer : PlayerDrawLayer
{
    // Draw immediately after the mount "back" layer so it appears over the mount and behind the player
    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.MountBack);

    public override bool GetDefaultVisibility(PlayerDrawSet drawInfo)
    {
        Player player = drawInfo.drawPlayer;
        if (!player.mount.Active)
            return false;

        int t = player.mount.Type;
        return DrawbridgePlayer.MountIsHorse(t);
    }

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Player player = drawInfo.drawPlayer;
        int mountType = player.mount.Type;

        Texture2D baseMountTexture = GetBaseMountTexture(mountType);
        if (baseMountTexture == null)
            return;

        // Choose overlay texture based on item in the new dedicated slot
        DrawbridgePlayer mountPlayer = player.GetModPlayer<DrawbridgePlayer>();
        Item slotItem = mountPlayer.horseArmorSlot;

        if (slotItem == null || slotItem.IsAir || slotItem.ModItem is not HorseArmorItem horseArmor)
        {
            slotItem = mountPlayer.horseArmorVanity;

            if (slotItem == null || slotItem.IsAir || slotItem.ModItem is not HorseArmorItem horseVanity)
                return;

            horseArmor = horseVanity;
        }

        Texture2D overlayTexture = ModContent.Request<Texture2D>(horseArmor.OverlayPath).Value;

        if (overlayTexture == null)
            return;

        // Find the mount's DrawData (back texture) that was just added to the cache and mirror its frame/transform
        for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--)
        {
            DrawData dd = drawInfo.DrawDataCache[i];
            if (dd.texture != baseMountTexture)
                continue;

            DrawData overlay = new(
                overlayTexture,
                dd.position,
                dd.sourceRect,
                dd.color,
                dd.rotation,
                dd.origin,
                dd.scale,
                dd.effect
            );
            overlay.shader = mountPlayer.horseArmorDye.dye;
            overlay.ignorePlayerRotation = dd.ignorePlayerRotation;
            drawInfo.DrawDataCache.Add(overlay);
            break;
        }
    }

    private static Texture2D GetBaseMountTexture(int mountType) => mountType switch
    {
        MountID.Unicorn => TextureAssets.UnicornMount.Value,
        MountID.PaintedHorse => TextureAssets.Extra[ExtrasID.PaintedHorseMount].Value,
        MountID.MajesticHorse => TextureAssets.Extra[ExtrasID.MajesticHorseMount].Value,
        MountID.DarkHorse => TextureAssets.Extra[ExtrasID.DarkHorseMount].Value,
        _ => null,
    };
}


