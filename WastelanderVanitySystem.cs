using Terraria.ModLoader;
using Terraria;

namespace Drawbridge
{
	public class WastelanderVanitySystem : ModSystem
	{
		public static int WastelanderHeadSlot = -1;
		public static int WastelanderBodySlot = -1;
		public static int WastelanderLegsSlot = -1;

		public override void Load()
		{
			if (Main.dedServ)
				return;
				
			WastelanderHeadSlot = EquipLoader.AddEquipTexture(Mod, "Drawbridge/Textures/WasteLanderSet/Wastelander_Hood", EquipType.Head, name: "WastelanderHood");
			WastelanderBodySlot = EquipLoader.AddEquipTexture(Mod, "Drawbridge/Textures/WasteLanderSet/Wastelander_Chestplate_Sheet", EquipType.Body, name: "WastelanderChestplate");
			WastelanderLegsSlot = EquipLoader.AddEquipTexture(Mod, "Drawbridge/Textures/WasteLanderSet/Wastelander_Boots_Sheet", EquipType.Legs, name: "WastelanderBoots");
		}
	}
}

