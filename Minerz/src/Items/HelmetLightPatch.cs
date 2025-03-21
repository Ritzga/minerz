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
                var id = __instance.PlayerUID;
                var inventory = __instance.World.PlayerByUid(id)?.InventoryManager?.Inventories[$"character-{id}"]; //for 1.20
                var itemSlot = inventory?.Count > 20? inventory?[31] : inventory?[12];
                if (itemSlot?.Itemstack is { Item.Code.Path: "armor-head-miner" or "armor-head-miner-enhanced"})
                {
                    __result = itemSlot.Itemstack.Collectible.Attributes["itemlight"].AsArray<byte>();
                    return __result;
                }
            }
            catch
            {
                // ignored
            }
            return __result;
        }
        
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