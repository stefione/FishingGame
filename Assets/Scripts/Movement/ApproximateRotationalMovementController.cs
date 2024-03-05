using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PixPlays.Fishing.Movement
{
    public class ApproximateRotationalMovementController : MovementController
    {
        [SerializeField] Transform _AncorObject;
        [SerializeField] float _ErrorDistance;


        public override void Teleport(Vector3 location)
        {
            Vector3 previousLocation = _AncorObject.position;
            _AncorObject.position = location;
            _AncorObject.right=(_AncorObject.position- previousLocation).normalized;
        }

        protected override void MoveToImplementation()
        {
            StopAllCoroutines();
            StartCoroutine(Coroutine_GoToDestination());
        }

        IEnumerator Coroutine_GoToDestination()
        {
            float lerp = 0;
            while ((_AncorObject.position - Destination).magnitude > _ErrorDistance)
            {
                Vector3 right = Vector3.Slerp(_AncorObject.right, (Destination - _AncorObject.position).normalized, lerp);
                right.z = 0;
                _AncorObject.right = right;
                Vector3 newPos = _AncorObject.position + _AncorObject.right * MovementSpeed * Time.deltaTime;
                newPos.z = 0;
                _AncorObject.position = newPos;
                lerp = Mathf.Clamp01(lerp + Time.deltaTime * RotationSpeed);
                yield return null;
            }
            DestinationReached();
        }
        public override Vector3 GetForward()
        {
            return _AncorObject.right;
        }

        public override Vector3 GetPosition()
        {
            return _AncorObject.position;
        }

        public override void SetForward(Vector3 forward)
        {
            _AncorObject.right = forward;
        }
    }
}
