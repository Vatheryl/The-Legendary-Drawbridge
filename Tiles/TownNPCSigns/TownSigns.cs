using Terraria.ID;

namespace Drawbridge.Tiles.TownNPCSigns;

[LegacyName("Fish_Sign_Placed")]
public class AnglerSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "Fish_Sign_Item");

    public override void AddItemRecipes(ModItem item) => item.CreateRecipe().AddIngredient(ItemID.Wood, 8).AddIngredient(ItemID.Bass, 1).AddTile(TileID.WorkBenches).Register();
}

[LegacyName("TinkererSignTile")]
public class AnvilSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "AnvilSignItem", "TinkererSignItem");
}

public class BlankSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "BlankSignItem");
}

public class ClothierSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "ClothierSignItem");
}

public class CyberSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "CyberSignItem");
}

public class DemolitionistSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "DemolitionistSignItem");
}

public class DryadSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "DryadSignItem");
}

public class DyeSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "DyeSignItem");
}

public class GolfSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "GolfSignItem");
}

public class GunSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "GunSignItem");
}

public class MechanicSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "MechanicSignItem");
}

public class MerchantSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "MerchantSignItem");
}

public class PirateSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "PirateSignItem");
}

public class PrincessSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "PrincessSignItem");

    public override void AddItemRecipes(ModItem item) => item.CreateRecipe().AddIngredient(ItemID.Wood, 8).AddIngredient(ItemID.PinkGel, 1).AddTile(TileID.WorkBenches).Register();
}

public class SteamSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "SteamSignItem");
}

public class StylistSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "StylistSignItem");
}

public class TruffleSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "TruffleSignItem");
}

public class ZooSignTile : BaseSignTile
{
    public override void StaticItemDefaults(ModItem item) => ModTypeLookup<ModItem>.RegisterLegacyNames(item, "ZooSignItem");
}

public class NurseSignTile : BaseSignTile
{
}

public class PainterSignTile : BaseSignTile
{
}

public class PartySignTile : BaseSignTile
{
}

public class TavernSignTile : BaseSignTile
{
}

public class TaxSignTile : BaseSignTile
{
}

public class WitchDoctorSignTile : BaseSignTile
{
}