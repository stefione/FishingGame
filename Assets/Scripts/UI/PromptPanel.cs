using PixPlays.Framework.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixPlays.Fishing.UI
{
    public class PromptPanel : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _MessageText;
        [SerializeField] TextMeshProUGUI _ButtonText;
        [SerializeField] Button _Button;
        [SerializeField] GameObject _PanelObj;
        private Action _callback;

        private void Awake()
        {
            EventManager.Subscribe<OpenPromptPanel>(x => ProcessOpenPromptPanel(x));
            _Button.onClick.AddListener(ButtonClick);
            _PanelObj.SetActive(false);
        }

        private void ButtonClick()
        {
            _callback?.Invoke();
            _PanelObj.SetActive(false);
        }

        private void ProcessOpenPromptPanel(OpenPromptPanel x)
        {
            _MessageText.text = x.Message;
            _ButtonText.text=x.ButtonText;
            _callback=x.Callback;
            _PanelObj.SetActive(true);
        }
    }
}
