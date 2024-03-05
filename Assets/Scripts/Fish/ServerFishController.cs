using PixPlays.Fishing.Movement;
using PixPlays.Fishing.Player;
using PixPlays.Fishing.States;
using PixPlays.Fishing.World;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
namespace PixPlays.Fishing.Fish
{
    public class ServerFishController : BaseFishController
    {

        public Vector3 GetPosition()
        {
            return _movementController.GetPosition();
        }

        public override void SetData(ulong id, FishTemplate fishTemplate, FishData fishData, WaterArea waterArea, Vector3 startLocation)
        {
            base.SetData(id,fishTemplate,fishData, waterArea, startLocation);
            _movementController = GetComponent<MovementController>();
            _originalMovementSpeed = _movementController.MovementSpeed;
            _states.Add(FishStates.RandomMove, new RandomMoveState(this, _movementController, _waterArea));
            _states.Add(FishStates.BiteHook, new BiteHookState(_movementController, this, OnBiteHookFinish,OnBiteHookCancel));
            _states.Add(FishStates.HookedState, new HookedState(this, _movementController, OnBiteHookCancel));
            _states.Add(FishStates.OutOfWater, new OutOfWaterState(_movementController, _originalMovementSpeed, waterArea, OnReturnToWater));
            _movementController.Teleport(startLocation);
            SetState(FishStates.RandomMove);
        }

        private void OnBiteHookCancel()
        {
            SetState(FishStates.RandomMove);
        }

        private void Update()
        {
            if (!IsOwned && _movementController.GetPosition().y > _waterArea.TopRight.position.y)
            {
                if (!_states[FishStates.OutOfWater].Active)
                {
                    SetState(FishStates.OutOfWater);
                }
            }
        }

        protected override void OnReturnToWater()
        {
            SetState(FishStates.RandomMove);
        }

        public override void ResetFish()
        {
            PlayerOwner = null;
            SetState(FishStates.RandomMove);
        }
    }
}
