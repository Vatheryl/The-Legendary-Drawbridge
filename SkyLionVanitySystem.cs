using Terraria.ModLoader;
using Terraria;

namespace Drawbridge
{
	public class SkyLionVanitySystem : ModSystem
	{
		public static int SkyLionHeadSlot = -1;
		public static int SkyLionBodySlot = -1;
		public static int SkyLionBodyWithCloakSlot = -1;
		public static int SkyLionLegsSlot = -1;
		public static int SkyLionCloakFrontSlot = -1;
		public static int SkyLionCloakBackSlot = -1;

		public override void Load()
		{
			if (Main.dedServ)
				return;
				
			SkyLionHeadSlot = EquipLoader.AddEquipTexture(Mod, "Drawbridge/Textures/SkyLionSet/SkyLionMane_Head", EquipType.Head, name: nameof(SkyLionHeadSlot));
			SkyLionBodySlot = EquipLoader.AddEquipTexture(Mod, "Drawbridge/Textures/SkyLionSet/SkyLionCuirass_Bod", EquipType.Body, name: nameof(SkyLionBodySlot));
			SkyLionBodyWithCloakSlot = EquipLoader.AddEquipTexture(Mod, "Drawbridge/Textures/SkyLionSet/SkyLionCuirass_WithCloak", EquipType.Body, name: nameof(SkyLionBodyWithCloakSlot));
			SkyLionLegsSlot = EquipLoader.AddEquipTexture(Mod, "Drawbridge/Textures/SkyLionSet/SkyLionGreaves_Legs", EquipType.Legs, name: nameof(SkyLionLegsSlot));
			SkyLionCloakFrontSlot = EquipLoader.AddEquipTexture(Mod, "Drawbridge/Textures/SkyLionSet/SkyLionCloak_Front", EquipType.Front, name: nameof(SkyLionCloakFrontSlot));
			SkyLionCloakBackSlot = EquipLoader.AddEquipTexture(Mod, "Drawbridge/Textures/SkyLionSet/SkyLionCloak_Back", EquipType.Back, name: nameof(SkyLionCloakBackSlot));
		}
	}
}
