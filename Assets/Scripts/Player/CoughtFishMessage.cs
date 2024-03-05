using Unity.Netcode;
namespace PixPlays.Fishing.Player
{
    public struct CoughtFishMessage : INetworkSerializable
    {
        public string Id;
        public int Count;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref Count);
        }
    }
}
