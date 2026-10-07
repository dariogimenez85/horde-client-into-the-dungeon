using System.Collections;
using Controllers;
using DG.Tweening;
using UnityEngine;

namespace Art.Shaders
{
    public class CurtainRevealController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private GameObject curtainRevealPrefab;

        [SerializeField] private UIStartGameAnimation uiStartGameAnim;

        [SerializeField] private Material material; // El material con el shader que tiene _Progress
        [SerializeField] private float duration = 1f; // Duración del tween (editable en el Inspector)
        [SerializeField] private float targetValue = 0f; // Valor final del progreso (editable en el Inspector)
        [SerializeField] private float startValue = 1f; // valor inicial del progress


        [Header("GamePlay Objects"), Space]
        [SerializeField]
        private GameObject goblinPrefab;

        [SerializeField] private GameObject tresaurePrefab;
        [SerializeField] private GridMatrix gridMatrixSO;
        [SerializeField] private GameParseData gameParseData;


        private void OnEnable()
        {
            GameBridge.OnGameStart += CheckinData;
            goblinPrefab.GetComponent<PlayerController>().OnGoblinDead += CheckinData;
            GameBridge.OnGameClosed += Reset;
        }

        private void OnDisable()
        {
            GameBridge.OnGameStart -= CheckinData;
            goblinPrefab.GetComponent<PlayerController>().OnGoblinDead -= CheckinData;
            GameBridge.OnGameClosed -= Reset;
        }

        private void CheckinData()
        {
            StartCoroutine(WaitForData());
        }

        private IEnumerator WaitForData()
        {
            yield return new WaitUntil(() => GameBridge.instance.StagingInfo != null);
            AnimateProgress();
        }

        private void AnimateProgress()
        {
            if (material == null)
            {
                Debug.LogWarning("Material no asignado.", this);
                return;
            }

            // se ejecuta la primera vez cuando la cortina esta activa
            if (curtainRevealPrefab.activeSelf)
            {
                float current = material.GetFloat("_Progress");
                DOTween.To(() => current, x =>
                    {
                        current = x;
                        material.SetFloat("_Progress", current);
                    }, targetValue, duration).SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        // animamos la ui
                        uiStartGameAnim.AnimateUIObjects();
                        // desactivamos el objecto
                        curtainRevealPrefab.SetActive(false);
                        // siempre debe iniciar en 1
                        material.SetFloat("_Progress", startValue);
                        // asignamos las posiciones de goblin y tesoro
                        SettingGoblinAndTreasure();
                    });
            }
            else
            {
                // se ejecuta la segunda vez cuando la cortina esta inactiva 
                // sucede al haber muerto el goblin o al recoger el tesoro
                if (gameParseData.GetGameStageGoblinData() > 0) SettingGoblinAndTreasure();
            }
        }

        private void SettingGoblinAndTreasure()
        {
            Debug.Log("Goblin y Tesoro Instanciados");

            Vector2Int ppIndex = gameParseData.GetGameInitialPositionPlayer();
            goblinPrefab.transform.position = gridMatrixSO.GetPosition(ppIndex.x, ppIndex.y).position;

            print(ppIndex + "XXX GOBLIN START POSITION");

            goblinPrefab.transform.GetChild(2).gameObject.SetActive(true);
            goblinPrefab.SetActive(true);

            Transform goblinTransform = goblinPrefab.transform;

            if (!goblinTransform.GetChild(0).gameObject.activeSelf)
                goblinTransform.GetChild(0).gameObject.SetActive(true);

            if (goblinTransform.GetChild(1).gameObject.activeSelf)
                goblinTransform.GetChild(1).gameObject.SetActive(false);


            Vector2Int tresaureIndex = gameParseData.GetGameTresaurePosition();
            tresaurePrefab.transform.position = gridMatrixSO.GetPosition(tresaureIndex.x, tresaureIndex.y).position;
            tresaurePrefab.SetActive(true);
            
            GameBridge.instance.ShowUIWrapper(); 
        }
        
        private void Reset()
        {
            curtainRevealPrefab.SetActive(true);
            goblinPrefab.SetActive(false);
            tresaurePrefab.SetActive(false);
            material.SetFloat("_Progress", startValue);
        }

    }
}