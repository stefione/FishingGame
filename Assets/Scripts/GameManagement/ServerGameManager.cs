using PixPlays.Fishing.Entities;
using PixPlays.Fishing.Player;
using PixPlays.Fishing.RandomGenerator;
using PixPlays.Framework.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace PixPlays.Fishing.GameManagement
{
    public class ServerGameManager : BaseGameManager
    {
        private Dictionary<string,ServerPlayerController> _playerControllers=new();

        private void Awake()
        {
            NetworkManager.Singleton.CustomMessagingManager?.RegisterNamedMessageHandler(
                GameManagerMessage.TryCatchFishMessage.ToString(), (ulong sender, FastBufferReader reader) =>
                {
                    reader.ReadValueSafe(out string playerId);
                    ProcessTryCatchFishMessage(playerId);
                });
        }

        private void ProcessTryCatchFishMessage(string playerId)
        {
            if (PlayerDatas.TryGetValue(playerId, out var player))
            {
                bool attemptResult = false;
                ulong fishId = 0;
                if (RNG.RTP_WillCatch(player.Attempts, GameConfig.SampleAttempts, GameConfig.SuccessChance))
                {
                    attemptResult = true;
                    player.Attempts.Add(true);
                    
                    FishController fish = CatchRandomFish();
                    //fish.BiteHook(PlayerControllers[x.PlayerID]);
                    fishId = fish.NetworkId;               
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
                if (_playerControllers.TryGetValue(playerId, out var playerCtrl))
                {
                    using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
                    {
                        writer.WriteValueSafe(playerId);
                        writer.WriteValueSafe(attemptResult);
                        writer.WriteValueSafe(fishId);
                        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
                            GameManagerMessage.CatchFishResultMessage.ToString(),
                            playerCtrl.NetworkId,
                            writer,
                            NetworkDelivery.ReliableFragmentedSequenced);
                    };
                }
            }
        }
        private FishController CatchRandomFish()
        {
            Dictionary<string, float> items = new();
            foreach (var i in fishes)
            {
                if (!i.IsOwned)
                {
                    items.Add(i.FishData.Id, i.FishData.CatchChance);
                }
            }
            string fishKey = RNG.RNG_ItemCought(items);
            return fishes.Find(x => x.FishData.Id == fishKey);
        }
    }
}
