using PixPlays.Fishing.Configuration;
using PixPlays.Fishing.Entities;
using PixPlays.Fishing.Hook;
using PixPlays.Fishing.Player;
using PixPlays.Fishing.RandomGenerator;
using PixPlays.Fishing.World;
using PixPlays.Framework.Events;
using PixPlays.Framework.Network;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace PixPlays.Fishing.GameManagement
{
    public class ClientGameManager : BaseGameManager
    {
        public List<Transform> _SpawnPoints;

        private Dictionary<string, ClientPlayerController> PlayerControllers = new();

        private void Awake()
        {
            Application.targetFrameRate = 1000;
            EventManager.Subscribe<TryCatchFishEvent>(x => ProcessTryCatchFishEvent(x));
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                GameManagerMessage.CatchFishResultMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                {
                    reader.ReadValueSafe(out string playerId);
                    reader.ReadValueSafe(out bool attemptResult);
                    reader.ReadValueSafe(out ulong fishId);
                    ProcessCatchFishResultMessage(playerId, attemptResult, fishId);
                });
        }

        private void Start()
        {
            SetupPlayers();
            SpawnFishes();
        }



        public void SetupPlayers()
        {
            for (int i = 0; i < GameSceneData.Players.Count; i++)
            {
                PlayerDatas.Add(GameSceneData.Players[i].Id, GameSceneData.Players[i]);
                ClientPlayerController playerController = Instantiate(GameConfig.PlayerTemplate, _SpawnPoints[i]);
                playerController.SetData(GameSceneData.Players[i], WaterArea);
                PlayerControllers.Add(GameSceneData.Players[i].Id, playerController);
            }
        }

        private void ProcessTryCatchFishEvent(TryCatchFishEvent x)
        {
            using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
            {
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    GameManagerMessage.TryCatchFishMessage.ToString(),
                    NetworkManager.ServerClientId,
                    writer,
                    NetworkDelivery.ReliableFragmentedSequenced);
            };
        }

        private void ProcessCatchFishResultMessage(string playerId, bool attemptResult, ulong fishId)
        {
            if (PlayerDatas.TryGetValue(playerId, out var player))
            {
                if (attemptResult)
                {
                    player.Attempts.Add(true);
                    FishController fish = fishes.Find(x => x.NetworkId == fishId);
                    fish.BiteHook(PlayerControllers[playerId]);
                    if (player.FishCought.ContainsKey(fish.FishData.Id))
                    {
                        player.FishCought[fish.FishData.Id]++;
                    }
                    else
                    {
                        player.FishCought.Add(fish.FishData.Id, 1);
                    }
                }
                else
                {
                    player.Attempts.Add(false);
                    StartCoroutine(Coroutine_WaitToLift(PlayerControllers[playerId]));
                }
            }
        }

        IEnumerator Coroutine_WaitToLift(ClientPlayerController playerController)
        {
            float time = Random.Range(1f, 2f);
            yield return new WaitForSeconds(time);
            playerController.LiftHook(null);
        }

        private void SpawnFishes()
        {
            Dictionary<string, Vector2> spawnChances = new();
            float maxSpawnValue = 0;
            foreach (var i in GameConfig.FishDatas)
            {
                float endValue = maxSpawnValue + i.Value.SpawnChance;
                spawnChances.Add(i.Key, new Vector2(maxSpawnValue, endValue));
                maxSpawnValue = endValue;
            }
            for (int i = 0; i < GameConfig.NumberOfFish; i++)
            {
                float spawnValue = Random.Range(0, maxSpawnValue);
                string spawnKey = "";
                foreach (var spawnItem in spawnChances)
                {
                    if (spawnItem.Value.x < spawnValue && spawnItem.Value.y >= spawnValue)
                    {
                        spawnKey = spawnItem.Key;
                        break;
                    }
                }
                if (GameConfig.FishDatas.TryGetValue(spawnKey, out var fishData))
                {
                    float x = Random.Range(WaterArea.BottomLeft.position.x, WaterArea.TopRight.position.x);
                    float y = Random.Range(WaterArea.BottomLeft.position.y, WaterArea.TopRight.position.y);
                    if (GameConfig.FishTemplates.TryGetValue(fishData.Id, out var template))
                    {
                        FishController fish = Instantiate(template);
                        fish.SetData(fishData, WaterArea, new Vector3(x, y, 0));
                        fishes.Add(fish);
                    }
                    else
                    {
                        Debug.LogError("Missing fish template for id:" + fishData.Id);
                    }
                }
            }
        }
    }
}