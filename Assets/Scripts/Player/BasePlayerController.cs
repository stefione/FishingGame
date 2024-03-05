using PixPlays.Fishing.Fish;
using PixPlays.Fishing.GameManagement;
using PixPlays.Fishing.World;
using PixPlays.Framework.Events;
using UnityEngine;
namespace PixPlays.Fishing.Player
{
    public class BasePlayerController : MonoBehaviour
    {
        public ulong Id { get; private set; }
        public bool IsOwner { get; private set; }
        public int SpawnPointIndex { get; private set; }
        protected PlayerData _playerData;
        public HookController Hook;
        private BaseFishController _caughtFish;

        public bool CanThrow { get; protected set; }

        public void SetData(ulong playerClientId, PlayerData playerData, WaterArea waterArea, bool isOwner,int spawnPointIndex)
        {
            CanThrow = true;
            SpawnPointIndex= spawnPointIndex;
            IsOwner = isOwner;
            Id = playerClientId;
            _playerData = playerData;
            Hook.SetData(waterArea);
            Hook.OnHookReachedMaxDepth += _hook_OnHookReachedMaxDepth;
            Hook.OnHookLifted += _hook_OnHookLifted;
        }

        public virtual void ThrowHook()
        {
            Hook.ThrowHook();
            CanThrow = false;
        }

        public virtual void LiftHook(BaseFishController fish)
        {
            _caughtFish = fish;
            Hook.LiftHook();
        }
        protected virtual void _hook_OnHookLifted()
        {
            if (_caughtFish != null)
            {
                _caughtFish.ResetFish();
            }
            CanThrow = true;
        }

        protected void _hook_OnHookReachedMaxDepth()
        {
            EventManager.Fire(new TryCatchFishEvent()
            {
                PlayerID = Id
            });
        }
    }
}
