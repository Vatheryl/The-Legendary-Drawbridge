using Drawbridge.Tiles;
using Terraria.ID;
using Drawbridge.Tiles.TownNPCSigns;
using Drawbridge.Common.Tiles;

namespace Drawbridge.Players;

public class DrawbridgeGlobalNPC : GlobalNPC
{
    private Vector2 lastSafePosition;
    private int spikeDamageTimer = 0;

    public override bool InstancePerEntity => true;

    public override void PostAI(NPC npc)
    {
        int tileX = (int)(npc.position.X + npc.width / 2) / 16;
        int tileY = (int)(npc.position.Y + npc.height / 2) / 16;

        // Bounds check to prevent IndexOutOfRangeException
        if (tileX < 0 || tileX >= Main.maxTilesX || tileY < 0 || tileY >= Main.maxTilesY)
        {
            lastSafePosition = npc.position;
            return;
        }

        var tile = Main.tile[tileX, tileY];
        if (tile != null && tile.HasTile && ModContent.GetModTile(tile.TileType) is PortcullisDownTile or SpikedPallisadeTile or SpikedPallisadeFlippedTile)
        {
            bool block = false;

            if (tile.TileType == ModContent.TileType<SpikedPallisadeTile>() || tile.TileType == ModContent.TileType<SpikedPallisadeFlippedTile>())
            {
                // Skip all collision and damage logic for bosses
                if (npc.boss)
                {
                    lastSafePosition = npc.position;
                    return;
                }
                
                bool isFlipped = tile.TileType == ModContent.TileType<SpikedPallisadeFlippedTile>();
                bool facingLeft = (tile.TileFrameX >= 48);
                if (isFlipped) 
                    facingLeft = !facingLeft;

                // Find the top-left of the multitile
                // Remove oddPlacement logic
                // int localX = (int)(npc.position.X + npc.width / 2) % 48;
                // int localY = (int)(npc.position.Y + npc.height / 2) % 96;
                int localX = (int)(npc.position.X + npc.width / 2) % 48;
                int localY = (int)(npc.position.Y + npc.height / 2) % 96;
                // Remove shifting for oddPlacement
                if (facingLeft) 
                    localX = 47 - localX;

                int localTileX = localX / 16 + 1; // 1-based
                int localTileY = localY / 16 + 1; // 1-based
                // Solid: column 1, row 1, or 3x3 area top-left, plus [2,4] and [3,4]
                bool isSolid = (localTileX == 1 || localTileY == 1 || (localTileX >= 1 && localTileX <= 3 && localTileY >= 1 && localTileY <= 3) ||
                    (localTileX == 2 && localTileY == 4) || (localTileX == 3 && localTileY == 4));

                if (isSolid && !tile.IsActuated)
                    block = true;

                // All solid tiles are also damaging, but only if not actuated
                if (isSolid && !tile.IsActuated)
                {
                    spikeDamageTimer++;
                    if (spikeDamageTimer >= 30)
                    {
                        NPC.HitInfo hit = new NPC.HitInfo();
                        hit.Damage = 50;
                        npc.StrikeNPC(hit);
                        npc.AddBuff(BuffID.Bleeding, 600);
                        spikeDamageTimer = 0;
                    }
                }
            }
            else
                block = true;

            if (block)
            {
                npc.position = lastSafePosition;
                npc.velocity = Vector2.Zero;
            }
            else
                lastSafePosition = npc.position;
        }
        else
        {
            lastSafePosition = npc.position;
            // Do not reset the timer when not in contact
        }
    }

    public override void OnKill(NPC npc)
    {
        if (npc.lastInteraction != -1)
        {
            Player player = Main.player[npc.lastInteraction];

            if (player.GetModPlayer<DrawbridgePlayer>().hasGoldenResonance)
            {
                if (npc.value > 0)
                {
                    int extraCoins = (int)(npc.value * 0.2f);

                    if (extraCoins > 0)
                        Item.NewItem(npc.GetSource_Loot(), npc.getRect(), ItemID.CopperCoin, extraCoins);
                }
            }
        }
    }

    public override void ModifyShop(NPCShop shop)
    {
        AddQuick<MerchantSignTile>(NPCID.Merchant);
        AddQuick<PrincessSignTile>(NPCID.Merchant, NPCID.Princess);
        AddQuick<AnglerSignTile>(NPCID.Merchant, NPCID.Angler);
        AddQuick<TaxSignTile>(NPCID.Merchant, NPCID.TaxCollector);
        AddQuick<TaxSignTile>(NPCID.Merchant, NPCID.TaxCollector);

        AddQuick<GunSignTile>(NPCID.ArmsDealer);
        AddQuick<DryadSignTile>(NPCID.Dryad);
        AddQuick<DemolitionistSignTile>(NPCID.Demolitionist);
        AddQuick<ClothierSignTile>(NPCID.Clothier);
        AddQuick<AnvilSignTile>(NPCID.GoblinTinkerer);
        AddQuick<MechanicSignTile>(NPCID.Mechanic);
        AddQuick<PirateSignTile>(NPCID.Pirate);
        AddQuick<SteamSignTile>(NPCID.Steampunker);
        AddQuick<DyeSignTile>(NPCID.DyeTrader);
        AddQuick<TruffleSignTile>(NPCID.Truffle);
        AddQuick<CyberSignTile>(NPCID.Cyborg);
        AddQuick<GolfSignTile>(NPCID.Golfer);
        AddQuick<ZooSignTile>(NPCID.BestiaryGirl);

        AddQuick<NurseSignTile>(NPCID.Nurse);
        AddQuick<PartySignTile>(NPCID.PartyGirl);
        AddQuick<PainterSignTile>(NPCID.Painter);
        AddQuick<TavernSignTile>(NPCID.DD2Bartender);
        AddQuick<WitchDoctorSignTile>(NPCID.WitchDoctor);

        return;

        void AddQuick<T>(int type, int hasNpc = -1) where T : BaseSignTile
        {
            if (shop.NpcType == type)
            {
                Condition[] conditions = hasNpc == -1 ? [] : [Condition.NpcIsPresent(hasNpc)];
                shop.Add(ModContent.GetInstance<T>().ItemType(), conditions);
            }
        }
    }
}
