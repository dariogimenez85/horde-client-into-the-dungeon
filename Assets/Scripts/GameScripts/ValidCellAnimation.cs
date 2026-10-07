
using DG.Tweening;
using UnityEngine;

public class ValidCellAnimation : MonoBehaviour
{
    private Vector3 finalScale= new Vector3(1.35f, 1.35f, 1.35f);
    private Vector3 initialScale= Vector3.zero;

    private void OnEnable()
    {
        Sequence showCell = DOTween.Sequence();
        showCell.Append(transform.DOScale(finalScale, 0.2f).SetEase(Ease.OutBack));
        showCell.Append(transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 10, 0.1f).SetEase(Ease.OutBack));
    }

    private void OnDisable()
    {
        // Reset the scale when the animation is disabled
        transform.localScale = initialScale;
    }
}
