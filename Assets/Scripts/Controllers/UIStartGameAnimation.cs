
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;


namespace Controllers
{
    public class UIStartGameAnimation : MonoBehaviour
    {
        [SerializeField] private List<Transform> uiObjects = new List<Transform>();

        private void Start()
        {
            foreach (Transform uiObject in uiObjects)
            {
                TextMeshProUGUI txtComponent = uiObject.GetComponent<TextMeshProUGUI>();
                if (txtComponent != null)
                    txtComponent.alpha = 0;

                uiObject.localScale = Vector3.zero;
            }
        }

        public void AnimateUIObjects()
        {
            Sequence seq = DOTween.Sequence();

            foreach (Transform uiObject in uiObjects)
            {
                TextMeshProUGUI txtComponent = uiObject.GetComponent<TextMeshProUGUI>();
                if (txtComponent == null)
                {
                    Debug.LogWarning($"No TMP component on {uiObject.name}");
                    continue;
                }

                float flip = uiObject.name == "TimerTxT" ? -1f : 1f;
                seq.Join(txtComponent.DOFade(1, 0.3f).SetEase(Ease.Linear));
                seq.Join(uiObject.DOScale(new Vector3(flip, 1, 1), 0.3f).SetEase(Ease.OutQuad));
            }

            seq.OnComplete(() =>
            {
                foreach (Transform uiObject in uiObjects)
                {
                    float flip = uiObject.name == "TimerUI" ? -0.1f : 0.1f;
                    uiObject.DOPunchScale(new Vector3(flip, 1, 1), 0.3f, 5, 0.5f).SetEase(Ease.OutQuad);
                }
            });
        }
    }
}