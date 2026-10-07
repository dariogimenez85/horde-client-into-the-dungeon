using System;
using UnityEngine;

namespace Controllers
{
    public class AudioController : MonoBehaviour
    {
        public static AudioController Instance;

        public AudioList[] audioList;

        [SerializeField] private AudioSource sourceFx;
        [SerializeField] private AudioSource sourceFxIncremental;
        [SerializeField] private AudioSource sourceBgm;

        private float pitchStepIncrement = 0.1f;
        private float maxPitch = 3f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            GameBridge.OnGameClosed += StopBgm;    
        }
        
        private void OnDisable()
        {
            GameBridge.OnGameClosed -= StopBgm;
        }

        public void DisableSound()
        {
            sourceFx.mute = true;
            sourceBgm.mute = true;
            sourceFxIncremental.mute = true;
        }

        public void EnableSound()
        {
            sourceFx.mute = false;
            sourceBgm.mute = false;
            sourceFxIncremental.mute = false;
        }

        public void PlaySound(string nameClip)
        {
            AudioList s = Array.Find(audioList, sound => sound.audioIds.ToString() == nameClip);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + nameClip + " not found!", gameObject);
                return;
            }

            sourceFx.PlayOneShot(s.clip);
        }

        public void PlaySoundIncremental(string name)
        {
            AudioList s = Array.Find(audioList, sound => sound.audioIds.ToString() == name);
            if (s == null)
            {
                Debug.LogWarning("Sound: " + name + " not found!", gameObject);
                sourceFxIncremental.pitch = 1;
                return;
            }

            sourceFxIncremental.PlayOneShot(s.clip);
            if (sourceFx.pitch < maxPitch) sourceFxIncremental.pitch += pitchStepIncrement;
        }

        public void PlayBgm()
        {
            sourceBgm.Play();
        }

        public void StopBgm()
        {
            sourceBgm.Stop();
        }

        public void ResetPitchIncremental()
        {
            sourceFxIncremental.pitch = 1;
        }
    }
}