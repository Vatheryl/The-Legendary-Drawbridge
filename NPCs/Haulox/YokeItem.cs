using Drawbridge.Systems.Synchronization;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

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

            if (Player.HeldItem.IsAir && Main.myPlayer == Player.whoAmI && Main.mouseRight && Main.mouseRightRelease)
            {
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.ModNPC is Haulox haulox && npc.Hitbox.Contains(Main.MouseWorld.ToPoint()) && npc.DistanceSQ(Player.Center) < 120 * 120 && haulox.ArmorId != -1)
                    {
                        Main.mouseRightRelease = false;

                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            HauloxArmor.HauloxArmorHandler.SendUnarmor((short)npc.whoAmI);
                        else
                            Item.NewItem(npc.GetSource_FromThis(), npc.Hitbox, haulox.ArmorId);

                        haulox.ArmorId = -1;
                        break;
                    }
                }
            }
        }

        public override bool CanHitNPC(NPC target) =>  target.whoAmI != attachedNPC || target.ModNPC is not Haulox haulox || haulox.State is not Haulox.States.Staked and not Haulox.States.BeingGuided;
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

    public class YokePlayerHandler : Handler
    {
        public override MessageType Message => MessageType.YokePlayer;

        public static void SendAttach(short npcWho, byte? who = null)
        {
            ModPacket packet = PacketHandler.GetPacket(MessageType.YokePlayer);
            packet.Write(true);

            if (who is { } whoAm)
                packet.Write(whoAm);

            packet.Write(npcWho);
            packet.Send();
        }

        public static void SendDetatch(byte? who = null)
        {
            ModPacket packet = PacketHandler.GetPacket(MessageType.YokePlayer);
            packet.Write(false);

            if (who is { } whoAm)
                packet.Write(whoAm);

            packet.Send();
        }

        public override void Recieve(int whoAmI, BinaryReader reader, bool onServer)
        {
            bool added = reader.ReadBoolean();
            byte who = onServer ? (byte)whoAmI : reader.ReadByte();
            Player player = Main.player[who];

            if (added)
            {
                short npcWho = reader.ReadInt16();
                player.GetModPlayer<YokePlayer>().attachedNPC = npcWho;

                if (onServer)
                    SendAttach(npcWho, who);
            }
            else
            {
                player.GetModPlayer<YokePlayer>().attachedNPC = -1;

                if (onServer)
                    SendDetatch(who);
            }
        }
    }

    public bool attached = false;

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 3;

    public override void SetDefaults()
    {
        Item.Size = new Vector2(46, 38);
        Item.useTime = Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.rare = ItemRarityID.Green;
        Item.noUseGraphic = true;
        Item.noMelee = true;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool? UseItem(Player player)
    {
        if (player.altFunctionUse == 2)
        {
            if (!attached)
                return true;

            attached = false;
            ref int attachedNPC = ref player.GetModPlayer<YokePlayer>().attachedNPC;

            if (attachedNPC != -1 && Main.npc[attachedNPC] is { active: true } and { ModNPC: Haulox haul })
            {
                haul.GuidingPlayer = -1;

                if (Main.netMode == NetmodeID.MultiplayerClient && player.whoAmI == Main.myPlayer)
                {
                    Haulox.YokeHauloxHandler.SendRelease((short)attachedNPC);
                    YokePlayerHandler.SendDetatch();
                }
            }

            attachedNPC = -1;
            return true;
        }

        ref int attach = ref player.GetModPlayer<YokePlayer>().attachedNPC;

        if (attach >= 0 || player.whoAmI != Main.myPlayer)
            return false;

        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.ModNPC is Haulox haulox && npc.Hitbox.Contains(Main.MouseWorld.ToPoint()) && npc.DistanceSQ(player.Center) < 120 * 120 && haulox.GuidingPlayer == -1 && !haulox.Staked)
            {
                attached = true;
                attach = haulox.NPC.whoAmI;
                haulox.GuidingPlayer = player.whoAmI;
                haulox.AttachedYokeId = Item.type;
                haulox.StoredSlot = (sbyte)player.selectedItem;

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    Haulox.YokeHauloxHandler.SendYoked((short)npc.whoAmI, (short)Item.type, (sbyte)player.selectedItem);
                    YokePlayerHandler.SendAttach((short)npc.whoAmI);
                }

                break;
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

    public override void AddRecipes() => CreateRecipe().AddRecipeGroup(RecipeGroupID.Wood, 12).AddIngredient(ItemID.Rope, 20).AddTile(TileID.Sawmill).Register();
}

internal class BlackHauloxYoke : YokeItem { }
internal class BrownHauloxYoke : YokeItem { }
internal class GingerHauloxYoke : YokeItem { }
internal class SpottedHauloxYoke : YokeItem { }
internal class WhiteHauloxYoke : YokeItem { }
