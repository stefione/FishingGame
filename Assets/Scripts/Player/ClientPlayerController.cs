using PixPlays.Fishing.Animation;
using PixPlays.Fishing.Fish;
using PixPlays.Fishing.GameManagement;
using PixPlays.Fishing.UI;
using PixPlays.Framework.Events;
using UnityEngine;
namespace PixPlays.Fishing.Player
{
    public class ClientPlayerController : BasePlayerController
    {
        [SerializeField] Transform _BoatTransform;
        [SerializeField] Animator _Anim;
        private AnimationEventsController _animEventsController;
        private void Awake()
        {
            EventManager.Subscribe<OnClientStartedEvent>(x => ProcessOnClientStartedEvent(x));
            _animEventsController=_Anim.GetComponent<AnimationEventsController>();
            _animEventsController.Subscribe("ThrowHook", ThrowHook);
        }
        private void Start()
        {
            if (transform.position.x > 0)
            {
                Vector3 scale = _BoatTransform.localScale;
                scale.x *= -1;
                _BoatTransform.localScale = scale;
            }
            EventManager.Fire(new PlayerDataUpdatedEvent()
            {
                ClientId=Id,
                playerData = _playerData
            });
        }

        private void ProcessOnClientStartedEvent(OnClientStartedEvent eventData)
        {
        }

        public void Update()
        {
            if (IsOwner && CanThrow && Input.GetKeyDown(KeyCode.Space))
            {
                EventManager.Fire(new ThrowHookEvent() { PlayerID = Id });
            }
        }

        public void StartThrowHook()
        {
            _Anim.SetTrigger("ThrowHook");
        }

        public override void LiftHook(BaseFishController fish)
        {
            base.LiftHook(fish);
            _Anim.SetBool("PullingHook",true);
        }

        protected override void _hook_OnHookLifted()
        {
            base._hook_OnHookLifted();
            _Anim.SetBool("PullingHook", false);
            EventManager.Fire(new PlayerDataUpdatedEvent()
            {
                ClientId = Id,
                playerData = _playerData
            });
        }


    }
}