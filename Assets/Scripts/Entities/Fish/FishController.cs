using PixPlays.Fishing.GameManagement;
using PixPlays.Fishing.Hook;
using PixPlays.Fishing.Movement;
using PixPlays.Fishing.Player;
using PixPlays.Fishing.States;
using PixPlays.Fishing.World;
using PixPlays.Framework.Events;
using PixPlays.Framework.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
namespace PixPlays.Fishing.Entities
{

    public class FishSpawnEvent
    {
        public FishController Fish;
    }

    public class FishController : BaseFishController
    {
        private bool _isInitalized;
        private Dictionary<FishStates, EntityState> _states=new();
        private NetworkVariable<Vector3> m_SomeValue = new NetworkVariable<Vector3>();
        public bool IsOwned => Owner!=null;
        public float _SyncLerpSpeed = 10;
        private float _originalMovementSpeed;

        public Vector3 GetPosition()
        {
            return _movementController.GetPosition();
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
            _states.Add(FishStates.RandomMove, new RandomMoveState(this,_movementController, _waterArea));
            _states.Add(FishStates.BiteHook, new BiteHookState(_movementController, this, OnBiteHookFinish));
            _states.Add(FishStates.HookedState, new HookedState(this, _movementController));
            _states.Add(FishStates.OutOfWater, new OutOfWaterState(_movementController, waterArea, OnReturnToWater));

            _movementController.Teleport(startLocation);

            if(IsClient)
            {
                //_movementController.OnNewDestinationSet += _movementController_OnNewDestinationSet;
            }
            else
            {
                _movementController.OnNewDestinationSet += _movementController_OnNewDestinationSet;
                _states[FishStates.RandomMove].Activate();
            }
            _isInitalized = true;
        }

        private void _movementController_OnNewDestinationSet(Vector3 arg1, float arg2)
        {
            if (Input.GetKeyDown(KeyCode.U))
            {
                GoToNewDest_ClientRpc(arg1, arg2);
            }
        }

        [ClientRpc]
        public void GoToNewDest_ClientRpc(Vector3 pos,float speed)
        {
            _movementController.MovementSpeed = speed;
            _movementController.MoveTo(pos);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            EventManager.Fire(new FishSpawnEvent()
            {
                Fish = this
            });
        }

        private void Update()
        {
            if (IsClient)
            {
                return;
            }
            //m_SomeValue.Value = _movementController.GetPosition();
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

        private void OnReturnToWater()
        {
            RandomMove();
        }

        public override void RandomMove()
        {
            foreach(var i in _states)
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
            EventManager.Fire(new LiftHookEvent()
            {
                Fish = this,
                Owner = Owner
            });

        }

        public void BiteHook(ClientPlayerController playerController)
        {
            Owner= playerController;
            foreach (var i in _states)
            {
                i.Value.Deactivate();
            }
            _states[FishStates.BiteHook].Activate();
        }
    }
}
