
using Controllers;
using UnityEngine;

public class ShowGoblinSound : MonoBehaviour
{
    private void OnEnable()
    {
        AudioController.Instance.PlaySound(AudioIds.showGoblinFx.ToString());
        AudioController.Instance.PlaySoundIncremental(AudioIds.smileGoblin.ToString());
    }
}
