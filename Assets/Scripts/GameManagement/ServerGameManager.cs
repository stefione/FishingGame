using PixPlays.Fishing.Fish;
using PixPlays.Fishing.Player;
using PixPlays.Fishing.RandomGenerator;
using PixPlays.Fishing.UI;
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
using UnityEngine.XR;
using Random = UnityEngine.Random;
namespace PixPlays.Fishing.GameManagement
{

    public class ServerGameManager : BaseGameManager
    {
        protected Dictionary<ulong, ServerPlayerController> PlayerControllers = new();
        protected Dictionary<ulong, ServerFishController> spawnedFish = new();
        protected Dictionary<ulong, int> _spawnPointsTaken=new();
        public float PositionUpdateTickTime=0.035f;
        private int _spawnPointIndex;
        private ulong _fishIndex;

        private void Awake()
        {
            Application.targetFrameRate = 30;
        }

        private void Start()
        {
            EventManager.Subscribe<TryCatchFishEvent>(x => ProcessTryCatchFishEvent(x));
            EventManager.Subscribe<LiftHookEvent>(x => ProcessLiftHookEvent(x));
            NetworkManager.Singleton.OnServerStarted += NetworkManager_OnServerStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectedCallback;
            NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_OnConnectionApprovalCallback;
            NetworkManager.Singleton.StartServer();
        }

