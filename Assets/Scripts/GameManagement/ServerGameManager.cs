using PixPlays.Fishing.Entities;
using PixPlays.Fishing.Player;
using PixPlays.Fishing.RandomGenerator;
using PixPlays.Fishing.World;
using PixPlays.Framework.Events;
using PixPlays.Framework.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;
namespace PixPlays.Fishing.GameManagement
{

    public struct FishNetworkData : INetworkSerializable
    {
        public ulong[] NetworkIds;
        public List<string> FishIds;
        public Vector3[] Positions;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref NetworkIds);
            FishIds = NetworkController.SerializeStringList<T>(FishIds, serializer);
            serializer.SerializeValue(ref Positions);
        }
    }

    public class ServerGameManager : BaseGameManager
    {
        public ServerFishController ServerFishControllerTemplate;
        private int _spawnPointIndex;

        private void Start()
        {
            EventManager.Subscribe<TryCatchFishEvent>(x => ProcessTryCatchFishSignal(x));
            EventManager.Subscribe<LiftHookEvent>(x => ProcessLiftHookEvent(x));
            NetworkManager.OnServerStarted += NetworkManager_OnServerStarted;
            NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
            NetworkManager.Singleton.StartServer();
        }

        private void ProcessLiftHookEvent(LiftHookEvent x)
        {
            x.Owner.LiftHook(x.Fish);
            using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
            {
                writer.WriteValueSafe(x.Owner.OwnerClientId);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    GameManagerMessage.LiftHookMessage.ToString(),
                    NetworkManager.ConnectedClientsIds,
                    writer,
                    NetworkDelivery.ReliableFragmentedSequenced);
            }
        }

        private void NetworkManager_OnServerStarted()
        {
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                GameManagerMessage.RegisterPlayerMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                {
                    reader.ReadNetworkSerializable(out PlayerDataStruct data);
                    PlayerData playerData = new PlayerData(data);
                    ProcessRegisterPlayerMessage(sender, playerData);
                });
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                GameManagerMessage.ThrowHookMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                {
                    Debug.Log("Receive hook throw");
                    ProcessThrowHookMessage(sender);
                });
            SpawnFishes();
        }
        private void NetworkManager_OnClientConnectedCallback(ulong obj)
        {
            Debug.Log("Client connected to server");
            SendFishData();
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
                    float x = Random.Range(WorldData.Instance.WaterArea.BottomLeft.position.x, WorldData.Instance.WaterArea.TopRight.position.x);
                    float y = Random.Range(WorldData.Instance.WaterArea.BottomLeft.position.y, WorldData.Instance.WaterArea.TopRight.position.y);
                    if (GameConfig.FishTemplates.TryGetValue(fishData.Id, out var template))
                    {
                        FishController fish = Instantiate(template);
                        fish.SetData(fishData, WorldData.Instance.WaterArea, new Vector3(x, y, 0));
                        fish.GetComponent<NetworkObject>().Spawn();
                        fishes.Add(fish);
                    }
                    else
                    {
                        Debug.LogError("Missing fish template for id:" + fishData.Id);
                    }
                }
            }

        }
        private void SendFishData()
        {
            using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
            {
                var fishNetwork = new FishNetworkData()
                {
                    NetworkIds = new ulong[fishes.Count],
                    Positions = new Vector3[fishes.Count],
                    FishIds=new()
                };
                for (int i = 0; i < fishes.Count; i++)
                {
                    fishNetwork.NetworkIds[i] = fishes[i].NetworkObjectId;
                    fishNetwork.FishIds.Add(fishes[i].FishData.Id);
                    fishNetwork.Positions[i] = fishes[i].GetPosition();
                }
                writer.WriteNetworkSerializable(in fishNetwork);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    GameManagerMessage.SpawnEntitiesMessage.ToString(),
                    NetworkManager.ConnectedClientsIds,
                    writer,
                    NetworkDelivery.ReliableFragmentedSequenced);
            };
        }

        private void ProcessThrowHookMessage(ulong sender)
        {
            PlayerControllers[sender].ThrowHook();
            using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
            {
                writer.WriteValueSafe(sender);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    GameManagerMessage.ThrowHookAllMessage.ToString(),
                    NetworkManager.ConnectedClientsIds,
                    writer,
                    NetworkDelivery.ReliableFragmentedSequenced);
            };
        }
        private void ProcessRegisterPlayerMessage(ulong clientId,PlayerData playerData)
        {
            PlayerDatas.Add(clientId, playerData);
            ClientPlayerController playerController = Instantiate(GameConfig.PlayerTemplate, WorldData.Instance.SpawnPoints[_spawnPointIndex]);
            playerController.SetData(clientId,playerData, WorldData.Instance.WaterArea);
            playerController.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            PlayerControllers.Add(clientId, playerController);
            using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
            {
                var data = new PlayerDataStruct(playerData);
                writer.WriteValueSafe(clientId);
                writer.WriteValueSafe(_spawnPointIndex);
                writer.WriteNetworkSerializable(in data);

                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    GameManagerMessage.PlayerRegisteredMessage.ToString(),
                    NetworkManager.ConnectedClientsIds,
                    writer,
                    NetworkDelivery.ReliableFragmentedSequenced);
            };
            _spawnPointIndex++;
        }
        private void ProcessTryCatchFishSignal(TryCatchFishEvent x)
        {
            if (PlayerDatas.TryGetValue(x.PlayerID, out var player))
            {
                bool attemptResult =false;
                string fishId = "";
                if (RNG.RTP_WillCatch(player.Attempts, GameConfig.SampleAttempts, GameConfig.SuccessChance))
                {
                    attemptResult = true;
                    if (player.Attempts == null)
                    {
                        player.Attempts = new();
                    }
                    player.Attempts.Add(true);
                    
                    FishController fish = CatchRandomFish();
                    fish.BiteHook(PlayerControllers[x.PlayerID]);
                    fishId = fish.FishData.Id;               
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
                }
                using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
                {
                    writer.WriteValueSafe(x.PlayerID);
                    writer.WriteValueSafe(attemptResult);
                    writer.WriteValueSafe(fishId);
                    NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                        GameManagerMessage.CatchFishResultMessage.ToString(),
                        NetworkManager.ConnectedClientsIds,
                        writer,
                        NetworkDelivery.ReliableFragmentedSequenced);
                };
            }
        }
        private FishController CatchRandomFish()
        {
            Dictionary<string, float> items = new();
            foreach (var i in fishes)
            {
                if (!i.IsOwned)
                {
                    items.TryAdd(i.FishData.Id, i.FishData.CatchChance);
                }
            }
            string fishKey = RNG.RNG_ItemCought(items);
            return fishes.Find(x => x.FishData.Id == fishKey);
        }
    }
}
