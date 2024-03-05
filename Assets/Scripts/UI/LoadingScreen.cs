using PixPlays.Framework.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace PixPlays.Fishing.UI
{

    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _Message;
        [SerializeField] GameObject _ScreenObj;

        private void Awake()
        {
            EventManager.Subscribe<OpenLoadingScreenEvent>(x => ProcessOpenLoadingScreenEvent(x));
            EventManager.Subscribe<CloseLoadingScreenEvent>(x => ProcessClassLoadingScreenEvent(x));
        }

        private void ProcessClassLoadingScreenEvent(CloseLoadingScreenEvent x)
        {
            _ScreenObj.SetActive(false);
        }

        private void ProcessOpenLoadingScreenEvent(OpenLoadingScreenEvent x)
        {
            _Message.text = x.Message;
            _ScreenObj.SetActive(true);
        }
    }
}