        private void NetworkManager_OnConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            if (PlayerDatas.Count == 2)
            {
                response.Approved = false;
                response.Reason = "Server is full";
            }
            else
            {
                response.Approved = true;
            }
        }
        private void NetworkManager_OnClientDisconnectedCallback(ulong obj)
        {
            if(PlayerControllers.TryGetValue(obj, out var playerController))
            {
                Destroy(playerController.gameObject);
                PlayerControllers.Remove(obj);
            }
            PlayerDatas.Remove(obj);
            _spawnPointsTaken.Remove(obj);
            SendClientDisconnectedMessage(obj);

        }
        private void NetworkManager_OnServerStarted()
        {
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                GameManagerMessageType.RegisterPlayerMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                {
                    reader.ReadNetworkSerializable(out PlayerDataMessage data);
                    PlayerData playerData = new PlayerData(data);
                    ProcessRegisterPlayerMessage(sender, playerData);
                });
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                GameManagerMessageType.ThrowHookRequestMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                {
                    ProcessThrowHookRequestMessage(sender);
                });
            SpawnFishes();
            EventManager.Fire(new OnServerStartedEvent());
        }
        private void NetworkManager_OnClientConnectedCallback(ulong obj)
        {
            SendFishData(obj);
            SendPlayerData(obj);
        }



        private void ProcessThrowHookRequestMessage(ulong sender)
        {
            if (PlayerControllers[sender].CanThrow)
            {
                PlayerControllers[sender].ThrowHook();
                SendThrowHookAllMessage(sender);
            }
        }
        private void ProcessRegisterPlayerMessage(ulong clientId,PlayerData playerData)
        {
            int count = 0;
            while (_spawnPointsTaken.ContainsValue(_spawnPointIndex))
            {
                _spawnPointIndex = (_spawnPointIndex + 1) % WorldData.Instance.SpawnPoints.Count;
                count++;
                if (count >= WorldData.Instance.SpawnPoints.Count)
                {
                    return;
                }
            }

            PlayerDatas.Add(clientId, playerData);
            ServerPlayerController playerController = Instantiate(GameConfig.ServerPlayerTemplate, WorldData.Instance.SpawnPoints[_spawnPointIndex]);
            playerController.SetData(clientId, playerData, WorldData.Instance.WaterArea,true, _spawnPointIndex);
            PlayerControllers.Add(clientId, playerController);
            SendPlayerRegisteredMessage(clientId, playerData, _spawnPointIndex);
            _spawnPointsTaken.Add(clientId,_spawnPointIndex);
            _spawnPointIndex=(_spawnPointIndex + 1) % WorldData.Instance.SpawnPoints.Count;
        }



        private void ProcessLiftHookEvent(LiftHookEvent x)
        {
            if (PlayerControllers.TryGetValue(x.OwnerId, out var controller))
            {
                if (x.Success && spawnedFish.TryGetValue(x.FishId, out var fish))
                {
                    controller.LiftHook(fish);
                }
                else
                {
                    controller.LiftHook(null);
                    SendLiftHookAllMessage(x);
                }
            }
        }
        private void ProcessTryCatchFishEvent(TryCatchFishEvent x)
        {
            if (PlayerDatas.TryGetValue(x.PlayerID, out var player))
            {
                bool attemptResult = false;
                ulong fishId = 0;
                FishData fishData = null;
                if (RNG.GetSuccessAttempt(player.AttemptLogSuccesses, GameConfig.SampleAttempts, GameConfig.SuccessChance))
                {
                    attemptResult = true;
                    ServerFishController fish = CatchRandomFish();
                    fish.BiteHook(PlayerControllers[x.PlayerID]);
                    fishId = fish.Id;
                    fishData = fish.FishData;
                }
                else
                {
                    StartCoroutine(Coroutine_DelayedLiftHook(x.PlayerID));
                }
                player.UpdateAttempt(attemptResult, fishData);
                SendCatchFishResultMessage(x.PlayerID, attemptResult, fishId);
            }
        }



        private void SendLiftHookAllMessage(LiftHookEvent x)
        {
            using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
            {
                writer.WriteValueSafe(x.OwnerId);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    GameManagerMessageType.LiftHookAllMessage.ToString(),
                    NetworkManager.Singleton.ConnectedClientsIds,
                    writer,
                    NetworkDelivery.ReliableFragmentedSequenced);
            }
        }
        private void SendThrowHookAllMessage(ulong sender)
        {
            using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
            {
                writer.WriteValueSafe(sender);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    GameManagerMessageType.ThrowHookAllMessage.ToString(),
                    NetworkManager.Singleton.ConnectedClientsIds,
                    writer,
                    NetworkDelivery.ReliableFragmentedSequenced);
            };
        }
        private void SendPlayerRegisteredMessage(ulong clientId, PlayerData playerData,int spawnPointIndex)
        {
            using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
            {
                var data = new PlayerDataMessage(playerData);
                writer.WriteValueSafe(clientId);
                writer.WriteValueSafe(WorldData.Instance.SpawnPoints[_spawnPointIndex].position);
                writer.WriteValueSafe(spawnPointIndex);
                writer.WriteNetworkSerializable(in data);

                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    GameManagerMessageType.PlayerRegisteredMessage.ToString(),
                    NetworkManager.Singleton.ConnectedClientsIds,
                    writer,
                    NetworkDelivery.ReliableFragmentedSequenced);
            };
        }
        private void SendCatchFishResultMessage(ulong playerId, bool attemptResult, ulong fishId)
        {
            using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
            {
                writer.WriteValueSafe(playerId);
                writer.WriteValueSafe(attemptResult);
                writer.WriteValueSafe(fishId);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    GameManagerMessageType.CatchFishResultMessage.ToString(),
                    NetworkManager.Singleton.ConnectedClientsIds,
                    writer,
                    NetworkDelivery.ReliableFragmentedSequenced);
            };
        }
        private void SendFishData(ulong clientId)
        {
            using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
            {
                var fishNetwork = new FishesDataMessage()
                {
                    NetworkIds = new ulong[spawnedFish.Count],
                    Positions = new Vector3[spawnedFish.Count],
                    FishIds = new()
                };
                int index = 0;
                foreach (var i in spawnedFish)
                {
                    fishNetwork.NetworkIds[index] = i.Key;
                    fishNetwork.FishIds.Add(i.Value.FishData.Id);
                    fishNetwork.Positions[index] = i.Value.GetPosition();
                    index++;
                }
                writer.WriteNetworkSerializable(in fishNetwork);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    GameManagerMessageType.SpawnFishMessage.ToString(),
                    clientId,
                    writer,
                    NetworkDelivery.ReliableFragmentedSequenced);
            };
        }
        private void SendFishPositions()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
                {
                    FishesUpdateMessage fishUpdatekData = new()
                    {
                        Ids = new ulong[spawnedFish.Count],
                        Positions = new Vector3[spawnedFish.Count]
                    };
                    int index = 0;
                    foreach (var i in spawnedFish)
                    {
                        fishUpdatekData.Ids[index] = i.Key;
                        fishUpdatekData.Positions[index] = i.Value.GetPosition();
                        index++;
                    }
                    writer.WriteNetworkSerializable(in fishUpdatekData);
                    NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                        GameManagerMessageType.UpdateFishPositionsMessage.ToString(),
                        NetworkManager.Singleton.ConnectedClientsIds,
                        writer,
                        NetworkDelivery.ReliableFragmentedSequenced);
                }
            }
        }
        private void SendPlayerData(ulong clientId)
        {
            using(FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
            {
                PlayersDataMessage playerNetworkData = new()
                {
                    PlayerDataStructs = new List<PlayerDataMessage>(),
                    Positions = new Vector3[PlayerDatas.Count],
                    NetworkIds = new ulong[PlayerDatas.Count],
                    SpawnPointIndicies=new int[PlayerDatas.Count]
                };
                int i = 0;
                foreach(var playerData in PlayerDatas)
                {
                    if (PlayerControllers.TryGetValue(playerData.Key,out var controller))
                    {
                        playerNetworkData.Positions[i] = controller.transform.position;
                        playerNetworkData.SpawnPointIndicies[i] = controller.SpawnPointIndex;
                    }
                    playerNetworkData.NetworkIds[i] = playerData.Key;
                    playerNetworkData.PlayerDataStructs.Add(new PlayerDataMessage(playerData.Value));
                    i++;
                }
                writer.WriteNetworkSerializable(in playerNetworkData);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    GameManagerMessageType.PlayersMessage.ToString(),
                    clientId,
                    writer,
                    NetworkDelivery.ReliableFragmentedSequenced);
            }
        }
        private void SendClientDisconnectedMessage(ulong clientId)
        {
            if (NetworkManager.Singleton == null)
            {
                return;
            }
            using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
            {
                writer.WriteValueSafe(clientId);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    GameManagerMessageType.ClientDisconnectedMessage.ToString(),
                    NetworkManager.Singleton.ConnectedClientsIds,
                    writer,
                    NetworkDelivery.ReliableFragmentedSequenced);
            };
        }


        private IEnumerator Coroutine_DelayedLiftHook(ulong playerId)
        {
            yield return new WaitForSeconds(Random.Range(1.5f, 4f));
            EventManager.Fire(new LiftHookEvent()
            {
                Success = false,
                FishId = 0,
                OwnerId = playerId
            });
        }
        private ServerFishController CatchRandomFish()
        {
            Dictionary<ulong, float> items = new();
            foreach (var i in spawnedFish)
            {
                if (!i.Value.IsOwned)
                {
                    items.TryAdd(i.Key, i.Value.FishData.CatchChance);
                }
            }
            ulong fishKey = RNG.SelectRandomItem(items);
            return spawnedFish[fishKey];
        }
        private void SpawnFishes()
        {
            Dictionary<FishData, float> fishSpawnDict = new();
            foreach(var i in GameConfig.FishDatas)
            {
                fishSpawnDict.Add(i.Value.Data, i.Value.Data.SpawnChance);
            }
            List<FishData> fishToSpawn = RNG.SelectRandomItems(fishSpawnDict,GameConfig.NumberOfFish);
            foreach(var i in fishToSpawn)
            {
                if (GameConfig.FishTemplates.TryGetValue(i.Id, out var fishTemplate))
                {
                    float x = Random.Range(WorldData.Instance.WaterArea.BottomLeft.position.x, WorldData.Instance.WaterArea.TopRight.position.x);
                    float y = Random.Range(WorldData.Instance.WaterArea.BottomLeft.position.y, WorldData.Instance.WaterArea.TopRight.position.y);
                    ServerFishController fish = Instantiate(GameConfig.ServerFishController);
                    fish.SetData(_fishIndex, fishTemplate, i, WorldData.Instance.WaterArea, new Vector3(x, y, 0));
                    spawnedFish.Add(_fishIndex, fish);
                    _fishIndex++;
                }
            }
            StartCoroutine(Coroutine_FishUpdate());
        }
        IEnumerator Coroutine_FishUpdate()
        {
            while (true)
            {
                SendFishPositions();
                yield return new WaitForSeconds(PositionUpdateTickTime);
            }
        }
    }
}
