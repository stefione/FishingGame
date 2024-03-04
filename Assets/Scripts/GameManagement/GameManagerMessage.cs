using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.Fishing.GameManagement
{
    public enum GameManagerMessage
    {
        SpawnEntitiesMessage,


        RegisterPlayerMessage,
        PlayerRegisteredMessage,

        TryCatchFishMessage,
        CatchFishResultMessage,

        ThrowHookMessage,
        ThrowHookAllMessage,
        LiftHookMessage,
    }
}