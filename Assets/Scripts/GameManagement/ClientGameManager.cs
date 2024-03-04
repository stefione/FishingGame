using PixPlays.Fishing.Configuration;
using PixPlays.Fishing.Entities;
using PixPlays.Fishing.Hook;
using PixPlays.Fishing.Player;
using PixPlays.Fishing.RandomGenerator;
using PixPlays.Fishing.World;
using PixPlays.Framework.Events;
using PixPlays.Framework.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor.MPE;
using UnityEngine;
namespace PixPlays.Fishing.GameManagement
{


    public class ClientGameManager : BaseGameManager
    {
        protected Dictionary<ulong, FishController> spawnedFish=new();
        public GameSceneData GameSceneData;

        private FishNetworkData? fishNetworkData;

        private void Start()
        {
            Application.targetFrameRate = 1000;

            EventManager.Subscribe<PlayerControllerSpawnedEvent>(x => ProcessPlayerControllerSpawnedEvent(x));
            EventManager.Subscribe<ThrowHookEvent>(x => ProcessThrowHookEvent(x));
            EventManager.Subscribe<FishSpawnEvent>(x => ProcessFishSpawnEvent(x));
            NetworkManager.OnClientStarted += NetworkManager_OnClientStarted;
            NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
            NetworkManager.Singleton.StartClient();
        }

        private void ProcessFishSpawnEvent(FishSpawnEvent x)
        {
            spawnedFish.Add(x.Fish.NetworkObjectId,x.Fish);
            if (fishNetworkData != null)
            {
                int index = fishNetworkData.Value.NetworkIds.ToList().FindIndex(y => y == x.Fish.NetworkObjectId);
                if (GameConfig.FishDatas.TryGetValue(fishNetworkData.Value.FishIds[index], out var data)) 
                {
                    x.Fish.SetData(data, WorldData.Instance.WaterArea, fishNetworkData.Value.Positions[index]);
                }
            }
        }

        private void NetworkManager_OnClientStarted()
        {
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                 GameManagerMessage.PlayerRegisteredMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                 {
                     reader.ReadValueSafe(out ulong clientId);
                     reader.ReadValueSafe(out int spawnIndex);
                     reader.ReadNetworkSerializable(out PlayerDataStruct data);
                     ProcessPlayerRegisteredMessage(clientId,spawnIndex,new PlayerData(data));
                 });
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                GameManagerMessage.CatchFishResultMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                {
                    reader.ReadValueSafe(out ulong clientId);
                    reader.ReadValueSafe(out bool attemptResult);
                    reader.ReadValueSafe(out string fishId);
                    ProcessCatchFishResultMessage(clientId, attemptResult, fishId);
                });
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                GameManagerMessage.LiftHookMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                {
                    reader.ReadValueSafe(out ulong clientId);
                    ProcessLiftHookMessage(clientId);
                });
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                 GameManagerMessage.ThrowHookAllMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                 {
                     reader.ReadValueSafe(out ulong clientId);
                     ProcessThrowAllHookMessage(clientId);
                 });
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
              GameManagerMessage.SpawnEntitiesMessage.ToString(), (ulong sender, FastBufferReader reader) =>
              {
                  reader.ReadValueSafe(out FishNetworkData fishData);
                  ProcessSpawnEntitiesEvent(fishData);
              });
        }

        private void ProcessSpawnEntitiesEvent(FishNetworkData fishData)
        {
            fishNetworkData = fishData;
            int count = 0;
            if (spawnedFish != null)
            {
                foreach (var i in fishData.NetworkIds)
                {
                    if (spawnedFish.TryGetValue(i, out var fish))
                    {
                        if (GameConfig.FishDatas.TryGetValue(fishNetworkData.Value.FishIds[count], out var data))
                        {
                            fish.SetData(data, WorldData.Instance.WaterArea, fishNetworkData.Value.Positions[count]);
                        }
                    }
                    count++;
                }
            }
        }

        private void ProcessPlayerRegisteredMessage(ulong clientId, int spawnIndex, PlayerData playerData)
        {
            PlayerDatas.Add(clientId, playerData);
            if(PlayerControllers.TryGetValue(clientId,out var controller))
            {
                controller.SetData(clientId, playerData, WorldData.Instance.WaterArea);
            }
        }
        private void ProcessPlayerControllerSpawnedEvent(PlayerControllerSpawnedEvent x)
        {
            if (PlayerDatas.TryGetValue(x.Controller.OwnerClientId, out var playerData))
            {
                x.Controller.SetData(x.Controller.OwnerClientId, playerData, WorldData.Instance.WaterArea);
            }
            PlayerControllers.Add(x.Controller.OwnerClientId, x.Controller);
        }

        private void NetworkManager_OnClientConnectedCallback(ulong obj)
        {
            using (FastBufferWriter writer =new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
            {
                var data = new PlayerDataStruct(GameSceneData.Players[0]);
                writer.WriteNetworkSerializable(in data);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    GameManagerMessage.RegisterPlayerMessage.ToString(),
                    NetworkManager.ServerClientId,
                    writer,
                    NetworkDelivery.ReliableFragmentedSequenced);
            }
        }

        private void ProcessThrowHookEvent(ThrowHookEvent x)
        {
            using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
            {
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    GameManagerMessage.ThrowHookMessage.ToString(),
                    NetworkManager.ServerClientId,
                    writer,
                    NetworkDelivery.ReliableFragmentedSequenced);
            }
        }

        private void ProcessCatchFishResultMessage(ulong playerId, bool attemptResult, string fishId)
        {
            if (PlayerDatas.TryGetValue(playerId, out var player))
            {
                if (attemptResult)
                {
                    if (player.Attempts == null)
                    {
                        player.Attempts = new();
                    }
                    player.Attempts.Add(true);
                    if (player.FishCought == null)
                    {
                        player.FishCought = new();
                    }
                    if (player.FishCought.ContainsKey(fishId))
                    {
                        player.FishCought[fishId]++;
                    }
                    else
                    {
                        player.FishCought.Add(fishId, 1);
                    }
                }
                else
                {
                    player.Attempts.Add(false);
                }
            }
        }

        private void ProcessThrowAllHookMessage(ulong clientId)
        {
            PlayerControllers[clientId].ThrowHook();
        }

        private void ProcessLiftHookMessage(ulong clientId)
        {
            PlayerControllers[clientId].LiftHook(null);
        }
    }
}