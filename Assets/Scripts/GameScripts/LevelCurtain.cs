using Controllers;
using DG.Tweening;
using UnityEngine;

public class LevelCurtain : MonoBehaviour
{
    private readonly float startPosition = 1887f;
    private readonly float endPosition = -1818f;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private GridSpawner gridSpawner;
    [SerializeField] private GameStateData stateData;
    
    private RectTransform rect;
    
    private void OnEnable()
    {
        playerController.OnGoblinGetTresaure += ChangeLevel;
    }

    private void OnDisable()
    {
        playerController.OnGoblinGetTresaure -= ChangeLevel;
    }

    [ContextMenu("Change Level")]
    private void ChangeLevel()
    {
        // no se ejecuta el cambio de nivel si ya es game over
        if (stateData.currentStateData.gameOver) return;
        
        AudioController.Instance.PlaySound(AudioIds.newLevelFx.ToString());
        rect = GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, startPosition);
        Sequence newLevel = DOTween.Sequence();
        newLevel.Append(rect.DOAnchorPosY(endPosition, 1f).SetEase(Ease.InOutQuad));
        Invoke(nameof(RefreshGrid), 0.5f);
    }

    private void RefreshGrid()
    {
        gridSpawner.NewGridValues();
    }
}