using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.Enums;
using Terraria.DataStructures;
using Drawbridge.NPCs;

namespace Drawbridge.Tiles
{
    public class CulvertDecorationTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileObsidianKill[Type] = false;
            Main.tileLavaDeath[Type] = false;
            Main.tileWaterDeath[Type] = false;
            Main.tileSolid[Type] = false;
            Main.tileSolidTop[Type] = false;
            TileID.Sets.DisableSmartCursor[Type] = true;
            Main.tileLighted[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.Height = 4;
            TileObjectData.newTile.Origin = new Point16(1, 3); // bottom-center for 4x4
            TileObjectData.newTile.AnchorWall = true;
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.AnchorTop = AnchorData.Empty;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 0;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(80, 80, 80), CreateMapEntryName());
            DustType = DustID.Stone;
            HitSound = SoundID.Tink;
        }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // Compute the anchor point (bottom-center of the 4x4, as configured by Origin = (1,3)) in tile units
            int topLeftX = i - 1; // since origin is (1,3)
            int topLeftY = j - 3;
            int anchorTileX = topLeftX + 1; // bottom-center x
            int anchorTileY = topLeftY + 3; // bottom row

            // World position for spawning head: bottom-middle of tile, then 6px upward
            Vector2 spawnPos = new Vector2(anchorTileX * 16 + 8, anchorTileY * 16 + 16 - 6);

            // Prevent double-spawn: if a serpent already anchored here exists, skip
            for (int n = 0; n < Main.maxNPCs; n++)
            {
                NPC ncheck = Main.npc[n];
                if (ncheck.active && ncheck.type == ModContent.NPCType<ObsidianSerpent>() && (int)ncheck.ai[0] == anchorTileX && (int)ncheck.ai[1] == anchorTileY)
                {
                    return;
                }
            }

            var source = new EntitySource_Misc("CulvertDecorationTile.PlaceInWorld");
            int npcIndex = NPC.NewNPC(source, (int)spawnPos.X, (int)spawnPos.Y, ModContent.NPCType<ObsidianSerpent>());
            if (npcIndex >= 0 && npcIndex < Main.npc.Length)
            {
                NPC serpent = Main.npc[npcIndex];
                // Store anchor in ai[0]/ai[1] in tile coordinates for custom anchored behavior
                serpent.ai[0] = anchorTileX;
                serpent.ai[1] = anchorTileY;
                serpent.netUpdate = true;
            }
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // Determine top-left and anchor tile coordinates for this 4x4
            int topLeftX = i - (frameX / 16 % 4);
            int topLeftY = j - (frameY / 16 % 4);
            int anchorTileX = topLeftX + 1;
            int anchorTileY = topLeftY + 3;

            for (int n = 0; n < Main.maxNPCs; n++)
            {
                NPC npc = Main.npc[n];
                if (!npc.active)
                    continue;
                if (npc.type != ModContent.NPCType<ObsidianSerpent>())
                    continue;
                if ((int)npc.ai[0] == anchorTileX && (int)npc.ai[1] == anchorTileY)
                {
                    npc.StrikeInstantKill();
                }
            }
        }
    }
} 