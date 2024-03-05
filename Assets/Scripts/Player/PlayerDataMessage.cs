using PixPlays.Fishing.Network;
using System.Collections.Generic;
using Unity.Netcode;
namespace PixPlays.Fishing.Player
{
    public struct PlayerDataMessage : INetworkSerializable
    {
        public string Name;
        public bool[] AttemptLogSuccesses;
        public List<string> AttemptLog;
        public List<CoughtFishMessage> FishCought;

        public PlayerDataMessage(PlayerData data)
        {
            Name = data.Name;
            AttemptLogSuccesses = data.AttemptLogSuccesses.ToArray();
            AttemptLog = data.AttemptLog;
            FishCought = new();
            if (data.FishCought != null)
                foreach (var i in data.FishCought)
                {
                    FishCought.Add(new CoughtFishMessage()
                    {
                        Count = i.Value,
                        Id = i.Key
                    });
                }

        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Name);
            serializer.SerializeValue(ref AttemptLogSuccesses);
            AttemptLog = NetworkSerializationHelper.SerializeStringList<T>(AttemptLog, serializer);
            FishCought = NetworkSerializationHelper.SerializeList<CoughtFishMessage, T>(FishCought, serializer);
        }

    }
}
