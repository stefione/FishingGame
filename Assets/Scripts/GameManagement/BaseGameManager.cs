using PixPlays.Fishing.Configuration;
using PixPlays.Fishing.Entities;
using PixPlays.Fishing.Player;
using PixPlays.Fishing.World;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseGameManager : NetworkBehaviour
{
    protected List<FishController> fishes = new();
    public GameConfiguration GameConfig;


    public Dictionary<ulong, PlayerData> PlayerDatas = new();
    protected Dictionary<ulong, ClientPlayerController> PlayerControllers = new();
}
