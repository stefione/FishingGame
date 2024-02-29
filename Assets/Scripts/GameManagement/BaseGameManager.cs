using PixPlays.Fishing.Configuration;
using PixPlays.Fishing.Entities;
using PixPlays.Fishing.Player;
using PixPlays.Fishing.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGameManager : MonoBehaviour
{
    public GameSceneData GameSceneData;
    public GameConfiguration GameConfig;
    public WaterArea WaterArea;

    protected List<FishController> fishes = new();
    public Dictionary<string, PlayerData> PlayerDatas = new();
}
