using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Iced.Intel;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Minerz;

[HarmonyPatch]
public class BetterCharacter
{
    public static double miningSpeedMul = 0.2;
    public static double oreDropRate = 0.5;
    public static double hungerrate = 0.15;
    public static double maxhealthExtraPoints = -2;
    public static double walkspeed = -0.1;
    
    public static List<CharacterClass> CharacterClasses = new()
    {
        new CharacterClass()
        {
            Code = "miner",
            Gear = new JsonItemStack[]
            {
                new()
                {
                    Type = EnumItemClass.Item,
                    Code = new AssetLocation("game", "clothes-face-miner"),
                },
                new()
                {
                    Type = EnumItemClass.Item,
                    Code = new AssetLocation("game", "clothes-hand-miner"),
                },
                new()
                {
                    Type = EnumItemClass.Item,
                    Code = new AssetLocation("game", "clothes-upperbody-miner"),
                },
                new()
                {
                    Type = EnumItemClass.Item,
                    Code = new AssetLocation("game", "clothes-lowerbody-miner"),
                },
                new()
                {
                    Type = EnumItemClass.Item,
                    Code = new AssetLocation("game", "clothes-foot-miner"),
                },
                new()
                {
                    Type = EnumItemClass.Item,
                    Code = new AssetLocation("game", "clothes-upperbodyover-miner"),
                },
                new()
                {
                    Type = EnumItemClass.Item,
                    Code = new AssetLocation("game", "clothes-waist-miner"),
                },
                new()
                {
                    Type = EnumItemClass.Item,
                    Code = new AssetLocation("game", "clothes-waist-miner"),
                },
                new()
                {
                    Type = EnumItemClass.Item,
                    Code = new AssetLocation("minerz", "armor-head-miner"),
                }
            },
            Traits = new []
            {
                "miner", "coalLung", "lovesUnderground"
            }
        },
    };

    public static List<Trait> traits = new()
    {
        new Trait()
        {
            Code = "miner",
            Attributes = new Dictionary<string, double>()
            {
                {"miningSpeedMul", miningSpeedMul},
                {"oreDropRate", oreDropRate},
                {"hungerrate", hungerrate},
            },
            Type = EnumTraitType.Mixed
        },
        new Trait()
        {
            Code = "lovesUnderground",  
            Attributes = new Dictionary<string, double>(),
            Type = EnumTraitType.Positive
        },
        new Trait()
        {
            Code = "coalLung",
            Attributes = new Dictionary<string, double>()
            {
                {"maxhealthExtraPoints", maxhealthExtraPoints},
                {"walkspeed", walkspeed},
            },
            Type = EnumTraitType.Negative
        },
    };
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof (CharacterSystem), "loadCharacterClasses")]
    public static void ShowCharacterClasses(CharacterSystem __instance)
    {
        foreach (var trait in traits.Where(trait => !__instance.traits.Contains(trait)))
        {
            __instance.traits.Add(trait);
            __instance.TraitsByCode[trait.Code] = trait;
        }
        
        foreach (var characterClass in CharacterClasses.Where(characterClass => !__instance.characterClasses.Contains(characterClass)))
        {
            __instance.characterClasses.Add(characterClass);
            __instance.characterClassesByCode[characterClass.Code] = characterClass;
        }
    }
}