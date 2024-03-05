using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.Fishing.GameManagement
{
    public enum GameManagerMessageType
    {
        SpawnFishMessage,
        PlayersMessage,

        RegisterPlayerMessage,
        PlayerRegisteredMessage,

        CatchFishResultMessage,

        ThrowHookRequestMessage,
        ThrowHookAllMessage,
        LiftHookAllMessage,

        UpdateFishPositionsMessage,

        ClientDisconnectedMessage,
    }
}