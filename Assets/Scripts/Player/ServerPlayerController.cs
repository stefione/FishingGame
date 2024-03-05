using PixPlays.Fishing.GameManagement;
using PixPlays.Framework.Events;

namespace PixPlays.Fishing.Player
{
    public class ServerPlayerController : BasePlayerController
    {

        private void Awake()
        {
            EventManager.Subscribe<OnServerStartedEvent>(x => ProcessOnServerStartedEvent(x));
        }

        private void ProcessOnServerStartedEvent(OnServerStartedEvent eventData)
        {
        }
    }
}