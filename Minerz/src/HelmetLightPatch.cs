using HarmonyLib;
using Vintagestory.API.Common;

namespace Minerz;

[HarmonyPatch]
public class HelmetLightPatch
{
    //-- Changes the light of the player based on the item lights that may be in their hand --//
    [HarmonyPatch(typeof(EntityPlayer), "LightHsv", MethodType.Getter)]
    public class ItemHandLight
    {
        [HarmonyPostfix]
        static private byte[] LightHsv(byte[] __result, EntityPlayer __instance)
        {
            try
            {
                //search for head
                var itemStack = __instance.GearInventory[12]?.Itemstack;
                if (itemStack is { Item.Code.Path: "armor-head-miner" or "armor-head-miner-enhanced"})
                {
                    return itemStack.Collectible.Attributes["itemlight"].AsArray<byte>();
                }
            }
            catch
            {
                // ignored
            }
            return __result;
        }

        //-- Sets the entity light of the dropped item to be the light attribute if it exists --//
        [HarmonyPatch(typeof(EntityItem), "LightHsv", MethodType.Getter)]
        public class ItemEntityLight
        {
            [HarmonyPostfix]
            static private byte[] Postfix(byte[] __result, EntityItem __instance)
            {
                try
                {
                    var finalLight = __result;

                    if (__instance.Itemstack.Collectible.Attributes != null)
                        if (__instance.Itemstack.Collectible.Attributes.KeyExists("itemlight"))
                            finalLight = __instance.Itemstack.Collectible.Attributes["itemlight"].AsArray<byte>();
                    return finalLight;
                }
                catch
                {
                    return __result;
                }
            }
        }
    }
}