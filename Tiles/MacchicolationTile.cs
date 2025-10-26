using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Drawbridge.Tiles
{
    public class MacchicolationTile : ModTile
    {
        public override string Texture => "Drawbridge/Tiles/Cauldron_Off-1";

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;
            Main.tileSolid[Type] = false;
            Main.tileSolidTop[Type] = false;
            TileID.Sets.DisableSmartCursor[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.Width = 10;
            TileObjectData.newTile.Height = 6;
            TileObjectData.newTile.Origin = new Point16(0, 5);
            TileObjectData.newTile.AnchorTop = AnchorData.Empty;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, 1, 0);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(ModContent.GetInstance<MacchicolationTileEntity>().Hook_AfterPlacement, -1, 0, true);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(139, 69, 19), CreateMapEntryName());
            DustType = DustID.Stone;
            HitSound = SoundID.Tink;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            ModContent.GetInstance<MacchicolationTileEntity>().Kill(i, j);
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 160, 96,
                ModContent.ItemType<Items.MacchicolationItem>());
        }

        public override bool CanDrop(int i, int j) => false;

        public override bool RightClick(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            if (tile == null)
                return false;

            int frameX = tile.TileFrameX / 18;
            int frameY = tile.TileFrameY / 18;
            int originX = i - frameX;
            int originY = j - frameY;

            if (TileEntity.ByPosition.TryGetValue(new Point16(originX, originY), out TileEntity te) && te is MacchicolationTileEntity mte)
            {
                mte.Activate();
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.TileEntitySharing, number: mte.ID, number2: originX, number3: originY);
                return true;
            }
            return false;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            if (tile == null)
                return true;

            if (!TileEntity.ByPosition.TryGetValue(new Point16(i - tile.TileFrameX / 18, j - tile.TileFrameY / 18), out TileEntity te) || !(te is MacchicolationTileEntity mte))
                return true;

            if (mte.State == MacchicolationTileEntity.AnimationState.Off)
                return true;

            bool isTopLeft = tile.TileFrameX == 0 && tile.TileFrameY == 0;
            if (!isTopLeft)
                return false;

            string texturePath = null;
            int frameHeight = 106;
            int frameWidth = 176;

            switch (mte.State)
            {
                case MacchicolationTileEntity.AnimationState.Low:
                    texturePath = "Drawbridge/Tiles/Cauldron_Low_Fire";
                    break;
                case MacchicolationTileEntity.AnimationState.Medium:
                    texturePath = "Drawbridge/Tiles/Cauldron_Medium-1";
                    break;
                case MacchicolationTileEntity.AnimationState.High:
                    texturePath = "Drawbridge/Tiles/Cauldron_High_Use";
                    break;
            }

            Texture2D texture = ModContent.Request<Texture2D>(texturePath, AssetRequestMode.ImmediateLoad).Value;
            Vector2 off = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);

            int originX = i;
            int originY = j;

            int frameTop = frameHeight * mte.Frame;
            const int tilesWide = 10;
            const int tilesHigh = 6;
            const int step = 18; // 16px tile + 2px padding

            for (int y = 0; y < tilesHigh; y++)
            {
                for (int x = 0; x < tilesWide; x++)
                {
                    int sx = x * step;
                    int sy = frameTop + y * step;
                    int sw = 16;
                    int sh = 16;
                    if (sx + sw > frameWidth) sw = frameWidth - sx;
                    if (sy + sh > frameTop + frameHeight) sh = frameTop + frameHeight - sy;
                    if (sw <= 0 || sh <= 0)
                        continue;

                    Rectangle src = new Rectangle(sx, sy, sw, sh);
                    Rectangle dest = new Rectangle((originX + x) * 16 - (int)Main.screenPosition.X + (int)off.X,
                                                   (originY + y) * 16 - (int)Main.screenPosition.Y + (int)off.Y,
                                                   16, 16);
                    Color c = Lighting.GetColor(originX + x, originY + y);
                    spriteBatch.Draw(texture, dest, src, c);
                }
            }
            return false;
        }
    }

    public class MacchicolationTileEntity : ModTileEntity
    {
        public enum AnimationState
        {
            Off = 0,
            Low = 1,
            Medium = 2,
            High = 3
        }

        public AnimationState State;
        public int Frame;
        public int FrameCounter;
        public int StateTimer;
        public int CooldownTimer; // Cooldown before next use

        // Track boiling water positions and their lifetime (in ticks)
        private Dictionary<Point, int> boilingWaterPositions = new Dictionary<Point, int>();
        private HashSet<Point> sourcePositions = new HashSet<Point>(); // Track where water was originally dropped
        private const int BoilingWaterLifetime = 180; // 3 seconds at 60 fps
        private const int CooldownDuration = 10800; // 3 minutes at 60 fps (180 seconds)
        private const int MaxHorizontalSpread = 8; // Maximum horizontal distance from source
        private const int MaxVerticalDistance = 20; // Maximum vertical distance from source

        public override void Update()
        {
            // Update cooldown timer
            if (CooldownTimer > 0)
                CooldownTimer--;

            // Update boiling water effects
            UpdateBoilingWater();

            switch (State)
            {
                case AnimationState.Off:
                    return;
                case AnimationState.Low:
                    Animate(4, 8);
                    if (--StateTimer <= 0)
                        StartMedium();
                    break;
                case AnimationState.Medium:
                    Animate(8, 6);
                    if (--StateTimer <= 0)
                        StartHigh();
                    break;
                case AnimationState.High:
                    DropWater();
                    if (AnimateOnce(30, 3))
                        Stop();
                    break;
            }
        }

        void Animate(int frameCount, int ticksPerFrame)
        {
            FrameCounter++;
            if (FrameCounter >= ticksPerFrame)
            {
                FrameCounter = 0;
                Frame++;
                if (Frame >= frameCount)
                    Frame = 0;
            }
        }

        bool AnimateOnce(int frameCount, int ticksPerFrame)
        {
            FrameCounter++;
            if (FrameCounter >= ticksPerFrame)
            {
                FrameCounter = 0;
                Frame++;
            }
            return Frame >= frameCount;
        }

        void DropWater()
        {
            // Only drop water on the server or in single player
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            // Only drop water every 10 ticks to reduce the amount
            if (FrameCounter % 10 != 0)
                return;

            // Drop water from underneath tiles 8 and 9
            int waterX1 = Position.X + 8;
            int waterX2 = Position.X + 9;
            int waterY = Position.Y + 6; // Below the 6-tile-high structure

            // Track source positions
            sourcePositions.Add(new Point(waterX1, waterY));
            sourcePositions.Add(new Point(waterX2, waterY));

            // Place water at both positions with reduced amount
            WorldGen.PlaceLiquid(waterX1, waterY, 0, 50); // 0 = water, 50 = reduced amount
            WorldGen.PlaceLiquid(waterX2, waterY, 0, 50);

            // Track these positions and water flowing down from them
            TrackBoilingWaterArea(waterX1, waterY);
            TrackBoilingWaterArea(waterX2, waterY);
        }

        void TrackBoilingWaterArea(int x, int y)
        {
            // Track the initial position and water that flows nearby
            for (int dy = 0; dy < 15; dy++) // Check 15 tiles down
            {
                for (int dx = -2; dx <= 2; dx++) // Check 2 tiles to each side
                {
                    int checkX = x + dx;
                    int checkY = y + dy;
                    
                    Tile tile = Main.tile[checkX, checkY];
                    if (tile != null && tile.LiquidAmount > 0 && tile.LiquidType == 0) // Check for water
                    {
                        Point pos = new Point(checkX, checkY);
                        boilingWaterPositions[pos] = BoilingWaterLifetime;
                    }
                }
            }
        }

        void UpdateBoilingWater()
        {
            if (boilingWaterPositions.Count == 0)
                return;

            var positionsToRemove = new List<Point>();
            var newPositionsToTrack = new Dictionary<Point, int>(); // Track water that flows nearby

            foreach (var kvp in boilingWaterPositions.ToList())
            {
                Point pos = kvp.Key;
                int lifetime = kvp.Value;
                Tile tile = Main.tile[pos.X, pos.Y];

                // Decrease lifetime
                lifetime--;
                if (lifetime <= 0)
                {
                    positionsToRemove.Add(pos);
                    // Evaporate the water when boiling effect ends
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        if (tile.LiquidAmount > 0)
                        {
                            tile.LiquidAmount = 0;
                            tile.LiquidType = 0;
                            WorldGen.SquareTileFrame(pos.X, pos.Y);
                            if (Main.netMode == NetmodeID.Server)
                                NetMessage.sendWater(pos.X, pos.Y);
                        }
                    }
                    continue;
                }
                boilingWaterPositions[pos] = lifetime;

                // Check if there's still water at this position
                if (tile.LiquidAmount == 0)
                {
                    positionsToRemove.Add(pos);
                    continue;
                }

                // Track water flowing nearby (check adjacent tiles)
                for (int dy = -1; dy <= 2; dy++) // Check above, same level, and below
                {
                    for (int dx = -2; dx <= 2; dx++) // Check 2 tiles to each side
                    {
                        if (dx == 0 && dy == 0)
                            continue; // Skip the current position
                        
                        int checkX = pos.X + dx;
                        int checkY = pos.Y + dy;
                        Point newPos = new Point(checkX, checkY);
                        
                        // Skip if already tracked
                        if (boilingWaterPositions.ContainsKey(newPos))
                            continue;
                        
                        // Check if this position is within allowed distance from any source
                        bool withinRange = false;
                        foreach (Point source in sourcePositions)
                        {
                            int horizontalDist = System.Math.Abs(checkX - source.X);
                            int verticalDist = checkY - source.Y; // Positive = below source
                            
                            // Only track water that's below or at same level as source, within horizontal range, and not too far down
                            if (verticalDist >= -1 && verticalDist <= MaxVerticalDistance && horizontalDist <= MaxHorizontalSpread)
                            {
                                withinRange = true;
                                break;
                            }
                        }
                        
                        if (!withinRange)
                            continue;
                        
                        Tile checkTile = Main.tile[checkX, checkY];
                        if (checkTile != null && checkTile.LiquidAmount > 0 && checkTile.LiquidType == 0)
                        {
                            // Add new water positions with same lifetime as parent
                            if (!newPositionsToTrack.ContainsKey(newPos))
                            {
                                newPositionsToTrack[newPos] = lifetime;
                            }
                        }
                    }
                }

                // Spawn visual effects (orange/red dust for boiling effect)
                if (Main.rand.NextBool(5))
                {
                    Dust dust = Dust.NewDustDirect(new Vector2(pos.X * 16, pos.Y * 16), 16, 16, 
                        DustID.Torch, 0f, -2f, 100, default, 1.2f);
                    dust.noGravity = true;
                    dust.velocity.Y -= 0.5f;
                }

                // Apply damage to entities in contact with boiling water (server/singleplayer only)
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Rectangle waterRect = new Rectangle(pos.X * 16, pos.Y * 16, 16, 16);

                    // Damage players
                    foreach (Player player in Main.player)
                    {
                        if (player.active && !player.dead && player.Hitbox.Intersects(waterRect) && player.immuneTime <= 0)
                        {
                            player.Hurt(PlayerDeathReason.ByCustomReason(player.name + " was scalded by boiling water."), 20, 0);
                            player.immuneTime = 40; // ~0.66 second cooldown
                        }
                    }

                    // Damage NPCs
                    foreach (NPC npc in Main.npc)
                    {
                        if (npc.active && !npc.friendly && npc.Hitbox.Intersects(waterRect))
                        {
                            // Check if NPC can be damaged (not immune)
                            bool canDamage = true;
                            for (int i = 0; i < npc.immune.Length; i++)
                            {
                                if (npc.immune[i] > 0)
                                {
                                    canDamage = false;
                                    break;
                                }
                            }
                            
                            if (canDamage)
                            {
                                npc.StrikeNPC(npc.CalculateHitInfo(25, 0), false, false);
                                // Set immunity timer for all players (40 ticks = ~0.66 seconds)
                                for (int i = 0; i < npc.immune.Length; i++)
                                {
                                    npc.immune[i] = 40;
                                }
                            }
                        }
                    }
                }
            }

            // Remove expired positions
            foreach (Point pos in positionsToRemove)
            {
                boilingWaterPositions.Remove(pos);
            }

            // Add newly discovered water positions
            foreach (var kvp in newPositionsToTrack)
            {
                if (!boilingWaterPositions.ContainsKey(kvp.Key))
                {
                    boilingWaterPositions[kvp.Key] = kvp.Value;
                }
            }
        }

        public void Activate()
        {
            if (State != AnimationState.Off)
                return;

            if (CooldownTimer > 0)
            {
                // Display cooldown message to the player
                if (Main.netMode != NetmodeID.Server)
                {
                    int secondsRemaining = (int)System.Math.Ceiling(CooldownTimer / 60.0);
                    int minutes = secondsRemaining / 60;
                    int seconds = secondsRemaining % 60;
                    string timeText = minutes > 0 
                        ? $"{minutes}:{seconds:D2}" 
                        : $"{seconds}s";
                    Main.NewText($"Machicolation cooling down... ({timeText} remaining)", 255, 100, 100);
                }
                return;
            }

            StartLow();
        }

        void StartLow()
        {
            State = AnimationState.Low;
            Frame = 0;
            FrameCounter = 0;
            StateTimer = 180;
            // Clear previous water tracking when starting a new cycle
            boilingWaterPositions.Clear();
            sourcePositions.Clear();
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
        }

        void StartMedium()
        {
            State = AnimationState.Medium;
            Frame = 0;
            FrameCounter = 0;
            StateTimer = 180;
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
        }

        void StartHigh()
        {
            State = AnimationState.High;
            Frame = 0;
            FrameCounter = 0;
            StateTimer = 0;
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
        }

        void Stop()
        {
            State = AnimationState.Off;
            Frame = 0;
            FrameCounter = 0;
            StateTimer = 0;
            CooldownTimer = CooldownDuration; // Start cooldown after use
            // Note: Don't clear boilingWaterPositions or sourcePositions here - let water evaporate naturally
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, 10);
                NetMessage.SendData(MessageID.TileEntityPlacement, number: i, number2: j, number3: Type, number4: 0f);
                return -1;
            }
            return Place(i, j);
        }

        public override void OnKill()
        {
        }

        public override bool IsTileValidForEntity(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            return tile.HasTile && tile.TileType == ModContent.TileType<MacchicolationTile>() && tile.TileFrameX == 0 && tile.TileFrameY == 0;
        }

        public override void SaveData(TagCompound tag)
        {
            tag["s"] = (int)State;
            tag["f"] = Frame;
            tag["fc"] = FrameCounter;
            tag["t"] = StateTimer;
            tag["cd"] = CooldownTimer;
        }

        public override void LoadData(TagCompound tag)
        {
            State = (AnimationState)(int)tag.Get<int>("s");
            Frame = tag.Get<int>("f");
            FrameCounter = tag.Get<int>("fc");
            StateTimer = tag.Get<int>("t");
            CooldownTimer = tag.Get<int>("cd");
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write((byte)State);
            writer.Write((short)Frame);
            writer.Write((short)FrameCounter);
            writer.Write((short)StateTimer);
            writer.Write(CooldownTimer);
        }

        public override void NetReceive(BinaryReader reader)
        {
            State = (AnimationState)reader.ReadByte();
            Frame = reader.ReadInt16();
            FrameCounter = reader.ReadInt16();
            StateTimer = reader.ReadInt16();
            CooldownTimer = reader.ReadInt32();
        }
    }
}
