#if UNITY_EDITOR
using ParrelSync;
#endif
using PixPlays.Framework.Network;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixPlays.Fishing.Network
{
    public static class NetworkStartup
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            if (GetIsServer())
            {
                if (SceneManager.GetActiveScene().name != NetworkConstants.ServerScene)
                {

                    SceneManager.LoadScene(NetworkConstants.ServerScene);
                }
            }
            else
            {
                if (SceneManager.GetActiveScene().name != NetworkConstants.ClientScene)
                {
                    SceneManager.LoadScene(NetworkConstants.ClientScene);
                }
            }
        }

        public static bool GetIsServer()
        {
#if UNITY_EDITOR
            if (ClonesManager.IsClone())
            {
                var args = ClonesManager.GetArgument();
                return args.Contains("-server");
            }
            return false;
#elif UNITY_SERVER
            return true;
#else
            return false;
#endif
        }

    }
}
