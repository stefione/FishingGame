using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.Fishing.Movement
{
    public abstract class MovementController : MonoBehaviour
    {
        public float MovementSpeed;
        public float RotationSpeed;
        public event Action<Vector3,Vector3> OnDestinationReached;
        public event Action<Vector3, float> OnNewDestinationSet;
        public Vector3 Destination { get; protected set; }

        public abstract void Teleport(Vector3 location);
        protected bool _isMoving = false;

        public void MoveTo(Vector3 destination)
        {
            Destination = destination;
            if (!_isMoving)
            {
                MoveToImplementation();
                _isMoving = true;

            }
            OnNewDestinationSet?.Invoke(Destination,MovementSpeed);
        }

        protected abstract void MoveToImplementation();

        protected void DestinationReached() 
        {
            _isMoving = false;
            OnDestinationReached?.Invoke(GetPosition(),GetForward());
        }

        public abstract Vector3 GetPosition();
        public abstract Vector3 GetForward();
    }
}