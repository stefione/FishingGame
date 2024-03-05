using Unity.Netcode;
using UnityEngine;
namespace PixPlays.Fishing.GameManagement
{
    public struct FishesUpdateMessage : INetworkSerializable
    {
        public ulong[] Ids;
        public Vector3[] Positions;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Ids);
            serializer.SerializeValue(ref Positions);
        }
    }
}
