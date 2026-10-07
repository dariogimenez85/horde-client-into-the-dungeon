

using System.Collections;
using Controllers;
using TMPro;
using UnityEngine;



public class BoosterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeUpText;
    [SerializeField] private TextMeshProUGUI boosterTextBack;
    [SerializeField] private TextMeshProUGUI boosterTextFront;
    [SerializeField] private TextMeshProUGUI noLifeText;
    [SerializeField] private TextMeshProUGUI gameOverText;

    [Header("References Scripts"), Space]
    [SerializeField] private LivesGoblinController livesGoblinController;

    private void OnEnable()
    {
        GameBridge.OnExtraTimeBooster += ShowBoosterText;
        GameBridge.OnTimeUp += ShowTimeUpText;
        GameBridge.OnPlayerQuit += ShowGameOverText;
        livesGoblinController.NoLifes += ShowNoLifeText;
        GameBridge.OnGameClosed += Reset;
    }

    private void OnDisable()
    {
        GameBridge.OnExtraTimeBooster -= ShowBoosterText;
        GameBridge.OnTimeUp -= ShowTimeUpText;
        GameBridge.OnPlayerQuit -= ShowGameOverText;
        livesGoblinController.NoLifes -= ShowNoLifeText;
        GameBridge.OnGameClosed -= Reset;
    }

    private void Start()
    {
        Reset();
    }

    private void ShowTimeUpText()
    {
        StartCoroutine(DelayShowTimeUpText());
    }

    private IEnumerator DelayShowTimeUpText()
    {
        yield return new WaitUntil(() => !TweenChecker.IsTweenPlaying(nameof(GoblinAnimations.GoblinMove)));
        timeUpText.gameObject.SetActive(false);
        timeUpText.gameObject.SetActive(true);
        AudioController.Instance.StopBgm();
        AudioController.Instance.PlaySound(nameof(AudioIds.timeUp));
        yield return new WaitForSeconds(1f);
        livesGoblinController.GoblinLeftBonus();
    }

    private void ShowNoLifeText()
    {
        noLifeText.gameObject.SetActive(false);
        noLifeText.gameObject.SetActive(true);
        StartCoroutine(DelayGameOver());
    }

    private void ShowGameOverText()
    {
        gameOverText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(true);
        StartCoroutine(DelayGameOver());
    }

    private IEnumerator DelayGameOver()
    {
        yield return new WaitForSeconds(3f);
        GameBridge.instance.GoToResults();
    }

    private void ShowBoosterText(string typeBooster)
    {
        boosterTextBack.gameObject.SetActive(false);
        string[] words = typeBooster.Split('-');
        string firstWord = words[0];
        string secondWord = words[1];
        string hexColor = "#FFFFFF";

        switch (secondWord)
        {
            case "Time!":
                hexColor = "#FFD147";
                break;
            case "Goblins!":
                hexColor = "#81EE6B";
                break;
            case "Sight!":
                hexColor = "#E23965";
                break;
            default:
                break;
        }
        boosterTextBack.text = $"<size=70>{firstWord}</size>\n{secondWord}";
        boosterTextFront.text = $"<size=70><color=white>{firstWord}</color></size>\n<color={hexColor}>{secondWord}</color>";
        boosterTextBack.gameObject.SetActive(true);
    }

    private void Reset()
    {
        timeUpText.gameObject.SetActive(false);
        boosterTextBack.gameObject.SetActive(false);
        noLifeText.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(false);
    }
}
