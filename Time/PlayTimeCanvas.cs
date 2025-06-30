using UnityEngine;
using TMPro;
using UniRx;

namespace BeardAscetic
{
    public class PlayTimeCanvas : MonoBehaviour
    {
        [Tooltip("남은 시간을 보여줄 TMP_Text")]
        public TMP_Text playTimeText;

        private void Start()
        {
            playTimeText.text = "00:00";

            TimeManager.Instance.RemainingTime
                .Subscribe(UpdateDisplay)
                .AddTo(this);

            TimeManager.Instance.OnTimerCompleted
                .Subscribe(_ => playTimeText.text = "00:00")
                .AddTo(this);
        }

        private void UpdateDisplay(float remainingSeconds)
        {
            int minutes = Mathf.FloorToInt(remainingSeconds / 60f);
            int seconds = Mathf.FloorToInt(remainingSeconds % 60f);
            playTimeText.text = $"{minutes:00}:{seconds:00}";
        }
    }
}