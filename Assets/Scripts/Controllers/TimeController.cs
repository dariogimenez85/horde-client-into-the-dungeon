using SO_Scripts;
using TMPro;
using UnityEngine;

namespace Controllers
{
    public class TimeController : MonoBehaviour
    {
        [SerializeField] private GameParseData gameParseData;

        [Header("Scriptable Time Reference"), Space]
        [SerializeField] private TimeUIRef soTimeUIRef;
        [SerializeField] private TextMeshProUGUI timeText;

        private float currentTime;
        private bool isTimerRunning = false;
        private float lastRealTime;

        private void Awake()
        {
            soTimeUIRef.Initialize(timeText);
        }

        private void OnEnable()
        {
            timeText.transform.localScale = new Vector3(-1, 1, 1);
            GameBridge.OnUpdateTime += SettingTimer;
            GameBridge.OnGameOver += PauseTimer;
            GameBridge.OnGameClosed += ResetTimer;
        }

        private void OnDisable()
        {
            GameBridge.OnUpdateTime -= SettingTimer;
            GameBridge.OnGameOver -= PauseTimer;
            GameBridge.OnGameClosed -= ResetTimer;
        }

        private void Update()
        {
            if (!isTimerRunning) return;

            float realDeltaTime = Time.realtimeSinceStartup - lastRealTime;
            lastRealTime = Time.realtimeSinceStartup;
            currentTime -= realDeltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                UpdateTimerUI();

                isTimerRunning = false;
                print("Countdown finished!");
                return;
            }

            UpdateTimerUI();
        }


        private void UpdateTimerUI(bool isAnimate = false)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);

            string strTime = $"{minutes:00}:{seconds:00}";
            soTimeUIRef.SetTime(strTime, isAnimate);
        }

        private void SettingTimer(int seconds, bool active)
        {
            currentTime = seconds;
            lastRealTime = Time.realtimeSinceStartup;
            if (!gameParseData.GetGameOverData()) isTimerRunning = true;
            if (active) print("Booster Time!"); // create animation for the time booster
            UpdateTimerUI(active);
        }

        private void PauseTimer()
        {
            // AudioManager.instance.StopBgm();
            isTimerRunning = false;
        }

        private void ResetTimer()
        {
            currentTime = 0;
            isTimerRunning = false;
            lastRealTime = 0;
            isTimerRunning = false;
            soTimeUIRef.SetTime("00:00", false);
        }
    }
}