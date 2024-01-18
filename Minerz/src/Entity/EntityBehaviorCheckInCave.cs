#nullable enable
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Minerz.Entity;

public class EntityBehaviorCheckInCave : EntityBehavior
{
    public override string PropertyName() => "caveHandler";
    
    private ICoreAPI api;
    
    private bool inCave;
    private BlockPos plrpos = new();
    
    private float veryslowaccum;
    private bool hasTrait;
    private bool firstRun = true;

    public EntityBehaviorCheckInCave(Vintagestory.API.Common.Entities.Entity entity)
        : base(entity)
    {
    }
    
    public override void Initialize(EntityProperties properties, JsonObject typeAttributes)
    {
        base.Initialize(properties, typeAttributes);
        api = entity.World.Api;
        
    }
    
    public override void OnGameTick(float deltaTime)
    {
        veryslowaccum += deltaTime;

        if (veryslowaccum > 20)
        {
            //check for class exists
            if (firstRun)
            {
                var key = entity.WatchedAttributes.GetString("characterClass");
                if (key != null)
                {
                    firstRun = false;
                    //check for trait
                    hasTrait = api.ModLoader?.GetModSystem<CharacterSystem>()?.HasTrait((entity as EntityPlayer)?.Player, "lovesUnderground") ?? false;
                    if (!hasTrait)
                    {
                        entity.RemoveBehavior(this);
                    }
                }
            }
            
            if (hasTrait)
            {
                plrpos.Set((int) (entity?.Pos?.X ?? 0), (int) (entity?.Pos?.Y), (int) (entity?.Pos?.Z));
                var roomForPosition = api?.ModLoader?.GetModSystem<RoomRegistry>()?.GetRoomForPosition(plrpos);
                var checker = roomForPosition?.ExitCount == 0 || roomForPosition?.SkylightCount < roomForPosition?.NonSkylightCount;
        
                 CheckCave(checker);
            }
            veryslowaccum = 0;
        }
    }
    
    private void CheckCave(bool newState)
    {
#if DEBUG
        api.Logger.Notification("==================================== check under earth ====================================");
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
        api.Logger.Notification("==================================== under earth ====================================");
#endif
    }

    private void OnLeaveCave()
    {
        entity.Stats.Set("hungerrate", "minerstat", 0f);
#if DEBUG
        api.Logger.Notification("==================================== over earth ====================================");
#endif
    }
}