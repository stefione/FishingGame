using PixPlays.Fishing.Movement;
using PixPlays.Fishing.Player;
using PixPlays.Fishing.States;
using PixPlays.Fishing.World;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace PixPlays.Fishing.Entities
{
    public class ServerFishController : BaseFishController
    {
        private bool _isInitalized;
        private Dictionary<FishStates, EntityState> _states = new();

        public bool IsOwned => Owner != null;

        private float _originalMovementSpeed;

        public Vector3 GetPosition()
        {
            return _movementController.GetPosition();
        }


        private void Update()
        {
            if (!IsOwned && _movementController.GetPosition().y > _waterArea.TopRight.position.y)
            {
                if (!_states[FishStates.OutOfWater].Active)
                {
                    foreach (var i in _states)
                    {
                        i.Value.Deactivate();
                    }
                    _states[FishStates.OutOfWater].Activate();
                }
            }
        }

        public void SetData(FishData fishData, WaterArea waterArea, Vector3 startLocation)
        {
            if (_isInitalized)
            {
                return;
            }
            FishData = fishData;
            _waterArea = waterArea;
            _movementController = GetComponent<MovementController>();
            _originalMovementSpeed = _movementController.MovementSpeed;
            _states.Add(FishStates.RandomMove, new RandomMoveState(new FishController(),_movementController, _waterArea));
            _states.Add(FishStates.BiteHook, new BiteHookState(_movementController, this, OnBiteHookFinish));
            _states.Add(FishStates.HookedState, new HookedState(this, _movementController));
            _states.Add(FishStates.OutOfWater, new OutOfWaterState(_movementController, waterArea, OnReturnToWater));

            _movementController.Teleport(startLocation);

            _states[FishStates.RandomMove].Activate();

            _isInitalized = true;
        }

        private void OnReturnToWater()
        {
            RandomMove();
        }

        public void RandomMove()
        {
            foreach (var i in _states)
            {
                i.Value.Deactivate();
            }
            _states[FishStates.RandomMove].Activate();
        }

        private void OnBiteHookFinish()
        {
            foreach (var i in _states)
            {
                i.Value.Deactivate();
            }
            _states[FishStates.HookedState].Activate();
            Invoke(nameof(DelayLiftHook), 1f);
        }

        private void DelayLiftHook()
        {
            Owner.LiftHook(this);
        }

        public void BiteHook(ClientPlayerController playerController)
        {
            Owner = playerController;
            foreach (var i in _states)
            {
                i.Value.Deactivate();
            }
            _states[FishStates.BiteHook].Activate();
        }

        public void ResetFish()
        {
            Owner = null;
            RandomMove();
        }
    }
}
