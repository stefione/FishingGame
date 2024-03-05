using PixPlays.Fishing.Configuration;
using PixPlays.Fishing.GameManagement;
using PixPlays.Framework.Network;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace PixPlays.Fishing.Network {
    public class NetworkController : MonoBehaviour
    {
        public GameConfiguration GameConfig;
        public NetworkManager NetworkManagerTemplate;
        private void Awake()
        {
            Instantiate(NetworkManagerTemplate);
            if (NetworkStartup.GetIsServer())
            {
                if (SceneManager.GetActiveScene().name == NetworkConstants.ServerScene)
                {
                    ServerGameManager serverGameManager = Instantiate(GameConfig.ServerGameManagerTemplate);
                }
            }
            else
            {
                ClientGameManager clientGameManager = Instantiate(GameConfig.ClientGameManagerTemplate);
            }
        }
    }
}
