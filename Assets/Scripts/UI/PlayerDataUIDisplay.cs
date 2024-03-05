using PixPlays.Fishing.Player;
using PixPlays.Framework.Events;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PixPlays.Fishing.UI
{
    public class PlayerDataUIDisplay : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _AttemptsText;
        [SerializeField] TextMeshProUGUI _SuccessfulText;
        [SerializeField] TextMeshProUGUI _FishListText;
        [SerializeField] Button _LogButton;
        [SerializeField] TextMeshProUGUI _LogButtonText;
        [SerializeField] GameObject _FishLogObject;

        void Awake()
        {
            _LogButton.onClick.AddListener(LogButtonClick);
        }

        public void UpdateData(PlayerData x)
        {
            _AttemptsText.text = "Total Attempts: " + x.AttemptLogSuccesses.Count.ToString();
            _SuccessfulText.text = "Successful Attempts: " + x.AttemptLogSuccesses.FindAll(x => x == true).Count.ToString();
            string list = "";
            for (int i = x.AttemptLog.Count - 1; i >= 0 && i >= x.AttemptLog.Count - 10; i--)
            {
                list += x.AttemptLog[i] + "\n";
            }
            _FishListText.text = list;
        }

        private void LogButtonClick()
        {
            _FishLogObject.gameObject.SetActive(!_FishLogObject.gameObject.activeInHierarchy);
            if (_FishLogObject.gameObject.activeInHierarchy)
            {
                _LogButtonText.text = "Hide Log";
            }
            else
            {
                _LogButtonText.text = "Show Log";
            }

        }
    }
}
