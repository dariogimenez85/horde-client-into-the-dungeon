using DG.Tweening;
using TMPro;
using UnityEngine;

namespace SO_Scripts
{
    [CreateAssetMenu(fileName = "ScoreUIRef", menuName = "Scriptable's/ScoreUIRef")]
    public class ScoreUIRef : ScriptableObject
    {
        // Estado simple
        private int currentScore = 0;

        // Tweens separados para evitar que completar uno afecte al otro
        private Tweener numberTween;
        private Tweener scaleTween;

        private Vector3 baseScale;
        private bool baseScaleCaptured = false;

        public void SetInitialScore(TextMeshProUGUI scoreText, int initialScore)
        {
            currentScore = initialScore;
            scoreText.text = currentScore.ToString();
        }
        
        public void UpdateScoreUI(TextMeshProUGUI scoreText, int increase, int newScore)
        {
            if (!baseScaleCaptured)
            {
                baseScale = scoreText.transform.localScale;
                baseScaleCaptured = true;
            }

            // Cancelar tween de número sin completar (no queremos saltar al final)
            if (numberTween != null && numberTween.IsActive())
                numberTween.Kill(false);

            // Completar tween de escala para que regrese a su estado original
            if (scaleTween != null && scaleTween.IsActive())
                scaleTween.Kill(true);

            // Garantizar que partimos de la escala base
            scoreText.transform.localScale = baseScale;

            // Calcular target sin pasarse
            int targetScore = Mathf.Min(currentScore + increase, newScore);
            if (targetScore <= currentScore)
                return;

            // Punch de escala independiente
            scaleTween = scoreText.transform
                .DOPunchScale(new Vector3(0.5f, 0.5f, 0.5f), 0.25f)
                .SetEase(Ease.OutBounce);

            // Tween del número (simple, de current -> target)
            numberTween = DOTween
                .To(() => currentScore,
                    x =>
                    {
                        currentScore = x;
                        scoreText.text = currentScore.ToString();
                    },
                    targetScore,
                    0.5f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => numberTween = null);
        }

        /// <summary>
        /// Reinicia visualmente el score a 0.
        /// </summary>
        public void ResetUI(TextMeshProUGUI scoreText)
        {
            numberTween?.Kill();
            scaleTween?.Kill();
            scoreText.text = "0";
        }
    }
}