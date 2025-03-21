using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Minerz;

/// <summary>
/// Thanks to https://github.com/NathanNgo/BetterProspecting/
/// </summary>
public class MinerzReinforcedProspectingPick : ItemProspectingPick
{
    private SkillItem[]? toolModes;
    private int radius = 20; 
    private int yLength = 20;

    /// <summary>
    /// Getter
    /// </summary>
    private SkillItem[] ToolModes
    {
        get => toolModes ??= LoadNewToolModes(api);
        set => toolModes = value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="coreAPI"></param>
    public override void OnLoaded(ICoreAPI coreAPI)
    {
        ToolModes = LoadNewToolModes(coreAPI);
        
        base.OnLoaded(coreAPI);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="forPlayer"></param>
    /// <param name="blockSel"></param>
    /// <returns></returns>
    public override SkillItem[] GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
    {
        return ToolModes;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="byPlayer"></param>
    /// <param name="blockSel"></param>
    /// <returns></returns>
    public override int GetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSel)
    {
        return Math.Min(ToolModes.Length - 1, slot.Itemstack.Attributes.GetInt("toolMode"));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="blockSel"></param>
    /// <param name="itemslot"></param>
    /// <param name="remainingResistance"></param>
    /// <param name="dt"></param>
    /// <param name="counter"></param>
    /// <returns></returns>
    public override float OnBlockBreaking(IPlayer player, BlockSelection blockSel, ItemSlot itemslot, float remainingResistance, float dt, int counter)
    {
        var remain = base.OnBlockBreaking(player, blockSel, itemslot, remainingResistance, dt, counter);
        var toolMode = GetToolMode(itemslot, player, blockSel);

        remain = (remain + remainingResistance) / 2.2f;
        return remain;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="world"></param>
    /// <param name="byEntity"></param>
    /// <param name="itemslot"></param>
    /// <param name="blockSel"></param>
    /// <param name="dropQuantityMultiplier"></param>
    /// <returns></returns>
    public override bool OnBlockBrokenWith(IWorldAccessor world, Vintagestory.API.Common.Entities.Entity byEntity, ItemSlot itemslot, BlockSelection blockSel, float dropQuantityMultiplier = 1)
    {
        var byPlayer = (byEntity as EntityPlayer)?.Player;
        if (byPlayer != null)
        {
            var toolMode = GetToolMode(itemslot, byPlayer, blockSel);
            var damage = 4;
            var initialToolsPresent = 2;
            
            if (toolMode == 0)
            {
                damage = 1;
                ProbeBlockDensityMode(world, byEntity, itemslot, blockSel);
            }
            else if (toolMode == 1)
            {
                damage = 1;
                ProbeBlockNodeMode(world, byEntity, itemslot, blockSel, radius);
            }
            //else if (toolMode == initialToolsPresent + 0)
            //{
            //    ProbeLineMode(world, byEntity, blockSel);
            //    damage = 4;
            //}
            else if (toolMode == initialToolsPresent + 0)
            {
                ProbeDistanceSampleMode(world, byEntity, blockSel, radius / 2);
                damage = 4;
            }
            else if (toolMode == initialToolsPresent + 1)
            {
                ProbeAreaSampleMode(world, byEntity, blockSel, radius * 2);
                damage = 2;
            }
            else if (toolMode == initialToolsPresent + 2)
            {
                ProbeAreaSampleMode(world, byEntity, blockSel, radius * 3);
                damage = 2;
            }
            /// only do extra damage if not using the game's built in <see cref="ItemProspectingPick.ProbeBlockDensityMode"/>
            if (DamagedBy != null && DamagedBy.Contains(EnumItemDamageSource.BlockBreaking))
            {
                DamageItem(world, byEntity, itemslot, damage);
            }
        }

        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="world"></param>
    /// <param name="byEntity"></param>
    /// <param name="blockSel"></param>
    //protected virtual void ProbeLineMode(IWorldAccessor world, Vintagestory.API.Common.Entities.Entity byEntity, BlockSelection blockSel)
    //{
    //    IPlayer? byPlayer = null;
    //    if (byEntity is EntityPlayer player)
    //    {
    //        byPlayer = world.PlayerByUid(player.PlayerUID);
    //    }

    //    var block = world.BlockAccessor.GetBlock(blockSel.Position);
    //    block.OnBlockBroken(world, blockSel.Position, byPlayer, 0);

    //    if (!IsPropickable(block))
    //    {
    //        return;
    //    }

    //    if (byPlayer is not IServerPlayer serverPlayer)
    //    {
    //        return;
    //    }

    //    serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, $"Area sample taken for a length of x:{radius / 4} y:{yLength / 2} z:{radius / 4}"), EnumChatType.Notification);

    //    bool toBottom = false;
    //    
    //    bool toX = false;
    //    bool toZ = false;
    //    bool toMX = false;
    //    bool toMZ = false;
    //    
    //    //TODO fix this mess here ....
    //    if (serverPlayer.Entity.Pos.Yaw is < 0.5f or > 5.5f)
    //    {
    //        toX = true;
    //    }
    //    else if (serverPlayer.Entity.Pos.Yaw > 4.3)
    //    {
    //        toZ = true;
    //        
    //        toX = false;
    //    }
    //    else if (serverPlayer.Entity.Pos.Yaw > 3.3)
    //    {
    //        toMX = true;
    //        
    //        toX = false;
    //        toZ = false;
    //    }
    //    else if (serverPlayer.Entity.Pos.Yaw <= 3.3)
    //    {
    //        toMZ = true;
    //        
    //        toX = false;
    //        toZ = false;
    //        toMX = false;
    //    }
    //    if (serverPlayer.Entity.Pos.Pitch > 3)
    //    {
    //        toBottom = true;
    //        
    //        toX = false;
    //        toZ = false;
    //        toMX = false;
    //        toMZ = false;
    //    }
    //    
    //    serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, $"{serverPlayer.Entity.Pos.Yaw}", EnumChatType.Notification);
    //    serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, $"{serverPlayer.Entity.Pos.Pitch}", EnumChatType.Notification);
    //    
    //    var quantityFound = new Dictionary<string, int>();
    //    var blockPos = blockSel.Position.Copy();
    //    api.World.BlockAccessor.WalkBlocks(
    //        blockPos.AddCopy(toX ? radius * 100 : radius / 4, toBottom ? radius : radius * 100 / 4, toZ ? radius * 100 : radius / 4), 
    //        blockPos.AddCopy(toMX ? -radius * 100 : -radius / 4, toBottom ? -radius * 100 : -radius / 4, toMZ ? -radius * 100 : -radius / 4),  
    //        delegate (Block nblock, int x, int y, int z)
    //        {
    //            if (nblock.BlockMaterial == EnumBlockMaterial.Ore && nblock.Variant.TryGetValue("type", out var ore))
    //            {
    //                var key = "ore-" + ore;
    //                quantityFound.TryGetValue(key, out var value);
    //                quantityFound[key] = value + 1;
    //            }
    //        });
    //    
    //    var list = quantityFound.OrderByDescending(val => val.Value).ToList();
    //    if (list.Count == 0)
    //    {
    //        serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, "No ore node nearby"), EnumChatType.Notification);
    //        return;
    //    }

    //    serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, "Found the following ore nodes"), EnumChatType.Notification);
    //    foreach (var item in list)
    //    {
    //        var l = Lang.GetL(serverPlayer.LanguageCode, item.Key);
    //        var l2 = Lang.GetL(serverPlayer.LanguageCode, resultTextByQuantity(item.Value), Lang.Get(item.Key));
    //        serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, l2, l), EnumChatType.Notification);
    //    }
    //}

    /// <summary>
    /// 
    /// </summary>
    /// <param name="world"></param>
    /// <param name="byEntity"></param>
    /// <param name="blockSel"></param>
    /// <param name="xzlength"></param>
    protected virtual void ProbeAreaSampleMode(IWorldAccessor world, Vintagestory.API.Common.Entities.Entity byEntity, BlockSelection blockSel, int xzlength)
    {
        IPlayer? byPlayer = null;
        if (byEntity is EntityPlayer player) byPlayer = world.PlayerByUid(player.PlayerUID);

        var block = world.BlockAccessor.GetBlock(blockSel.Position);
        block.OnBlockBroken(world, blockSel.Position, byPlayer, 0);

        if (!IsPropickable(block))
        {
            return;
        }

        if (byPlayer is not IServerPlayer serverPlayer)
        {
            return;
        }

        serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, $"Area sample taken for a length of {xzlength}:"), EnumChatType.Notification);

        var quantityFound = new Dictionary<string, int>();
        var blockPos = blockSel.Position.Copy();
        api.World.BlockAccessor.WalkBlocks(blockPos.AddCopy(xzlength, yLength, xzlength), blockPos.AddCopy(-xzlength, -yLength, -xzlength), 
            delegate (Block nblock, int x, int y, int z)
            {
                if (nblock.BlockMaterial == EnumBlockMaterial.Ore && nblock.Variant.TryGetValue("type", out var ore))
                {
                    var key = "ore-" + ore;
                    quantityFound.TryGetValue(key, out var value);
                    quantityFound[key] = value + 1;
                }
            });
        
        var list = quantityFound.OrderByDescending(val => val.Value).ToList();
        if (list.Count == 0)
        {
            serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, "No ore node nearby"), EnumChatType.Notification);
            return;
        }

        serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, "Found the following ore nodes"), EnumChatType.Notification);
        foreach (var item in list)
        {
            var l = Lang.GetL(serverPlayer.LanguageCode, item.Key);
            var l2 = Lang.GetL(serverPlayer.LanguageCode, resultTextByQuantity(item.Value), Lang.Get(item.Key));
            serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, l2, l), EnumChatType.Notification);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="world"></param>
    /// <param name="byEntity"></param>
    /// <param name="blockSel"></param>
    /// <param name="xzlength"></param>
    protected virtual void ProbeDistanceSampleMode(IWorldAccessor world, Vintagestory.API.Common.Entities.Entity byEntity, BlockSelection blockSel, int xzlength)
    {
        IPlayer? byPlayer = null;
        if (byEntity is EntityPlayer player)
        {
            byPlayer = world.PlayerByUid(player.PlayerUID);
        }

        var block = world.BlockAccessor.GetBlock(blockSel.Position);
        block.OnBlockBroken(world, blockSel.Position, byPlayer, 0);

        if (!IsPropickable(block))
        {
            return;
        }

        if (byPlayer is not IServerPlayer serverPlayer)
        {
            return;
        }

        serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, $"Area sample taken for a length of {xzlength}:"), EnumChatType.Notification);

        var firstOreDistance = new Dictionary<string, int>();

        var blockPos = blockSel.Position.Copy();
        api.World.BlockAccessor.WalkBlocks(
            blockPos.AddCopy(xzlength, yLength, xzlength), 
            blockPos.AddCopy(-xzlength, -yLength, -xzlength), 
            delegate (Block nblock, int x, int y, int z)
        {
            if (nblock.BlockMaterial == EnumBlockMaterial.Ore && nblock.Variant.TryGetValue("type", out var ore))
            {
                var key = "ore-" + ore;
                var distance = (int) blockSel.Position.DistanceTo(new BlockPos(x, y, z));
                if (!firstOreDistance.TryGetValue(key, out var value) || distance < value)
                {
                    value = distance;
                    firstOreDistance[key] = value;
                }
            }
        });
        
        var message = Lang.Get("No ore node nearby");
        var list = firstOreDistance.ToList();
        if (list.Count == 0)
        {
            serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, message), EnumChatType.Notification);
            return;
        }

        message = Lang.Get("Found the following ore nodes");
        serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, Lang.GetL(serverPlayer.LanguageCode, message), EnumChatType.Notification);
        message = "";
        foreach (var item in list)
        {
            var l = Lang.GetL(serverPlayer.LanguageCode, item.Key);
            message += Lang.GetL(serverPlayer.LanguageCode, $"{l.ToUpper()}: {item.Value} block(s) away");
        }
        serverPlayer.SendMessage(GlobalConstants.InfoLogChatGroup, message, EnumChatType.Notification);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
    private bool IsPropickable(Block? block)
    {
        return block?.Attributes?["propickable"].AsBool() == true;
    }
    
    private SkillItem[] LoadNewToolModes(ICoreAPI coreAPI)
    {
        return ObjectCacheUtil.GetOrCreate(coreAPI, "reinforcedPickToolModes", () =>
        {
            SkillItem[] modes = {
                new() { Code = new AssetLocation("density"), Name = Lang.Get("Density Search Mode (Long range, chance based search)") },
                new() { Code = new AssetLocation("node"), Name = Lang.Get("Node Search Mode (Short range, exact search)") },
                //new() { Code = new AssetLocation("line"), Name = Lang.Get("Line Mode (Long range, one direction)") },
                new() { Code = new AssetLocation("distance_short"), Name = Lang.Get("Short Distance Mode (Short range, distance search)") },
                new() { Code = new AssetLocation("area1"), Name = Lang.Get("Area Sample Mode (Searches in a small area)") },
                new() { Code = new AssetLocation("area2"), Name = Lang.Get("Area Sample Mode (Searches in a high area)") },
            };

            if (api is ICoreClientAPI clientAPI)
            {
                modes[0].WithIcon(clientAPI, clientAPI.Gui.LoadSvgWithPadding(new AssetLocation("textures/icons/heatmap.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                modes[0].TexturePremultipliedAlpha = false;
                modes[1].WithIcon(clientAPI, clientAPI.Gui.LoadSvgWithPadding(new AssetLocation("textures/icons/rocks.svg"),48, 48, 5, ColorUtil.WhiteArgb));
                modes[1].TexturePremultipliedAlpha = false;
                //modes[2].WithIcon(clientAPI, clientAPI.Gui.LoadSvgWithPadding(new AssetLocation("minerz", "textures/icons/line.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                //modes[2].TexturePremultipliedAlpha = false;
                modes[2].WithIcon(clientAPI, clientAPI.Gui.LoadSvgWithPadding(new AssetLocation("minerz", "textures/icons/short.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                modes[2].TexturePremultipliedAlpha = false;
                modes[3].WithIcon(clientAPI, clientAPI.Gui.LoadSvgWithPadding(new AssetLocation("minerz", "textures/icons/small.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                modes[3].TexturePremultipliedAlpha = false;
                modes[4].WithIcon(clientAPI, clientAPI.Gui.LoadSvgWithPadding(new AssetLocation("minerz", "textures/icons/med.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                modes[4].TexturePremultipliedAlpha = false;
            }

            return modes;
        });
    }
}