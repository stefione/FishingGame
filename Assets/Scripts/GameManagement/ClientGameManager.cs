
using PixPlays.Fishing.Fish;
using PixPlays.Fishing.Player;
using PixPlays.Fishing.UI;
using PixPlays.Fishing.World;
using PixPlays.Framework.Events;
using PixPlays.Framework.Network;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace PixPlays.Fishing.GameManagement
{

    public class ClientGameManager : BaseGameManager
    {
        protected Dictionary<ulong, ClientFishController> spawnedFish = new();
        protected Dictionary<ulong, ClientPlayerController> PlayerControllers = new();
        public GameSceneData GameSceneData;

        private void Start()
        {
            Application.targetFrameRate = 1000;
            EventManager.Subscribe<ThrowHookEvent>(x => ProcessThrowHookEvent(x));
            EventManager.Subscribe<LiftHookEvent>(x => ProcessLiftHookEvent(x));
            NetworkManager.Singleton.OnClientStarted += NetworkManager_OnClientStarted;
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.StartClient();
            EventManager.Fire(new OpenLoadingScreenEvent()
            {
                Message = "Waiting for server"
            });
        }

        private void NetworkManager_OnClientDisconnectCallback(ulong obj)
        {
            string reason = NetworkManager.Singleton.DisconnectReason;
            if (string.IsNullOrEmpty(reason))
            {
                reason = "Disconnected";
            }
            EventManager.Fire(new OpenPromptPanel()
            {
                ButtonText = "Try Again",
                Callback = TryConnectAgain,
                Message = reason
            });
            EventManager.Fire(new CloseLoadingScreenEvent());
            foreach (var i in PlayerDatas)
            {
                EventManager.Fire(new PlayerDisconnectedEvent()
                {
                    ClientId = i.Key
                });
            }
            Cleanup();
        }
        private void TryConnectAgain()
        {
            NetworkManager.Singleton.StartClient();
            EventManager.Fire(new OpenLoadingScreenEvent()
            {
                Message = "Waiting for server"
            });
        }
        private void NetworkManager_OnClientStarted()
        {
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                GameManagerMessageType.PlayerRegisteredMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                {
                    reader.ReadValueSafe(out ulong clientId);
                    reader.ReadValueSafe(out Vector3 position);
                    reader.ReadValueSafe(out int spawnPointIndex);
                    reader.ReadNetworkSerializable(out PlayerDataMessage data);
                    ProcessPlayerRegisteredMessage(clientId, position, new PlayerData(data), spawnPointIndex);
                });
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                GameManagerMessageType.CatchFishResultMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                {
                    reader.ReadValueSafe(out ulong clientId);
                    reader.ReadValueSafe(out bool attemptResult);
                    reader.ReadValueSafe(out ulong fishId);
                    ProcessCatchFishResultMessage(clientId, attemptResult, fishId);
                });
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                GameManagerMessageType.LiftHookAllMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                {
                    reader.ReadValueSafe(out ulong clientId);
                    ProcessLiftHookMessage(clientId);
                });
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                GameManagerMessageType.ThrowHookAllMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                {
                    reader.ReadValueSafe(out ulong clientId);
                    ProcessThrowAllHookMessage(clientId);
                });
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                GameManagerMessageType.SpawnFishMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                {
                    reader.ReadValueSafe(out FishesDataMessage fishData);
                    ProcessSpawnFishMessage(fishData);
                });
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                GameManagerMessageType.UpdateFishPositionsMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                {
                    reader.ReadNetworkSerializable(out FishesUpdateMessage fishData);
                    ProcessUpdateFishPositionMessage(fishData);
                });
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                GameManagerMessageType.PlayersMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                {
                    reader.ReadNetworkSerializable(out PlayersDataMessage playersNetworkData);
                    ProcessPlayersMessage(playersNetworkData);
                });
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                GameManagerMessageType.ClientDisconnectedMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                {
                    reader.ReadValueSafe(out ulong clientId);
                    ProcessClientDisconnectedMessage(clientId);
                });
            EventManager.Fire(new OnClientStartedEvent());
        }
        private void NetworkManager_OnClientConnectedCallback(ulong obj)
        {
            SendRegisterPlayerMessage();
        }


        private void ProcessThrowHookEvent(ThrowHookEvent x)
        {
            SendThrowHookRequestMessage();
        }
        private void ProcessLiftHookEvent(LiftHookEvent x)
        {
            if (spawnedFish.TryGetValue(x.FishId, out var fish))
            {
                if (PlayerControllers.TryGetValue(x.OwnerId, out var controller))
                {
                    PlayerControllers[x.OwnerId].LiftHook(fish);
                }
            }
        }


        private void ProcessPlayersMessage(PlayersDataMessage playersNetworkData)
        {
            for (int i = 0; i < playersNetworkData.NetworkIds.Length; i++)
            {
                RegisterPlayer(
                    playersNetworkData.NetworkIds[i], 
                    playersNetworkData.Positions[i], 
                    new PlayerData(playersNetworkData.PlayerDataStructs[i]),
                    playersNetworkData.SpawnPointIndicies[i]);
            }
        }
        private void ProcessUpdateFishPositionMessage(FishesUpdateMessage fishData)
        {
            for(int i=0;i<fishData.Ids.Length;i++)
            {
                if (spawnedFish.TryGetValue(fishData.Ids[i],out var fish))
                {
                    fish.UpdatePosition(fishData.Positions[i]);
                }
            }
        }
        private void ProcessSpawnFishMessage(FishesDataMessage fishReceivedDatas)
        {
            for (int i = 0; i < fishReceivedDatas.Positions.Length; i++)
            {
                if (GameConfig.FishDatas.TryGetValue(fishReceivedDatas.FishIds[i], out var fishData))
                {
                    if (GameConfig.FishTemplates.TryGetValue(fishData.Data.Id, out var fishTemplate))
                    {
                        ClientFishController fish = Instantiate(GameConfig.ClientFishController);
                        fish.SetData(fishReceivedDatas.NetworkIds[i], fishTemplate, fishData.Data, WorldData.Instance.WaterArea, fishReceivedDatas.Positions[i]);
                        spawnedFish.Add(fishReceivedDatas.NetworkIds[i], fish);
                    }
                }
            }
        }
        private void ProcessPlayerRegisteredMessage(ulong clientId, Vector3 position, PlayerData playerData,int spawnPointIndex)
        {
            RegisterPlayer(clientId, position, playerData, spawnPointIndex);
            EventManager.Fire(new CloseLoadingScreenEvent());
        }



        private void ProcessCatchFishResultMessage(ulong playerId, bool attemptResult, ulong fishId)
        {
            if (PlayerDatas.TryGetValue(playerId, out var player))
            {
                if (attemptResult)
                {
                    if (spawnedFish.TryGetValue(fishId, out var fish))
                    {
                        fish.BiteHook(PlayerControllers[playerId]);
                        player.UpdateAttempt(true,fish.FishData);
                    }
                }
                else
                {
                    player.UpdateAttempt(false,null);
                }
            }
        }
        private void ProcessThrowAllHookMessage(ulong clientId)
        {
            PlayerControllers[clientId].StartThrowHook();
        }
        private void ProcessLiftHookMessage(ulong clientId)
        {
            if(PlayerControllers.TryGetValue(clientId,out var controller))
            {
                controller.LiftHook(null);
            }
        }
        private void ProcessClientDisconnectedMessage(ulong clientId)
        {
            if (PlayerControllers.TryGetValue(clientId, out var playerController))
            {
                Destroy(playerController.gameObject);
                PlayerControllers.Remove(clientId);
            }
            PlayerDatas.Remove(clientId);
            EventManager.Fire(new PlayerDisconnectedEvent()
            {
                ClientId = clientId
            });
        }


        private void RegisterPlayer(ulong clientId, Vector3 position, PlayerData playerData,int spawnPointIndex)
        {
            PlayerDatas.Add(clientId, playerData);
            ClientPlayerController clientPlayerController = Instantiate(GameConfig.ClientPlayerTemplate, position, Quaternion.identity);
            bool isOwner = NetworkManager.Singleton.LocalClientId == clientId;
            clientPlayerController.SetData(clientId, playerData, WorldData.Instance.WaterArea, isOwner,spawnPointIndex);
            if (!PlayerControllers.TryAdd(clientId, clientPlayerController))
            {
                Debug.LogError("Trying to add controller with same ID");
            }
            EventManager.Fire(new PlayerRegisteredEvent()
            {
                PlayerData = playerData,
                ClientId = clientId,
                SpawnPointIndex=spawnPointIndex
            });
        }
        private static void SendThrowHookRequestMessage()
        {
            using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
            {
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    GameManagerMessageType.ThrowHookRequestMessage.ToString(),
                    NetworkManager.ServerClientId,
                    writer,
                    NetworkDelivery.ReliableFragmentedSequenced);
            }
        }
        private void SendRegisterPlayerMessage()
        {
            using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
            {
                var data = new PlayerDataMessage(GameSceneData.LocalPlayerData);
                writer.WriteNetworkSerializable(in data);
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                    GameManagerMessageType.RegisterPlayerMessage.ToString(),
                    NetworkManager.ServerClientId,
                    writer,
                    NetworkDelivery.ReliableFragmentedSequenced);
            }
        }
        private void Cleanup()
        {
            foreach(var i in PlayerControllers)
            {
                Destroy(i.Value.gameObject);
            }
            foreach(var i in spawnedFish)
            {
                Destroy(i.Value.gameObject);
            }
            spawnedFish.Clear();
            PlayerControllers.Clear();
            PlayerDatas.Clear();
        }
    }
}