using PixPlays.Fishing.GameManagement;
using PixPlays.Fishing.Player;
using PixPlays.Framework.Network;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class ServerPlayerController : BasePlayerController
{
    public ulong NetworkId;

    //public void UpdateAttempt(bool success,string fishId)
    //{
    //    _playerData.Attempts.Add(success);
    //    if (!string.IsNullOrEmpty(fishId))
    //    {
    //        if (_playerData.FishCought.ContainsKey(fishId))
    //        {
    //            _playerData.FishCought[fishId]++;
    //        }
    //        else
    //        {
    //            _playerData.FishCought.Add(fishId,1);
    //        }
    //    }
    //    using (FastBufferWriter writer = new FastBufferWriter(NetworkConstants.MaxPayloadSize, Allocator.Temp))
    //    {
    //        if (fishId==null)
    //        {
    //            fishId = "";
    //        }
    //        writer.WriteValueSafe(success);
    //        writer.WriteValueSafe(fishId);
    //        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
    //                          PlayerControllerMessage.AttemptResultMessage.ToString(),
    //                          NetworkId,
    //                          writer,
    //                          NetworkDelivery.ReliableFragmentedSequenced);
    //    }
    //}
}
