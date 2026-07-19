using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;
using System.Collections.Generic;
using System.Linq;

namespace Drawbridge.Tiles;

public class DoorcaseTile : ModTile
{
    private const int TELEPORT_COOLDOWN = 60; // 1 second at 60 FPS

    // Track animation state for each doorcase
    private static readonly Dictionary<Point, DoorcaseState> doorcaseStates = [];
    
    // Track teleporter pairs - stores connections between doorcases
    private static readonly Dictionary<Point, Point> teleporterConnections = [];
    private static readonly List<Point> availableDoorcases = [];
    
    // Teleportation cooldown to prevent spam teleporting
    private static readonly Dictionary<int, int> playerTeleportCooldown = [];

    private class DoorcaseState
    {
        public const int ANIMATION_SPEED = 4; // ticks per frame (72 ticks total ÷ 18 frames = 4 ticks/frame for 1.2s duration)
        public const int TOTAL_FRAMES = 18;

        public int currentFrame = 0;
        public bool isAnimating = false;
        public bool isOpening = true; // true = opening, false = closing
        public int animationTimer = 0;
    }

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileSolid[Type] = false;
        Main.tileSolidTop[Type] = false;
        TileID.Sets.DisableSmartCursor[Type] = true;

        // Copy from Style3x4 (like vanilla bookcases)
        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
        TileObjectData.newTile.StyleWrapLimitVisualOverride = 1; // Only one style variant
        TileObjectData.addTile(Type);

        // Enable animation - each frame is 70px tall (3x4 tiles with padding)
        AnimationFrameHeight = 70;

