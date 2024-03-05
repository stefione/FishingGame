using PixPlays.Fishing.Network;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace PixPlays.Fishing.GameManagement
{
    public struct FishesDataMessage : INetworkSerializable
    {
        public ulong[] NetworkIds;
        public List<string> FishIds;
        public Vector3[] Positions;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref NetworkIds);
            FishIds = NetworkSerializationHelper.SerializeStringList<T>(FishIds, serializer);
            serializer.SerializeValue(ref Positions);
        }
    }
}
