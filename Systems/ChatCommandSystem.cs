using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Drawbridge.Tiles.Drawbridges;

namespace Drawbridge.Systems
{
    public class ChatCommandSystem : ModSystem
    {
        private string lastChatText = "";
        private bool wasChatOpen = false;

        public override void PostUpdateEverything()
        {
            // Check for chat commands
            CheckForChatCommands();
        }

        private void CheckForChatCommands()
        {
            // Only process if we're in a world and have a local player
            if (!Main.gameMenu && Main.LocalPlayer != null)
            {
                // Check if Easter egg is disabled in server config
                var config = ModContent.GetInstance<DrawbridgeConfig>();
                if (config.DisableEasterEgg)
                {
                    return; // Skip processing if disabled
                }

                bool isChatOpen = Main.chatRelease;
                string currentChatText = Main.chatText;
                
                // Check if chat was just closed (message was sent)
                if (wasChatOpen && !isChatOpen && !string.IsNullOrEmpty(lastChatText))
                {
                    // Process the chat command
                    ProcessChatCommand(lastChatText);
                }
                
                // Update our tracking variables
                wasChatOpen = isChatOpen;
                lastChatText = currentChatText;
            }
        }

        private void ProcessChatCommand(string message)
        {
            // Check for our specific commands
            if (message == "when im with you")
            {
                TriggerNearestHorizontalBridge();
            }
            else if (message == "when im without you")
            {
                TriggerNearestVerticalBridge();
            }
        }

        private void TriggerNearestHorizontalBridge()
        {
            // Find the nearest horizontal bridge
            var horizontalBridges = FindHorizontalBridges();
            if (horizontalBridges.Count > 0)
            {
                var nearest = horizontalBridges.OrderBy(bridge => 
                    Vector2.DistanceSquared(Main.LocalPlayer.position, new Vector2(bridge.X * 16, bridge.Y * 16))).First();
                
                // Trigger the bridge
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    PacketHandler.SendContextPacket(MessageType.ToggleDrawbridge, nearest.X, nearest.Y);
                }
                else
                {
                    TriggerBridgeAt(nearest.X, nearest.Y);
                }
            }
        }

        private void TriggerNearestVerticalBridge()
        {
            // Find the nearest vertical bridge
            var verticalBridges = FindVerticalBridges();
            if (verticalBridges.Count > 0)
            {
                var nearest = verticalBridges.OrderBy(bridge => 
                    Vector2.DistanceSquared(Main.LocalPlayer.position, new Vector2(bridge.X * 16, bridge.Y * 16))).First();
                
                // Trigger the bridge
                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    PacketHandler.SendContextPacket(MessageType.ToggleDrawbridge, nearest.X, nearest.Y);
                }
                else
                {
                    TriggerBridgeAt(nearest.X, nearest.Y);
                }
            }
        }

        private List<Point> FindHorizontalBridges()
        {
            var bridges = new List<Point>();
            int horizontalTileType = ModContent.TileType<DrawbridgeHorizontalTile>();
            int horizontalFlippedTileType = ModContent.TileType<DrawbridgeHorizontalFlippedTile>();

            // Search in a reasonable radius around the player
            int searchRadius = 100; // tiles
            int playerTileX = (int)(Main.LocalPlayer.position.X + Main.LocalPlayer.width / 2) / 16;
            int playerTileY = (int)(Main.LocalPlayer.position.Y + Main.LocalPlayer.height / 2) / 16;

            for (int x = playerTileX - searchRadius; x <= playerTileX + searchRadius; x++)
            {
                for (int y = playerTileY - searchRadius; y <= playerTileY + searchRadius; y++)
                {
                    if (WorldGen.InWorld(x, y))
                    {
                        Tile tile = Main.tile[x, y];
                        if (tile.HasTile && (tile.TileType == horizontalTileType || tile.TileType == horizontalFlippedTileType))
                        {
                            bridges.Add(new Point(x, y));
                        }
                    }
                }
            }

            return bridges;
        }

        private List<Point> FindVerticalBridges()
        {
            var bridges = new List<Point>();
            int verticalTileType = ModContent.TileType<DrawbridgeTile>();
            int verticalFlippedTileType = ModContent.TileType<DrawbridgeVerticalFlippedTile>();

            // Search in a reasonable radius around the player
            int searchRadius = 100; // tiles
            int playerTileX = (int)(Main.LocalPlayer.position.X + Main.LocalPlayer.width / 2) / 16;
            int playerTileY = (int)(Main.LocalPlayer.position.Y + Main.LocalPlayer.height / 2) / 16;

            for (int x = playerTileX - searchRadius; x <= playerTileX + searchRadius; x++)
            {
                for (int y = playerTileY - searchRadius; y <= playerTileY + searchRadius; y++)
                {
                    if (WorldGen.InWorld(x, y))
                    {
                        Tile tile = Main.tile[x, y];
                        if (tile.HasTile && (tile.TileType == verticalTileType || tile.TileType == verticalFlippedTileType))
                        {
                            bridges.Add(new Point(x, y));
                        }
                    }
                }
            }

            return bridges;
        }

        private void TriggerBridgeAt(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            if (!tile.HasTile) return;

            // Call the appropriate Toggle method based on tile type
            if (tile.TileType == ModContent.TileType<DrawbridgeHorizontalTile>())
            {
                ModContent.GetInstance<DrawbridgeHorizontalTile>().Toggle(x, y);
            }
            else if (tile.TileType == ModContent.TileType<DrawbridgeHorizontalFlippedTile>())
            {
                ModContent.GetInstance<DrawbridgeHorizontalFlippedTile>().Toggle(x, y);
            }
            else if (tile.TileType == ModContent.TileType<DrawbridgeTile>())
            {
                ModContent.GetInstance<DrawbridgeTile>().Toggle(x, y);
            }
            else if (tile.TileType == ModContent.TileType<DrawbridgeVerticalFlippedTile>())
            {
                ModContent.GetInstance<DrawbridgeVerticalFlippedTile>().Toggle(x, y);
            }
        }
    }
} 