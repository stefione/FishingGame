using PixPlays.Fishing.Network;
using PixPlays.Fishing.Player;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace PixPlays.Fishing.GameManagement
{
    public struct PlayersDataMessage : INetworkSerializable
    {
        public ulong[] NetworkIds;
        public List<PlayerDataMessage> PlayerDataStructs;
        public Vector3[] Positions;
        public int[] SpawnPointIndicies;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref NetworkIds);
            serializer.SerializeValue(ref SpawnPointIndicies);
            PlayerDataStructs = NetworkSerializationHelper.SerializeList<PlayerDataMessage,T>(PlayerDataStructs, serializer);
            serializer.SerializeValue(ref Positions);
        }
    }
}
