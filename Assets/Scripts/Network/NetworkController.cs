using ParrelSync;
using PixPlays.Fishing.Configuration;
using PixPlays.Fishing.GameManagement;
using PixPlays.Fishing.RandomGenerator;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkController : MonoBehaviour
{
    public GameConfiguration GameConfig;
    
    private void Awake()
    {


        if (GetArg("-server"))
        {
            if (SceneManager.GetActiveScene().name == "SampleSceneServer")
            {
                ClientGameManager clientGameManager = Instantiate(GameConfig.ClientGameManagerTemplate);
            }
        }
        else
        {
            ClientGameManager clientGameManager = Instantiate(GameConfig.ClientGameManagerTemplate);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        if (GetArg("-server"))
        {
            if (SceneManager.GetActiveScene().name != "SampleSceneServer")
            {
                SceneManager.LoadScene("SampleSceneServer");
            }
        }
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
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


}
