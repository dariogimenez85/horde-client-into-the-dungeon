using TMPro;
using UnityEngine;
using DG.Tweening;

namespace SO_Scripts
{
    [CreateAssetMenu(fileName = "TimeUI", menuName = "Scriptable's/TimeUI")]
    public class TimeUIRef : ScriptableObject
    {
        private TextMeshProUGUI timeText;

        public void Initialize(TextMeshProUGUI texRef)
        {
            timeText = texRef;
        }

        public void SetTime(string strTime, bool isAnimate)
        {
            if (timeText == null) return;

            timeText.text = strTime;
            if (isAnimate) SimpleAnimateTimeText();
        }

        private void SimpleAnimateTimeText()
        {
            timeText.transform.DOPunchScale(new Vector2(0.1f, 0.1f), 0.25f, 5, 1);
        }
    }
}