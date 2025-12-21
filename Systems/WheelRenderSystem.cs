using Drawbridge.Common.Tiles;
using Drawbridge.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Drawbridge.Systems;

#nullable enable

public class WheelRenderSystem : ModSystem
{
    public enum DrawType : byte
    {
        Default,
        Flipped,
        Horizontal,
        HorizontalFlipped,
    }

    private static readonly Dictionary<string, Asset<Texture2D>> TextureLookup = [];

    public override void Load()
    {
        TextureLookup.Clear();

        // Old textures, directly mirrored in PostDrawTiles by a named variable
        TextureLookup.Add("wheelV", ModContent.Request<Texture2D>("Drawbridge/Tiles/WheelTile"));
        TextureLookup.Add("wheelH", ModContent.Request<Texture2D>("Drawbridge/Tiles/WheelHorizontalTile"));
        TextureLookup.Add("hellWheel", ModContent.Request<Texture2D>("Drawbridge/Tiles/Hell_Wheel"));
        TextureLookup.Add("hellWheelHorizontal", ModContent.Request<Texture2D>("Drawbridge/Tiles/Hell_WheelHorizontalTile"));
        TextureLookup.Add("hellGlowMask", ModContent.Request<Texture2D>("Drawbridge/Tiles/Hell_Drawbridge-Glow_Mask"));
        TextureLookup.Add("hellGlowMaskFlipped",  ModContent.Request<Texture2D>("Drawbridge/Tiles/Hell_Drawbridge-Glow_MaskFlipped"));
        TextureLookup.Add("hellGlowMaskHorizontal",  ModContent.Request<Texture2D>("Drawbridge/Tiles/Hell_Drawbridge-Glow_MaskHorizontal"));
        TextureLookup.Add("hellGlowMaskHorizontalFlipped",  ModContent.Request<Texture2D>("Drawbridge/Tiles/Hell_Drawbridge-Glow_MaskHorizontalFlipped"));
        TextureLookup.Add("hellWheelGlowMask",  ModContent.Request<Texture2D>("Drawbridge/Tiles/Hell_Wheel-Glow_mask"));
        TextureLookup.Add("hellWheelGlowMaskHorizontal", ModContent.Request<Texture2D>("Drawbridge/Tiles/Hell_Wheel-Glow_maskHorizontal"));
        TextureLookup.Add("lionWheel", ModContent.Request<Texture2D>("Drawbridge/Tiles/Lion_Wheel"));
        TextureLookup.Add("lionWheelHorizontal", ModContent.Request<Texture2D>("Drawbridge/Tiles/Lion_WheelHorizontal"));
        TextureLookup.Add("stableMiddle", ModContent.Request<Texture2D>("Drawbridge/Tiles/HorseStableMiddle"));
        TextureLookup.Add("stableExtendedMiddle", ModContent.Request<Texture2D>("Drawbridge/Tiles/HorseStableExtendedMiddle"));
        TextureLookup.Add("lionCover", ModContent.Request<Texture2D>("Drawbridge/Tiles/Lion_Drawbridge_cover"));
        TextureLookup.Add("lionCoverFlipped", ModContent.Request<Texture2D>("Drawbridge/Tiles/Lion_DrawbridgeFlipped_cover"));
        TextureLookup.Add("lionCoverHorizontal", ModContent.Request<Texture2D>("Drawbridge/Tiles/Lion_DrawbridgeHorizontal_cover"));
        TextureLookup.Add("lionCoverHorizontalFlipped", ModContent.Request<Texture2D>("Drawbridge/Tiles/Lion_DrawbridgeHorizontalFlipped_cover"));

        // New values
        TextureLookup.Add("hellCover", ModContent.Request<Texture2D>("Drawbridge/Tiles/Hell_Drawbridge_cover"));
        TextureLookup.Add("hellFlippedCover", ModContent.Request<Texture2D>("Drawbridge/Tiles/Hell_DrawbridgeFlipped_cover"));
        TextureLookup.Add("hellHoriCover", ModContent.Request<Texture2D>("Drawbridge/Tiles/Hell_DrawbridgeHorizontal_cover"));
        TextureLookup.Add("hellHoriFlippedCover", ModContent.Request<Texture2D>("Drawbridge/Tiles/Hell_DrawbridgeHorizontalFlipped_cover"));
    }

    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "beginCalled")]
    static extern ref bool GetBeginCalled(SpriteBatch batch);

    public override void PostDrawTiles()
    {
        // Not sure why this is necessary?
        if (GetBeginCalled(Main.spriteBatch))
            Main.spriteBatch.End();

        // Begin a new sprite batch for world drawing
        SpriteBatch sb = Main.spriteBatch;
        Matrix matrix = Main.GameViewMatrix.TransformationMatrix;
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, matrix);

        Texture2D wheelH = TextureLookup["wheelH"].Value;
        Texture2D lionWheel = TextureLookup["lionWheel"].Value;
        Texture2D lionWheelHorizontal = TextureLookup["lionWheelHorizontal"].Value;
        Texture2D stableMiddle = TextureLookup["stableMiddle"].Value;
        Texture2D stableExtendedMiddle = TextureLookup["stableExtendedMiddle"].Value;

        Vector2 screenPos = Main.screenPosition;
        int screenW = Main.screenWidth;
        int screenH = Main.screenHeight;
        int iStart = (int)(screenPos.X / 16f) - 2;
        int iEnd = (int)((screenPos.X + screenW) / 16f) + 2;
        int jStart = (int)(screenPos.Y / 16f) - 2;
        int jEnd = (int)((screenPos.Y + screenH) / 16f) + 2;

        for (int i = iStart; i <= iEnd; i++)
        {
            for (int j = jStart; j <= jEnd; j++)
            {
                if (!WorldGen.InWorld(i, j))
                    continue;

                Tile tile = Main.tile[i, j];

                if (!tile.HasTile)
                    continue;

                Vector2 coverOffset = Vector2.Zero;
                Color chainColor = Lighting.GetColor(i, j);

                if (ModContent.GetModTile(tile.TileType) is IDrawbridgeCover draw && draw.CanDraw(tile, out DrawType drawType, out string mainTex, out string glowTex, ref coverOffset))
                    DrawCoverSprite(i, j, mainTex, glowTex, drawType, coverOffset);

                if (ModContent.GetModTile(tile.TileType) is IDrawbridgeWheels wheels && wheels.DrawWheel(tile, out string wheelTex, out string wheelGlow, out DrawType wheelType))
                    DrawWheel(wheelTex, wheelGlow, i, j, wheelType);

                if (ModContent.GetModTile(tile.TileType) is IDrawbridgeChains chains && chains.DrawChains(tile, out DrawType chainType, ref chainColor))
                    DrawChains(i, j, chainType, chainColor);

                if (tile.TileType == ModContent.TileType<LionDrawbridgeTile>() && tile.TileFrameX == 16 && tile.TileFrameY == 224)
                {
                    const float tileOffset = 12 * 16f;
                    Vector2 pivot = new Vector2(i * 16f + 16f + tileOffset - 16f,
                                                j * 16f + 16f + tileOffset) - screenPos;
                    pivot -= new Vector2(12 * 16f, 12 * 16f);
                    Vector2 origin = new Vector2(lionWheel.Width * 0.5f, lionWheel.Height * 0.5f);
                    sb.Draw(lionWheel, pivot, null, Lighting.GetColor(i, j) * 0.9f, 0f, origin, 1f, SpriteEffects.None, 0f);
                }
                // Static Lion vertical hinge segment (flipped)
                else if (tile.TileType == ModContent.TileType<LionDrawbridgeVerticalFlippedTile>() && tile.TileFrameX == 0 && tile.TileFrameY == 224)
                {
                    const float tileOffset = 12 * 16f;
                    // Wheel pivot (shifted left one tile)
                    Vector2 pivot = new Vector2(i * 16f + 16f + tileOffset - 16f,
                                                j * 16f + 16f + tileOffset) - screenPos;
                    pivot -= new Vector2(12 * 16f, 12 * 16f);
                    pivot.X += 16f; // Shift one more tile right
                    Vector2 originFlipped = new Vector2(lionWheel.Width * 0.5f, lionWheel.Height * 0.5f);
                    sb.Draw(lionWheel, pivot, null, Lighting.GetColor(i, j) * 0.9f, 0f, originFlipped, 1f, SpriteEffects.FlipHorizontally, 0f);
                    // Draw static chain for flipped vertical drawbridge (to left)
                    Texture2D chainTex2 = TextureAssets.Chain.Value;
                    Vector2 chainOrigin2 = new Vector2(chainTex2.Width * 0.5f, chainTex2.Height * 0.5f);
                    int segmentY2 = 2; // 13th tile from bottom
                    Vector2 worldStart2 = new Vector2((i + 1) * 16f + 8f + tileOffset,
                                                     (j - 14 + segmentY2) * 16f + 8f + tileOffset);
                    Vector2 worldEnd2 = new Vector2((i + 2) * 16f + 8f + tileOffset,
                                                   (j - 14 + segmentY2) * 16f + 8f + tileOffset);
                    Vector2 start2 = worldStart2 - screenPos;
                    start2 -= new Vector2(12 * 16f, 12 * 16f);
                    Vector2 end2 = worldEnd2 - screenPos;
                    end2 -= new Vector2(12 * 16f, 12 * 16f);
                    Vector2 dir2 = end2 - start2;
                    float distance2 = dir2.Length();
                    if (distance2 > 0f)
                    {
                        dir2 /= distance2;
                        float rotationChain2 = dir2.ToRotation() + MathHelper.PiOver2;
                        float step2 = chainTex2.Height * 0.9f;
                        for (float t2 = step2; t2 < distance2 - step2; t2 += step2)
                        {
                            Vector2 pos2 = start2 + dir2 * t2;
                            // Apply golden tint for lion drawbridge chains
                            Color lionChainColor = Color.Lerp(Lighting.GetColor(i, j), new Color(255, 215, 0), 0.3f);
                            sb.Draw(chainTex2, pos2, null, lionChainColor, rotationChain2, chainOrigin2, 1f, SpriteEffects.FlipHorizontally, 0f);
                        }
                    }
                }
                // Static Lion horizontal hinge segment (normal)
                else if (tile.TileType == ModContent.TileType<LionDrawbridgeHorizontalTile>() && tile.TileFrameX == 224 && tile.TileFrameY == 16)
                {
                    const float tileOffset = 12 * 16f;
                    Vector2 pivot = new Vector2(i * 16f + 16f + tileOffset - 16f,
                                                j * 16f + 16f + tileOffset) - screenPos;
                    pivot -= new Vector2(12 * 16f, 12 * 16f);
                    Vector2 origin = new Vector2(lionWheelHorizontal.Width * 0.5f, lionWheelHorizontal.Height * 0.5f);
                    sb.Draw(lionWheelHorizontal, pivot, null, Lighting.GetColor(i, j) * 0.9f, 0f, origin, 1f, SpriteEffects.FlipHorizontally, 0f);
                    
                    // Draw static chain for lion horizontal drawbridge (left pivot)
                    Texture2D chainTexture = TextureAssets.Chain.Value;
                    Vector2 chainOrigin = new Vector2(chainTexture.Width * 0.5f, chainTexture.Height * 0.5f);
                    int segmentY = 2; // 13th tile from bottom
                    int originX = i - 1;
                    int originY = j;
                    int attachX = i - (14 - segmentY);
                    int attachY = j;
                    Vector2 worldStart = new Vector2(attachX * 16f + 8f + tileOffset,
                                                     attachY * 16f + 8f + tileOffset - 16f);
                    Vector2 worldEnd = new Vector2((originX + 2) * 16f + 8f + tileOffset,
                                                   (originY - 14 + segmentY) * 16f + 8f + tileOffset);
                    Vector2 start = worldStart - screenPos;
                    start -= new Vector2(12 * 16f, 12 * 16f);
                    Vector2 end = worldEnd - screenPos;
                    end -= new Vector2(12 * 16f, 12 * 16f);
                    Vector2 dir = end - start;
                    float distance = dir.Length();
                    if (distance > 0f)
                    {
                        dir /= distance;
                        float rotationChain = dir.ToRotation() + MathHelper.PiOver2;
                        float step = chainTexture.Height * 0.9f;
                        for (float t = step; t < distance - step; t += step)
                        {
                            Vector2 pos = start + dir * t;
                            // Apply golden tint for lion drawbridge chains
                            Color lionChainColor = Color.Lerp(Lighting.GetColor(i, j), new Color(255, 215, 0), 0.3f);
                            sb.Draw(chainTexture, pos, null, lionChainColor,
                                    rotationChain, chainOrigin, 1f, SpriteEffects.None, 0f);
                        }
                    }
                }
                // Static Lion horizontal hinge segment (flipped)
                else if (tile.TileType == ModContent.TileType<LionDrawbridgeHorizontalFlippedTile>() && tile.TileFrameX == 0 && tile.TileFrameY == 16)
                {
                    const float tileOffset = 12 * 16f;
                    Vector2 pivot = new Vector2(i * 16f + 16f + tileOffset - 16f,
                                                j * 16f + 16f + tileOffset) - screenPos;
                    pivot -= new Vector2(12 * 16f, 12 * 16f);
                    pivot.X += 16f;
                    Vector2 origin = new Vector2(lionWheelHorizontal.Width * 0.5f, lionWheelHorizontal.Height * 0.5f);
                    sb.Draw(lionWheelHorizontal, pivot, null, Lighting.GetColor(i, j) * 0.9f, 0f, origin, 1f, SpriteEffects.None, 0f);
                    
                    // Draw static chain for lion horizontal drawbridge (right pivot)
                    Texture2D chainTextureFlipped = TextureAssets.Chain.Value;
                    Vector2 chainOriginFlipped = new Vector2(chainTextureFlipped.Width * 0.5f, chainTextureFlipped.Height * 0.5f);
                    int segmentYFlipped = 2; // 13th tile from bottom
                    int originXFlipped = i;
                    int originYFlipped = j;
                    int attachXFlipped = i + (14 - segmentYFlipped);
                    int attachYFlipped = j;
                    Vector2 worldStartFlipped = new Vector2(attachXFlipped * 16f + 8f + tileOffset,
                                                             attachYFlipped * 16f + 8f + tileOffset - 16f);
                    Vector2 worldEndFlipped = new Vector2((originXFlipped - 1) * 16f + 8f + tileOffset,
                                                           (originYFlipped - 14 + segmentYFlipped) * 16f + 8f + tileOffset);
                    Vector2 startFlipped = worldStartFlipped - screenPos;
                    startFlipped -= new Vector2(12 * 16f, 12 * 16f);
                    Vector2 endFlipped = worldEndFlipped - screenPos;
                    endFlipped -= new Vector2(12 * 16f, 12 * 16f);
                    Vector2 dirFlipped = endFlipped - startFlipped;
                    float distanceFlipped = dirFlipped.Length();
                    if (distanceFlipped > 0f)
                    {
                        dirFlipped /= distanceFlipped;
                        float rotationChainFlipped = dirFlipped.ToRotation() + MathHelper.PiOver2;
                        float stepFlipped = chainTextureFlipped.Height * 0.9f;
                        for (float tFlipped = stepFlipped; tFlipped < distanceFlipped - stepFlipped; tFlipped += stepFlipped)
                        {
                            Vector2 posFlipped = startFlipped + dirFlipped * tFlipped;
                            // Apply golden tint for lion drawbridge chains
                            Color lionChainColorFlipped = Color.Lerp(Lighting.GetColor(i, j), new Color(255, 215, 0), 0.3f);
                            sb.Draw(chainTextureFlipped, posFlipped, null, lionChainColorFlipped,
                                    rotationChainFlipped, chainOriginFlipped, 1f, SpriteEffects.FlipHorizontally, 0f);
                        }
                    }
                }
                // Static horizontal hinge segment (flipped)
                else if (tile.TileType == ModContent.TileType<DrawbridgeHorizontalFlippedTile>() && tile.TileFrameX == 0 && tile.TileFrameY == 16)
                {
                    const float tileOffset = 12 * 16f;
                    Vector2 pivot = new Vector2(i * 16f + 16f + tileOffset - 16f,
                                                j * 16f + 16f + tileOffset) - screenPos;
                    // Move pivot up and left by 13 tiles
                    pivot -= new Vector2(12 * 16f, 12 * 16f);
                    // Shift wheel pivot one tile right for flipped horizontal
                    pivot.X += 16f;
                    Vector2 origin = new Vector2(wheelH.Width * 0.5f, wheelH.Height * 0.5f);
                    sb.Draw(wheelH, pivot, null, Lighting.GetColor(i, j) * 0.9f, 0f, origin, 1f, SpriteEffects.FlipHorizontally, 0f);
                    // Draw static chain for horizontal drawbridge (right pivot)
                    Texture2D chainTextureFlipped = TextureAssets.Chain.Value;
                    Vector2 chainOriginFlipped = new Vector2(chainTextureFlipped.Width * 0.5f, chainTextureFlipped.Height * 0.5f);
                    int segmentYFlipped = 2; // 13th tile from bottom
                    int originXFlipped = i;
                    int originYFlipped = j;
                    int attachXFlipped = i + (14 - segmentYFlipped);
                    int attachYFlipped = j;
                    Vector2 worldStartFlipped = new Vector2(attachXFlipped * 16f + 8f + tileOffset,
                                                             attachYFlipped * 16f + 8f + tileOffset - 16f);
                    Vector2 worldEndFlipped = new Vector2((originXFlipped - 1) * 16f + 8f + tileOffset,
                                                           (originYFlipped - 14 + segmentYFlipped) * 16f + 8f + tileOffset);
                    Vector2 startFlipped = worldStartFlipped - screenPos;
                    startFlipped -= new Vector2(12 * 16f, 12 * 16f);
                    Vector2 endFlipped = worldEndFlipped - screenPos;
                    endFlipped -= new Vector2(12 * 16f, 12 * 16f);
                    Vector2 dirFlipped = endFlipped - startFlipped;
                    float distanceFlipped = dirFlipped.Length();
                    if (distanceFlipped > 0f)
                    {
                        dirFlipped /= distanceFlipped;
                        float rotationChainFlipped = dirFlipped.ToRotation() + MathHelper.PiOver2;
                        float stepFlipped = chainTextureFlipped.Height * 0.9f;
                        for (float tFlipped = stepFlipped; tFlipped < distanceFlipped - stepFlipped; tFlipped += stepFlipped)
                        {
                            Vector2 posFlipped = startFlipped + dirFlipped * tFlipped;
                            sb.Draw(chainTextureFlipped, posFlipped, null, Lighting.GetColor(i, j),
                                    rotationChainFlipped, chainOriginFlipped, 1f, SpriteEffects.FlipHorizontally, 0f);
                        }
                    }
                }

                if (tile.TileType == ModContent.TileType<HorseStableTile>())
                {
                    // Only draw at the origin tile (frameX == 4*16, frameY == 5*16)
                    if (tile.TileFrameX == 4 * 16 && tile.TileFrameY == 5 * 16)
                    {
                        int topLeftX = i - 4;
                        int topLeftY = j - 5;
                        Vector2 drawPos = new Vector2(topLeftX * 16 + 12, topLeftY * 16) - screenPos;
                        sb.Draw(stableMiddle, drawPos, null, Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                }

                // Add support for extended stable
                if (tile.TileType == ModContent.TileType<HorseStableExtendedTile>())
                {
                    // Only draw at the origin tile (frameX == 13*16, frameY == 5*16)
                    if (tile.TileFrameX == 13 * 16 && tile.TileFrameY == 5 * 16)
                    {
                        int topLeftX = i - 13;
                        int topLeftY = j - 5;
                        Vector2 drawPos = new Vector2(topLeftX * 16 + 12, topLeftY * 16) - screenPos;
                        sb.Draw(stableExtendedMiddle, drawPos, null, Lighting.GetColor(i, j), 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                    }
                }
            }
        }

        // End the custom batch to avoid interfering with other draw calls
        sb.End();
    }

    private static void DrawChains(int i, int j, DrawType type, Color color)
    {
        const float tileOffset = 12 * 16f;

        Vector2 pivot = new Vector2(i * 16f + 16f + tileOffset - 16f, j * 16f + 16f + tileOffset) - Main.screenPosition;

        // Move pivot up and left by 13 tiles
        pivot -= new Vector2(tileOffset);

        if (type == DrawType.Flipped)
        {
            pivot.X += 16;
        }

        // Draw static chain for vertical drawbridge
        Texture2D chainTexture = TextureAssets.Chain.Value;
        Vector2 chainOrigin = new(chainTexture.Width * 0.5f, chainTexture.Height * 0.5f);
        int segmentY = 2; // 13th tile from bottom => 3rd from top
        int attachX = i + (14 - segmentY);

        Vector2 worldStart = type switch
        {
            DrawType.Default => new((i + 1) * 16f + 8f + tileOffset, (j - 14 + segmentY) * 16f + 8f + tileOffset),
            DrawType.Flipped => new Vector2((i - 1) * 16f + 8f + tileOffset, (j - 13 + segmentY) * 16f + 8f + tileOffset - 16f)
        };

        Vector2 worldEnd = type switch
        {
            DrawType.Default => new((i - 13) * 16f + 8f + 12 * 16f, j * 16f + 8f + 12 * 16f),
            DrawType.Flipped => new Vector2((attachX + 1) * 16f + 8f + tileOffset, j * 16f + 8f + tileOffset)
        };

        Vector2 start = worldStart - Main.screenPosition;
        start -= new Vector2(tileOffset);
        Vector2 end = worldEnd - Main.screenPosition;
        end -= new Vector2(tileOffset);
        Vector2 dir = end - start;
        float distance = dir.Length();

        if (distance > 0f)
        {
            dir /= distance;
            float rotationChain = dir.ToRotation() + MathHelper.PiOver2;
            float step = chainTexture.Height * 0.9f;
            for (float t = step; t < distance - step; t += step)
            {
                Vector2 pos = start + dir * t;
                Main.spriteBatch.Draw(chainTexture, pos, null, color, rotationChain, chainOrigin, 1f, SpriteEffects.None, 0f);
            }
        }
    }

    private static void DrawWheel(string textureName, string glowName, int i, int j, DrawType type)
    {
        const float Offset = 12 * 16f;

        Texture2D tex = TextureLookup[textureName].Value;

        Vector2 pivot = type switch
        {
            DrawType.Default => new Vector2(i * 16f + 16f + Offset - 16f, j * 16f + 16f + Offset) - Main.screenPosition - new Vector2(12 * 16f, 12 * 16f),
            DrawType.Flipped => new Vector2(i * 16f + 16f + Offset - 16f, j * 16f + 16f + Offset) - Main.screenPosition - new Vector2(12 * 16f, 12 * 16f) + new Vector2(16, 0),
            _ => throw null!, // Wheels only need to be flipped
        };

        SpriteEffects effects = type != DrawType.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

        //pivot -= new Vector2(12 * 16f, 12 * 16f);
        Vector2 origin = new(tex.Width * 0.5f, tex.Height * 0.5f);
        Main.spriteBatch.Draw(tex, pivot, null, Lighting.GetColor(i, j) * 0.9f, 0f, origin, 1f, effects, 0f);

        if (glowName == "")
            return;

        Texture2D glow = TextureLookup[glowName].Value;
        float glowPulse = 0.6f + 0.4f * (float)Math.Sin(Main.GameUpdateCount * 0.03f);
        Main.spriteBatch.Draw(glow, pivot, null, (Color.White * glowPulse) with { A = 0 }, 0f, origin, 1f, effects, 0f);
    }

    private static void DrawCoverSprite(int i, int j, string textureName, string glowMaskName, DrawType type, Vector2 offset)
    {
        Texture2D texture = TextureLookup[textureName].Value;
        Vector2 pivot = new Vector2(i * 16f, j * 16f + 16f) - Main.screenPosition;

        Vector2 origin = type switch
        {
            DrawType.Default => new Vector2(30f + 4f, texture.Height), // 14px left of hinge + 4px adjustment
            DrawType.Flipped => new Vector2(texture.Width - 42f, texture.Height),
            DrawType.Horizontal => new Vector2(30f + 4f + 192f - 2f, texture.Height - 16f - 2f),
            DrawType.HorizontalFlipped => new Vector2(texture.Width - 46f - 4f - 192f + 2f, texture.Height - 16f - 2f),
            _ => throw null!,
        };

        pivot += offset;
        Main.spriteBatch.Draw(texture, pivot, null, Lighting.GetColor(i, j) * 0.9f, 0f, origin, 1f, SpriteEffects.None, 0f);

        if (glowMaskName == "")
            return;

        // Draw glow mask overlay with pulsing effect
        float glowPulse = 0.6f + 0.4f * (float)Math.Sin(Main.GameUpdateCount * 0.03f);
        Main.spriteBatch.Draw(TextureLookup[glowMaskName].Value, pivot, null, (Color.White * glowPulse) with { A = 0 }, 0f, origin, 1f, SpriteEffects.None, 0f);
    }
}