using SO_Scripts;
using TMPro;
using UnityEngine;

namespace Controllers
{
    public class ScoreController : MonoBehaviour
    {
        [Header("References Scriptable"), Space] [SerializeField]
        private GameStateData stateData;


        [Header("References UI"), Space] [SerializeField]
        private TextMeshProUGUI scoreText;

        [SerializeField] private ScoreUIRef scoreUIRef;


        [Header("Scripts References"), Space] [SerializeField]
        private PlayerController playerController;

        [SerializeField] private LivesGoblinController livesGoblin;

        private int maxScore = 0;
        private int increaseScore = 0;

        private void OnEnable()
        {
            stateData.OnUpdateScore += UpdateScore;
            playerController.onCellScore += IncreaseScore;
            GameBridge.OnGameStart += SetInitialScore;
            GameBridge.OnResyncView += SyncScore;
            livesGoblin.SetBonus += SetFinalScore;
            GameBridge.OnGameClosed += Reset;
        }

        private void OnDisable()
        {
            stateData.OnUpdateScore -= UpdateScore;
            playerController.onCellScore -= IncreaseScore;
            GameBridge.OnGameStart -= SetInitialScore;
            GameBridge.OnResyncView -= SyncScore;
            livesGoblin.SetBonus -= SetFinalScore;
            GameBridge.OnGameClosed -= Reset;
        }

        private void SetInitialScore()
        {
            int initialScore = stateData.currentStateData.score;
            scoreUIRef.SetInitialScore(scoreText, initialScore);
        }

        // Resincronización: el puntaje del estado nuevo, sin animar.
        private void SyncScore()
        {
            int score = stateData.currentStateData.score;
            maxScore = score;
            scoreUIRef.SetInitialScore(scoreText, score);
        }

        private void SetFinalScore(int val)
        {
            maxScore = GameBridge.instance.StagingInfo.GetInt("score");
            IncreaseScore(val);
        }

        private void UpdateScore(int newTotalScore)
        {
            maxScore = newTotalScore;
        }

        private void IncreaseScore(int amount)
        {
            increaseScore += amount;
            scoreUIRef.UpdateScoreUI(scoreText, increaseScore, maxScore);
        }

        private void Reset()
        {
            maxScore = 0;
            scoreUIRef.ResetUI(scoreText);
        }
    }
}