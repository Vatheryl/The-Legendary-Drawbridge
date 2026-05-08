using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Drawbridge.NPCs.Haulox;

internal abstract class YokeItem : ModItem
{
    public class YokePlayer : ModPlayer
    {
        public int attachedNPC = -1;

        public override void PreUpdate()
        {
            if (attachedNPC != -1 && Main.npc[attachedNPC] is { active: false } or { ModNPC: not Haulox })
            {
                attachedNPC = -1;
            }
        }

        public override bool CanHitNPC(NPC target) => target.whoAmI != attachedNPC;
    }

    public class YokeLayer : PlayerDrawLayer
    {
        public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.Head);

        protected override void Draw(ref PlayerDrawSet drawInfo)
        {
            if (Main.gameMenu)
                return;

            Player plr = drawInfo.drawPlayer;
            int attach = plr.GetModPlayer<YokePlayer>().attachedNPC;

            if (drawInfo.shadow > 0 || attach < 0)
                return;

            NPC haulox = Main.npc[attach];

            Vector2 lineOrigin = plr.MountedCenter;
            lineOrigin.Y += plr.gfxOffY;
            //This variable is used to account for Gravitation Potions
            float gravity = plr.gravDir;

            if (gravity == -1f)
                lineOrigin.Y -= 12f;

            drawInfo.DrawDataCache.AddRange(DrawYokeConnection(plr.RotatedRelativePoint(lineOrigin + new Vector2(8f), true) - new Vector2(8f), haulox, ref lineOrigin));
        }

        internal static List<DrawData> DrawYokeConnection(Vector2 origin, NPC haulox, ref Vector2 lineOrigin)
        {
            Vector2 hauloxCenter = haulox.Center;

            lineOrigin = origin;
            Vector2 playerToProjectile = hauloxCenter - lineOrigin;
            bool canDraw = true;

            if (playerToProjectile.X == 0f && playerToProjectile.Y == 0f)
                return null;

            float playerToProjectileMagnitude = playerToProjectile.Length();
            playerToProjectileMagnitude = 12f / playerToProjectileMagnitude;
            playerToProjectile *= playerToProjectileMagnitude;
            lineOrigin -= playerToProjectile;
            playerToProjectile = hauloxCenter - lineOrigin;

            List<DrawData> values = [];

            while (canDraw)
            {
                float height = 12f;
                float positionMagnitude = playerToProjectile.Length();

                if (float.IsNaN(positionMagnitude))
                    break;

                if (positionMagnitude < 20f)
                {
                    height = positionMagnitude - 8f;
                    canDraw = false;
                }

                playerToProjectile *= 12f / positionMagnitude;
                lineOrigin += playerToProjectile;
                playerToProjectile.X = hauloxCenter.X - lineOrigin.X;
                playerToProjectile.Y = hauloxCenter.Y - lineOrigin.Y;

                if (positionMagnitude > 12f)
                {
                    float positionInverseMultiplier = 0.3f;
                    float absVelocitySum = Math.Abs(haulox.velocity.X) + Math.Abs(haulox.velocity.Y);

                    if (absVelocitySum > 16f)
                        absVelocitySum = 16f;

                    absVelocitySum = 1f - absVelocitySum / 16f;
                    positionInverseMultiplier *= absVelocitySum;
                    absVelocitySum = positionMagnitude / 80f;

                    if (absVelocitySum > 1f)
                        absVelocitySum = 1f;

                    positionInverseMultiplier *= absVelocitySum;

                    if (positionInverseMultiplier < 0f)
                        positionInverseMultiplier = 0f;

                    absVelocitySum = 1f;
                    positionInverseMultiplier *= absVelocitySum;

                    if (playerToProjectile.Y > 0f)
                    {
                        playerToProjectile.Y *= 1f + positionInverseMultiplier;
                        playerToProjectile.X *= 1f - positionInverseMultiplier;
                    }
                    else
                    {
                        absVelocitySum = Math.Abs(haulox.velocity.X) / 3f;
                        if (absVelocitySum > 1f)
                            absVelocitySum = 1f;
                        absVelocitySum -= 0.5f;
                        positionInverseMultiplier *= absVelocitySum;
                        if (positionInverseMultiplier > 0f)
                            positionInverseMultiplier *= 2f;
                        playerToProjectile.Y *= 1f + positionInverseMultiplier;
                        playerToProjectile.X *= 1f - positionInverseMultiplier;
                    }
                }

                Color lineColor = Lighting.GetColor((int)lineOrigin.X / 16, (int)(lineOrigin.Y / 16f), Color.SaddleBrown);
                float rotation = playerToProjectile.ToRotation() - MathHelper.PiOver2;
                Texture2D texture = TextureAssets.FishingLine.Value;
                var position = lineOrigin + texture.Size() / 2f - Main.screenPosition;
                var src = new Rectangle(0, 0, texture.Width, (int)height);
                values.Add(new DrawData(texture, position, src, lineColor, rotation, new Vector2(texture.Width * 0.5f, 0f), 1f, SpriteEffects.None, 0f));
            }

            return values;
        }
    }

    public Haulox.Variants? storedVariant = null;

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

        storedVariant = null;
        ref int attachedNPC = ref player.GetModPlayer<YokePlayer>().attachedNPC;

        if (attachedNPC != -1 && Main.npc[attachedNPC] is { active: true } and { ModNPC: Haulox haul })
            haul.GuidingPlayer = -1;

        attachedNPC = -1;
    }

    public override bool? UseItem(Player player)
    {
        ref int attach = ref player.GetModPlayer<YokePlayer>().attachedNPC;

        if (attach >= 0)
        {
            return false;
        }

        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.ModNPC is Haulox haulox)
            {
                storedVariant = haulox.Variant;
                attach = haulox.NPC.whoAmI;
                haulox.GuidingPlayer = player.whoAmI;
                break;
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

internal class BlackHauloxYoke : YokeItem { }
internal class BrownHauloxYoke : YokeItem { }
internal class GingerHauloxYoke : YokeItem { }
internal class SpottedHauloxYoke : YokeItem { }
internal class WhiteHauloxYoke : YokeItem { }
