

using DG.Tweening;
using UnityEngine;


public class ScreenShake : MonoBehaviour
{
    [SerializeField] private float power = 0.7f;
    [SerializeField] private float duration = 1.0f;
    [SerializeField] private int vibrato = 10;
    [SerializeField] private float randomness = 90f;

    [Header("Scripts References")]
    [SerializeField] private PlayerController playerController;

    // [SerializeField] private Transform batsEffects;
    // private bool moveZ = false;
    // private Vector3 batPos;

    private void OnEnable()
    {
        // batPos = batsEffects.position;
        playerController.OnShake += DoShake;
    }

    private void OnDisable()
    {
        playerController.OnShake -= DoShake;
    }

    public void DoShake()
    {
        transform.DOShakePosition(duration, power, vibrato, randomness, false, true, ShakeRandomnessMode.Harmonic);
        // batsEffects.transform.position = moveZ ? batPos : new Vector3(batPos.x, batPos.y, 1);
        // moveZ = !moveZ;
    }
}