        // Bookcases count as furniture for housing
        AddMapEntry(new Color(120, 85, 60), CreateMapEntryName());
        DustType = DustID.WoodFurniture;
        HitSound = SoundID.Dig;
    }

    public override bool RightClick(int i, int j)
    {
        // Find ALL tiles of this type in a 3x4 area around the clicked tile
        // This ensures we find the complete structure regardless of where we click
        Point? structureKey = null;
        int topLeftX = -1, topLeftY = -1;
        
        // Search in a 3x4 area around the clicked tile to find the structure bounds
        for (int searchX = i - 2; searchX <= i + 2; searchX++)
        {
            for (int searchY = j - 3; searchY <= j + 3; searchY++)
            {
                if (searchX < 0 || searchY < 0 || searchX >= Main.maxTilesX || searchY >= Main.maxTilesY)
                    continue;
                    
                Tile searchTile = Main.tile[searchX, searchY];
                if (searchTile != null && searchTile.HasTile && searchTile.TileType == Type)
                {
                    // Found a tile of our type, calculate what would be the top-left if this is part of our structure
                    int frameX = searchTile.TileFrameX / 18;
                    int frameY = searchTile.TileFrameY / 18;
                    
                    int candidateTopLeftX = searchX - (frameX % 3);
                    int candidateTopLeftY = searchY - (frameY % 4);
                    
                    // Check if the clicked tile would be within this structure
                    if (i >= candidateTopLeftX && i < candidateTopLeftX + 3 && 
                        j >= candidateTopLeftY && j < candidateTopLeftY + 4)
                    {
                        topLeftX = candidateTopLeftX;
                        topLeftY = candidateTopLeftY;
                        structureKey = new Point(topLeftX, topLeftY);
                        break;
                    }
                }
            }

            if (structureKey.HasValue) 
                break;
        }
        
        if (!structureKey.HasValue)
            return false; // Couldn't find the structure
        
        Point key = structureKey.Value;
        
        // Get or create state for this doorcase
        if (!doorcaseStates.TryGetValue(key, out DoorcaseState state))
        {
            state = new DoorcaseState();
            doorcaseStates[key] = state;
        }

        // Don't allow new interactions while animating
        if (state.isAnimating)
            return true;

        // Start animation
        state.isAnimating = true;
        state.animationTimer = 0;

        // Determine direction based on current frame
        if (state.currentFrame == 0)
        {
            // Closed, so open
            state.isOpening = true;
        }
        else if (state.currentFrame == DoorcaseState.TOTAL_FRAMES - 1)
        {
            // Fully open, so close
            state.isOpening = false;
        }

        // Play appropriate sound based on opening/closing direction
        string soundPath = state.isOpening ? "Drawbridge/Sounds/DoorcaseOpen" : "Drawbridge/Sounds/DoorcaseClosed";
        SoundEngine.PlaySound(new SoundStyle(soundPath) { Volume = 0.8f }, new Vector2((topLeftX + 1.5f) * 16, (topLeftY + 2f) * 16));

        return true;
    }

    public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
    {
        // Find the structure this tile belongs to
        Point? structureKey = null;

        // Search for the structure this tile belongs to
        for (int searchX = i - 2; searchX <= i + 2; searchX++)
        {
            for (int searchY = j - 3; searchY <= j + 3; searchY++)
            {
                if (searchX < 0 || searchY < 0 || searchX >= Main.maxTilesX || searchY >= Main.maxTilesY)
                    continue;
                    
                Tile searchTile = Main.tile[searchX, searchY];
                if (searchTile != null && searchTile.HasTile && searchTile.TileType == Type)
                {
                    int frameXSearch = searchTile.TileFrameX / 18;
                    int frameYSearch = searchTile.TileFrameY / 18;
                    
                    int candidateTopLeftX = searchX - (frameXSearch % 3);
                    int candidateTopLeftY = searchY - (frameYSearch % 4);
                    
                    // Check if this tile is within this structure
                    if (i >= candidateTopLeftX && i < candidateTopLeftX + 3 && j >= candidateTopLeftY && j < candidateTopLeftY + 4)
                    {
                        int topLeftX = candidateTopLeftX;
                        int topLeftY = candidateTopLeftY;
                        structureKey = new Point(topLeftX, topLeftY);
                        break;
                    }
                }
            }

            if (structureKey.HasValue) 
                break;
        }
        
        if (!structureKey.HasValue)
            return; // Couldn't find the structure
        
        Point key = structureKey.Value;
        
        // Get the state for this doorcase
        if (!doorcaseStates.TryGetValue(key, out DoorcaseState state))
        {
            // No animation state, no offset needed
            return;
        }

        // Apply frame offset based on current animation frame
        // frameYOffset is already set to AnimationFrameHeight * Main.tileFrame[type] (which is 0)
        // We need to add our custom animation offset
        frameYOffset += state.currentFrame * 70;
    }

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        // Clean up state when tile is broken
        Tile tile = Main.tile[i, j];
        int tileFrameX = tile.TileFrameX / 18;
        int tileFrameY = tile.TileFrameY / 18;
        
        int topLeftX = i - (tileFrameX % 3);
        int topLeftY = j - (tileFrameY % 4);
        
        Point key = new Point(topLeftX, topLeftY);
        
        // Clean up all state related to this doorcase
        doorcaseStates.Remove(key);
        UnregisterDoorcaseFromTeleportation(key);

        Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 48, 64, ModContent.ItemType<Items.DoorcaseItem>());
    }

    public override bool CanDrop(int i, int j) => false;

    // Check if a doorcase is fully open (ready for teleportation)
    private bool IsDoorcaseOpen(Point structureKey)
    {
        if (!doorcaseStates.TryGetValue(structureKey, out DoorcaseState state))
            return false;
        
        return state.currentFrame >= DoorcaseState.TOTAL_FRAMES - 1 && !state.isAnimating;
    }

    // Register a doorcase for teleportation when it's placed (regardless of open/closed state)
    private void RegisterDoorcaseForTeleportation(Point structureKey)
    {
        if (!availableDoorcases.Contains(structureKey))
        {
            availableDoorcases.Add(structureKey);
            UpdateTeleporterConnections();
        }
    }

    // Unregister a doorcase from teleportation
    private void UnregisterDoorcaseFromTeleportation(Point structureKey)
    {
        if (availableDoorcases.Contains(structureKey))
        {
            availableDoorcases.Remove(structureKey);
            
            // Remove any connections involving this doorcase
            if (teleporterConnections.ContainsKey(structureKey))
                teleporterConnections.Remove(structureKey);
            
            var keysToRemove = teleporterConnections.Where(pair => pair.Value.Equals(structureKey)).Select(pair => pair.Key).ToList();
            foreach (var key in keysToRemove)
                teleporterConnections.Remove(key);
            
            UpdateTeleporterConnections();
        }
    }

    // Update teleporter connections - pairs up all available doorcases (regardless of open/closed state)
    private void UpdateTeleporterConnections()
    {
        teleporterConnections.Clear();
        
        // Pair up all available doorcases for teleportation (they don't all need to be open)
        for (int i = 0; i < availableDoorcases.Count - 1; i += 2)
        {
            Point firstDoorcase = availableDoorcases[i];
            Point secondDoorcase = availableDoorcases[i + 1];
            
            teleporterConnections[firstDoorcase] = secondDoorcase;
            teleporterConnections[secondDoorcase] = firstDoorcase;
        }
    }

    // Check for players near open doorcases and teleport them
    public override void NearbyEffects(int i, int j, bool closer)
    {
        if (!closer) 
            return;
        
        // Find the structure this tile belongs to
        Point? structureKey = FindStructureKey(i, j);
        if (!structureKey.HasValue) 
            return;
        
        Point key = structureKey.Value;
        
        // Only process effects at the center of the doorcase to avoid multiple triggers
        if (i != key.X + 1 || j != key.Y + 2) 
            return;
        
        // Register this doorcase for teleportation (happens when any player gets near any doorcase)
        RegisterDoorcaseForTeleportation(key);
        
        // Only allow teleportation if THIS doorcase is open (origin doorcase must be open)
        if (!IsDoorcaseOpen(key)) 
            return;
        
        // Check if we have a teleportation destination
        if (!teleporterConnections.TryGetValue(key, out Point destinationKey))
            return;
        
        // Check for players within the doorcase opening area (not the bottom row)
        // The doorcase is 3x4 tiles, we want to detect in the upper 3 rows only
        Rectangle doorcaseArea = new(key.X * 16, key.Y * 16, 48, 48);
        
        foreach (Player player in Main.player)
        {
            if (!player.active || player.dead) 
                continue;
            
            // Check cooldown
            if (playerTeleportCooldown.ContainsKey(player.whoAmI) && 
                playerTeleportCooldown[player.whoAmI] > 0)
                continue;
            
            // Check if player is within the doorcase area
            if (player.Hitbox.Intersects(doorcaseArea))
            {
                TeleportPlayer(player, key, destinationKey);
            }
        }
    }

    // Teleport a player from one doorcase to another
    private void TeleportPlayer(Player player, Point fromKey, Point toKey)
    {
        // Set teleport cooldown
        playerTeleportCooldown[player.whoAmI] = TELEPORT_COOLDOWN;
        
        // Calculate destination position (center of destination doorcase, at floor level)
        // Position player at the bottom center of the doorcase (3rd row of tiles)
        // toKey.Y + 3 is the tile row below the doorcase where player should stand
        Vector2 destinationPos = new Vector2((toKey.X + 1.5f) * 16, (toKey.Y + 3) * 16 - player.height);
        
        // Create dark dust at origin before teleporting
        CreateTeleportDust(new Vector2((fromKey.X + 1.5f) * 16, (fromKey.Y + 2f) * 16));
        
        // Teleport the player (without teleporter particles)
        player.teleporting = true;
        player.Teleport(destinationPos, TeleportationStyleID.DebugTeleport); // Use debug style for no particles
        
        // Create dark dust at destination
        CreateTeleportDust(destinationPos);
        
        // Close both doorcases with animation and sound
        CloseDoorcaseAfterTeleport(fromKey);
        CloseDoorcaseAfterTeleport(toKey);
        
        // Network sync if multiplayer
        if (Main.netMode != NetmodeID.SinglePlayer)
        {
            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, player.whoAmI, destinationPos.X, destinationPos.Y, TeleportationStyleID.DebugTeleport);
        }
    }

    // Create dark dust effect for teleportation (no particles, just dust)
    private void CreateTeleportDust(Vector2 position)
    {
        int dustCount = 25;
        
        for (int i = 0; i < dustCount; i++)
        {
            // Create dark smoke-like dust
            Vector2 dustPos = position + new Vector2(Main.rand.Next(-24, 25), Main.rand.Next(-24, 25));
            Vector2 dustVel = new Vector2(Main.rand.Next(-3, 4), Main.rand.Next(-4, 1)) * 0.5f;
            
            // Use smoke dust for dark theme
            Dust dust = Dust.NewDustPerfect(dustPos, DustID.Smoke, dustVel, 100, Color.Black, Main.rand.NextFloat(1.0f, 1.8f));
            dust.noGravity = true;
            dust.fadeIn = 1.2f;
            
            // Add some purple-tinted dust for magical effect
            if (Main.rand.NextBool(3))
            {
                Dust magicDust = Dust.NewDustPerfect(dustPos, DustID.PurpleTorch, dustVel * 0.8f, 120, Color.Purple * 0.7f, Main.rand.NextFloat(0.8f, 1.3f));
                magicDust.noGravity = true;
                magicDust.fadeIn = 1.0f;
            }
            
            // Add some shadowy dust
            if (Main.rand.NextBool(4))
            {
                Dust shadowDust = Dust.NewDustPerfect(dustPos, DustID.Shadowflame, dustVel * 0.6f, 80, Color.DarkGray * 0.8f, Main.rand.NextFloat(0.6f, 1.1f));
                shadowDust.noGravity = true;
            }
        }
    }

    // Close a doorcase after teleportation with animation and sound
    private void CloseDoorcaseAfterTeleport(Point structureKey)
    {
        // Get or create state for this doorcase
        if (!doorcaseStates.TryGetValue(structureKey, out DoorcaseState state))
        {
            state = new DoorcaseState();
            doorcaseStates[structureKey] = state;
        }

        // Force closing animation regardless of current state (even if already closed or animating)
        // If already closed, we'll animate from closed to closed to show the effect
        if (state.currentFrame == 0)
        {
            // Already closed, so animate it opening slightly then closing again for effect
            state.currentFrame = 3; // Start a few frames in to show movement
        }
        
        state.isAnimating = true;
        state.animationTimer = 0;
        state.isOpening = false; // Always closing

        // Play closing sound
        string soundPath = "Drawbridge/Sounds/DoorcaseClosed";
        Vector2 soundPos = new Vector2((structureKey.X + 1.5f) * 16, (structureKey.Y + 2f) * 16);
        SoundEngine.PlaySound(new SoundStyle(soundPath) { Volume = 0.8f }, soundPos);
    }

    // Helper method to find which structure a tile belongs to
    private Point? FindStructureKey(int i, int j)
    {
        // Search for the structure this tile belongs to
        for (int searchX = i - 2; searchX <= i + 2; searchX++)
        {
            for (int searchY = j - 3; searchY <= j + 3; searchY++)
            {
                if (searchX < 0 || searchY < 0 || searchX >= Main.maxTilesX || searchY >= Main.maxTilesY)
                    continue;
                    
                Tile searchTile = Main.tile[searchX, searchY];
                if (searchTile != null && searchTile.HasTile && searchTile.TileType == Type)
                {
                    int frameXSearch = searchTile.TileFrameX / 18;
                    int frameYSearch = searchTile.TileFrameY / 18;
                    
                    int candidateTopLeftX = searchX - (frameXSearch % 3);
                    int candidateTopLeftY = searchY - (frameYSearch % 4);
                    
                    // Check if this tile is within this structure
                    if (i >= candidateTopLeftX && i < candidateTopLeftX + 3 && 
                        j >= candidateTopLeftY && j < candidateTopLeftY + 4)
                    {
                        return new Point(candidateTopLeftX, candidateTopLeftY);
                    }
                }
            }
        }

        return null;
    }

    // Global update for cooldowns
    public override void AnimateTile(ref int frame, ref int frameCounter)
    {
        // Update player teleport cooldowns
        var playersToUpdate = new List<int>(playerTeleportCooldown.Keys);
        foreach (int playerID in playersToUpdate)
        {
            if (playerTeleportCooldown[playerID] > 0)
                playerTeleportCooldown[playerID]--;
            else
                playerTeleportCooldown.Remove(playerID);
        }

        // This controls the global animation frame for all tiles of this type
        // We need to update individual doorcase states and use the global frame
        
        // Update all active doorcase animations
        var keysToUpdate = new List<Point>(doorcaseStates.Keys);
        foreach (var key in keysToUpdate)
        {
            var state = doorcaseStates[key];
            if (state.isAnimating)
            {
                state.animationTimer++;
                
                if (state.animationTimer >= DoorcaseState.ANIMATION_SPEED)
                {
                    state.animationTimer = 0;
                    
                    if (state.isOpening)
                    {
                        state.currentFrame++;
                        if (state.currentFrame >= DoorcaseState.TOTAL_FRAMES - 1)
                        {
                            state.currentFrame = DoorcaseState.TOTAL_FRAMES - 1;
                            state.isAnimating = false;
                        }
                    }
                    else
                    {
                        state.currentFrame--;
                        if (state.currentFrame <= 0)
                        {
                            state.currentFrame = 0;
                            state.isAnimating = false;
                        }
                    }
                }
            }
        }
        
        // Keep the global frame at 0 since we handle individual animation in AnimateIndividualTile
        frame = 0;
    }
}
