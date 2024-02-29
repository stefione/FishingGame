using PixPlays.Fishing.World;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
namespace PixPlays.Fishing.Hook
{
    public class HookController : MonoBehaviour
    {
        [SerializeField] Transform _HookSource;
        [SerializeField] Transform _HookTransform;
        [SerializeField] LineRenderer _Line;
        [SerializeField] AnimationCurve _hookDropSpeedCurve;
        [SerializeField] AnimationCurve _hookLiftSpeedCurve;
        [SerializeField] float _DropSpeed;
        [SerializeField] float _LiftSpeed;
        public event Action OnHookReachedMaxDepth;
        public event Action OnHookLifted;

        public Vector3 HookPosition => _HookTransform.position;

        private WaterArea _waterArea;
        public void SetData(WaterArea waterArea)
        {
            _waterArea= waterArea;
        }

        public void ThrowHook()
        {
            StopAllCoroutines();
            StartCoroutine(Coroutine_ThrowHook());
        }

        public void LiftHook()
        {
            StopAllCoroutines();
            StartCoroutine(Coroutine_LiftHook());
        }

        IEnumerator Coroutine_ThrowHook()
        {
            Vector3 startPos = _HookTransform.position;
            float waterRange = _waterArea.TopRight.position.y - _waterArea.BottomLeft.position.y;
            float hookHeight = _waterArea.BottomLeft.position.y + waterRange * Random.Range(0.1f, 0.4f);
            Vector3 endPos = _HookTransform.position;
            endPos.y = hookHeight;
            float lerp = 0;
            while (lerp < 1)
            {
                _HookTransform.position = Vector3.Lerp(startPos, endPos, _hookDropSpeedCurve.Evaluate(lerp));
                lerp += Time.deltaTime* _DropSpeed;
                yield return null;
            }
            OnHookReachedMaxDepth?.Invoke();
        }

        IEnumerator Coroutine_LiftHook()
        {
            Vector3 startPos = _HookTransform.position;
            Vector3 endPos = _HookSource.position;
            float lerp = 0;
            while (lerp < 1)
            {
                _HookTransform.position = Vector3.Lerp(startPos, endPos, _hookLiftSpeedCurve.Evaluate(lerp));
                lerp += Time.deltaTime*_LiftSpeed;
                yield return null;
            }
            OnHookLifted?.Invoke();
        }
    }
}
