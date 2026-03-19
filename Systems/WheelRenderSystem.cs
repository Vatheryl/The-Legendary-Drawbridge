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

    internal static readonly Dictionary<string, Asset<Texture2D>> Textures = [];

    public static void AddTexture(string name, string path) => Textures.Add(name, ModContent.Request<Texture2D>(path));

    public override void Load()
    {
        Textures.Add("wheelV", ModContent.Request<Texture2D>("Drawbridge/Tiles/WheelTile"));
        Textures.Add("wheelH", ModContent.Request<Texture2D>("Drawbridge/Tiles/WheelHorizontalTile"));

        Textures.Add("stableMiddle", ModContent.Request<Texture2D>("Drawbridge/Tiles/HorseStableMiddle"));
        Textures.Add("stableExtendedMiddle", ModContent.Request<Texture2D>("Drawbridge/Tiles/HorseStableExtendedMiddle"));
    }

    public override void Unload() => Textures.Clear();

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

        Texture2D stableMiddle = Textures["stableMiddle"].Value;
        Texture2D stableExtendedMiddle = Textures["stableExtendedMiddle"].Value;

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
            DrawType.Flipped => new Vector2((i - 1) * 16f + 8f + tileOffset, (j - 13 + segmentY) * 16f + 8f + tileOffset - 16f),
            _ => throw null!,
        };

        Vector2 worldEnd = type switch
        {
            DrawType.Default => new((i - 13) * 16f + 8f + 12 * 16f, j * 16f + 8f + 12 * 16f),
            DrawType.Flipped => new Vector2((attachX + 1) * 16f + 8f + tileOffset, j * 16f + 8f + tileOffset),
            _ => throw null!,
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

        Texture2D tex = Textures[textureName].Value;

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

        Texture2D glow = Textures[glowName].Value;
        float glowPulse = 0.6f + 0.4f * (float)Math.Sin(Main.GameUpdateCount * 0.03f);
        Main.spriteBatch.Draw(glow, pivot, null, (Color.White * glowPulse) with { A = 0 }, 0f, origin, 1f, effects, 0f);
    }

    private static void DrawCoverSprite(int i, int j, string textureName, string glowMaskName, DrawType type, Vector2 offset)
    {
        Texture2D texture = Textures[textureName].Value;
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
        Main.spriteBatch.Draw(Textures[glowMaskName].Value, pivot, null, (Color.White * glowPulse) with { A = 0 }, 0f, origin, 1f, SpriteEffects.None, 0f);
    }
}