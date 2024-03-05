using PixPlays.Fishing.Player;
namespace PixPlays.Fishing.GameManagement
{
    public class PlayerRegisteredEvent
    {
        public PlayerData PlayerData;
        public ClientPlayerController Controller;
        public ulong ClientId;
        public int SpawnPointIndex;
    }
}