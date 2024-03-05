
using PixPlays.Fishing.World;
using System;
using UnityEngine;

namespace PixPlays.Fishing.Fish
{
    public class ClientFishController : BaseFishController
    {
        public float SyncLerpSpeed = 3;
        public Vector3 TargetPos { get; private set; }

        public override void SetData(ulong id,FishTemplate fishTemplate,FishData fishData, WaterArea waterArea, Vector3 startLocation)
        {
            base.SetData(id, fishTemplate, fishData, waterArea, startLocation);

            FishTemplate template = Instantiate(fishTemplate, transform);
            _movementController = template.MovementController;
            _originalMovementSpeed = _movementController.MovementSpeed;
            _states.Add(FishStates.RandomMove, new RandomMoveState(this, _movementController, _waterArea));
            _states.Add(FishStates.BiteHook, new BiteHookState(_movementController, this, OnBiteHookFinish, OnBiteHookCancel));
            _states.Add(FishStates.HookedState, new HookedState(this, _movementController,OnBiteHookCancel));
            _states.Add(FishStates.OutOfWater, new OutOfWaterState(_movementController, _originalMovementSpeed, waterArea, OnReturnToWater));
            _states.Add(FishStates.SyncedMoveState, new SyncedMoveState(this, _movementController));
            _movementController.Teleport(startLocation);

            SetState(FishStates.SyncedMoveState);
        }

        private void OnBiteHookCancel()
        {
            SetState(FishStates.SyncedMoveState);
        }

        public void UpdatePosition(Vector3 pos)
        {
            TargetPos = pos;
        }

        protected override void OnReturnToWater()
        {
            SetState(FishStates.SyncedMoveState);
        }

        public override void ResetFish()
        {
            PlayerOwner = null;
            SetState(FishStates.OutOfWater);
        }
    }
}
