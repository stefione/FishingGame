using PixPlays.Fishing.Configuration;
using PixPlays.Fishing.Fish;
using PixPlays.Fishing.Player;
using PixPlays.Fishing.World;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseGameManager : MonoBehaviour
{
    public Dictionary<ulong, PlayerData> PlayerDatas = new();
    public GameConfiguration GameConfig;
}
