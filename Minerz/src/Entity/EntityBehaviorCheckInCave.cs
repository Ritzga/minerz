#nullable enable
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Minerz.Entity;

public class EntityBehaviorCheckInCave : EntityBehavior
{
    public override string PropertyName() => "caveHandler";
    
    private ICoreAPI? api;
    private RoomRegistry? roomRegistry;
    
    private bool inCave;
    private BlockPos tmpPos;
    
    private float veryslowaccum;
    private bool hasTrait;
    private bool firstRun = true;
    private uint tries;
    private const uint maxTries = 15;
    

    public EntityBehaviorCheckInCave(Vintagestory.API.Common.Entities.Entity entity)
        : base(entity)
    {
    }
    
    public override void Initialize(EntityProperties properties, JsonObject typeAttributes)
    {
        base.Initialize(properties, typeAttributes);
        api = entity?.World?.Api;
        roomRegistry = api?.ModLoader?.GetModSystem<RoomRegistry>();
        tmpPos = new BlockPos(entity?.Pos?.Dimension ?? 0);
    }
    
    public override void OnGameTick(float deltaTime)
    {
        veryslowaccum += deltaTime;
        
        //approximately 20s 
        if (veryslowaccum > 20)
        {
            //because maybe null exceptions
            try
            {
                //check for class exists
                if (firstRun)
                {
#if DEBUG
                    api?.Logger.Notification("==================================== check characterClass with " + tries + " tries ====================================");
#endif
                    var key = entity.WatchedAttributes.GetString("characterClass");
                    //because if you join on a multiplayer for the first time you are a commoner and switch after the character creation to a miner (300s to try)
                    if (key != null && (key == "miner" || (key != "miner" && tries > maxTries)))
                    {
                        firstRun = false;
                        //check for trait
                        hasTrait = api?.ModLoader?.GetModSystem<CharacterSystem>()
                            ?.HasTrait((entity as EntityPlayer)?.Player, "lovesUnderground") ?? false;
                        if (!hasTrait)
                        {
#if DEBUG
                            api?.Logger.Notification("==================================== Character has not the miner class ====================================");
#endif
                            entity.RemoveBehavior(this);
                        }
                    }
                    tries++;
                }

                if (hasTrait)
                {
                    tmpPos.Set((int)(entity?.Pos?.X ?? 0), (int)(entity?.Pos?.Y ?? 0), (int)(entity?.Pos?.Z ?? 0));
                    var roomForPosition = roomRegistry?.GetRoomForPosition(tmpPos);
                    var checker = roomForPosition?.ExitCount == 0 || roomForPosition?.SkylightCount < roomForPosition?.NonSkylightCount;

                    CheckCave(checker);
                }
            }
            catch (Exception e)
            {
#if DEBUG
                api?.Logger.Notification("==================================== Error: " + e.Message + " ====================================");
                api?.Logger.Notification(e.StackTrace);
#endif
            }
            veryslowaccum = 0;
        }
    }
    
    private void CheckCave(bool newState)
    {
#if DEBUG
        api?.Logger.Notification("==================================== check under earth ====================================");
#endif
        if (inCave != newState)
        {
            if (!inCave && newState)
            {
                OnEnterCave();
            }
            else
            {
                OnLeaveCave();
            }
            inCave = newState;
        }
    }
    
    private void OnEnterCave()
    {
        entity.Stats.Set("hungerrate", "minerstat", -0.15f);
#if DEBUG
        api?.Logger.Notification("==================================== under earth ====================================");
#endif
    }

    private void OnLeaveCave()
    {
        entity.Stats.Set("hungerrate", "minerstat", 0f);
#if DEBUG
        api?.Logger.Notification("==================================== over earth ====================================");
#endif
    }
}