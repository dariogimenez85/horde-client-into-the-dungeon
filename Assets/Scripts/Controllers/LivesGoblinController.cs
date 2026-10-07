using TMPro;
using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;
using Controllers;


public class LivesGoblinController : MonoBehaviour
{
    [Header("References"), Space]
    [SerializeField] private GameStateData stateData;
    [SerializeField] private GameParseData gameParseData;

    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI liveBonusText;
    [SerializeField] private PlayerController playerController;

    private bool isInitialized = false;
    private int currentLives;
    public Action NoLifes = delegate { };
    public Action<int> SetBonus = delegate { };

    private void OnEnable()
    {
        // Suscribirse a eventos de actualización de estado
        stateData.OnUpdateGoblins += UpdateLives;
        GameBridge.OnExtraGoblins += ExtraGoblins;
        // GameBridge.OnPlayerQuit += GoblinLeftBonus;
        playerController.OnGoblinDead += AnimateLivesUI;
        GameBridge.OnResyncView += SyncLives;
        GameBridge.OnGameClosed += Reset;
    }

    private void OnDisable()
    {
        // Desuscribirse de eventos para evitar fugas de memoria
        stateData.OnUpdateGoblins -= UpdateLives;
        GameBridge.OnExtraGoblins -= ExtraGoblins;
        // GameBridge.OnPlayerQuit -= GoblinLeftBonus;
        playerController.OnGoblinDead -= AnimateLivesUI;
        GameBridge.OnResyncView -= SyncLives;
        GameBridge.OnGameClosed -= Reset;
    }

    private void ExtraGoblins()
    {
        currentLives = stateData.currentStateData.goblins;
        AnimateLivesUI();
    }

    private void UpdateLives(int goblins)
    {
        // actualizar el número de goblins 
        currentLives = goblins;

        // mostrar las viadas en la ui la primera vez que inicia el juego
        if (!isInitialized)
        {
            isInitialized = true;
            livesText.text = currentLives.ToString();
        }
    }

    // Resincronización: las vidas del estado nuevo, sin animar.
    private void SyncLives()
    {
        currentLives = stateData.currentStateData.goblins;
        isInitialized = true;
        livesText.text = currentLives.ToString();
    }

    public void GoblinLeftBonus()
    {
        int bonusGoblins = gameParseData.GetGoblinBonusData();
        // Sin bonus de vidas (o sin vidas que contar) no hay animación, pero igual se cierra.
        if (bonusGoblins > 0 && currentLives > 0) StartCoroutine(GoblinBonus(bonusGoblins));
        else StartCoroutine(CloseWithoutBonus());
    }

    private IEnumerator CloseWithoutBonus()
    {
        yield return new WaitForSeconds(1.5f);
        GameBridge.instance.GoToResults();
    }

    private IEnumerator GoblinBonus(int bonusGoblins)
    {
        yield return new WaitForSeconds(1);
        int val = bonusGoblins / currentLives;
        int total = 0;
        // currentLives = 8;
        // livesText.text = currentLives.ToString();

        yield return new WaitForSeconds(0.15f);
        liveBonusText.transform.DOScale(Vector3.one, 0.1f).SetEase(Ease.InOutBounce);


        for (int i = currentLives; i > 0; i--)
        {
            total += val;
            yield return new WaitForSeconds(0.15f);
            AudioController.Instance.PlaySound(nameof(AudioIds.pointCell));
            liveBonusText.transform.DOPunchScale(Vector2.one * 0.1f, 0.1f, 1, 0.2f);
            liveBonusText.transform.DOPunchRotation(new Vector3(0, 0, 5), 0.1f, 1, 0.2f);
            liveBonusText.text = $"Bonus + {total}";
            livesText.transform.DOPunchScale(Vector2.one * 0.1f, 0.1f, 1, 0.2f);
            livesText.text = i.ToString();
        }

        // Forzamos a 0
        livesText.text = "0";
        SetBonus?.Invoke(total);

        yield return new WaitForSeconds(1.5f);
        liveBonusText.transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(0.25f);
        GameBridge.instance.GoToResults();
    }

    private void AnimateLivesUI()
    {
        // Actualizar la visualización de vidas de los goblins
        Debug.Log($"Número de goblins actualizados: {currentLives}");
        livesText.text = currentLives.ToString();

        switch (currentLives)
        {
            case 10:
                AudioController.Instance.PlaySound(nameof(AudioIds.x10Goblins));
                break;
            case 3:
                AudioController.Instance.PlaySound(nameof(AudioIds.x3Goblins));
                break;
            case 0:
                AudioController.Instance.StopBgm();
                AudioController.Instance.PlaySound(nameof(AudioIds.gameOver));
                NoLifes?.Invoke();
                break;
            default:
                break;
        }
        

        // Animación de actualización de texto
        livesText.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 10, 0.5f)
            .OnComplete(() => livesText.transform.localScale = Vector3.one);
    }

    private void Reset()
    {
        isInitialized = false;
        currentLives = 0;
        livesText.text = "0";
        liveBonusText.text = "";
        liveBonusText.transform.localScale = Vector3.zero;
    }
}