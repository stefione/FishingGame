using ParrelSync;
using PixPlays.Fishing.Configuration;
using PixPlays.Fishing.GameManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class NetworkController : MonoBehaviour
{
    public GameConfiguration GameConfig;
    private void Awake()
    {
        if (GetArg("-server"))
        {
            ServerGameManager serverGameManager = Instantiate(GameConfig.ServerGameManagerTemplate);
        }
        else
        {
            ClientGameManager clientGameManager = Instantiate(GameConfig.ClientGameManagerTemplate);
        }
    }


    private static bool GetArg(string name)
    {
        if (ClonesManager.IsClone())
        {
            var args = ClonesManager.GetArgument();
            return args.Contains(name);
        }
        return false;
    }
    public static List<S> SerializeList<S, T>(List<S> networkSerializableList, BufferSerializer<T> serializer) where S : INetworkSerializable where T : IReaderWriter
    {
        int length = 0;
        S[] Array;

        if (!serializer.IsReader)
        {
            Array = networkSerializableList.ToArray();
            length = Array.Length;
            serializer.SerializeValue(ref length);
        }
        else
        {
            serializer.SerializeValue(ref length);
            Array = new S[length];
        }

        for (int n = 0; n < length; ++n)
            Array[n].NetworkSerialize(serializer);

        if (serializer.IsReader)
            networkSerializableList = Array.ToList<S>();

        return networkSerializableList;
    }

    public static List<string> SerializeStringList<T>(List<string> networkSerializableList, BufferSerializer<T> serializer) where T : IReaderWriter
    {
        int length = 0;
        string[] Array;

        if (!serializer.IsReader)
        {
            Array = networkSerializableList.ToArray();
            length = Array.Length;
            serializer.SerializeValue(ref length);
        }
        else
        {
            serializer.SerializeValue(ref length);
            Array = new string[length];
        }

        for (int n = 0; n < length; ++n)
            serializer.SerializeValue(ref Array[n]);

        if (serializer.IsReader)
            networkSerializableList = Array.ToList<string>();

        return networkSerializableList;
    }
}
