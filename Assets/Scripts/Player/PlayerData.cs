using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace PixPlays.Fishing.Player
{
    [System.Serializable]
    public class PlayerData
    {
        public string Id;
        public string Name;
        public List<bool> Attempts=new();
        public Dictionary<string, int> FishCought=new();

        public PlayerData(PlayerDataStruct data)
        {
            Id = data.Id;
            Name = data.Name;
            Attempts = data.Attempts;
            if (Attempts == null)
            {
                Attempts = new();
            }
            FishCought = new();
            if (data.FishCought != null)
                foreach (var i in data.FishCought)
                {
                    FishCought.Add(i.Id, i.Count);
                }
        }
    }


    public struct FishCoughtData : INetworkSerializable
    {
        public string Id;
        public int Count;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref Count);
        }
    }

    public struct PlayerDataStruct : INetworkSerializable
    {
        public string Id;
        public string Name;
        public List<bool> Attempts;
        public List<FishCoughtData> FishCought;

        public PlayerDataStruct(PlayerData data)
        {
            Id = data.Id;
            Name = data.Name;
            Attempts = data.Attempts;
            FishCought = new();
            if (data.FishCought != null)
                foreach (var i in data.FishCought)
                {
                    FishCought.Add(new FishCoughtData()
                    {
                        Count = i.Value,
                        Id = i.Key
                    });
                }

        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Id);
            serializer.SerializeValue(ref Name);
            FishCought = NetworkController.SerializeList<FishCoughtData, T>(FishCought, serializer);
        }

    }
}
